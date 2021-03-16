using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MapViewer.Properties;
using Image = System.Windows.Controls.Image;
using Path = System.IO.Path;
using Point = System.Windows.Point;

namespace MapViewer {
	public partial class MaskedMap {
		#region Properties

		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public BitmapImage MapImage;

        public TransformGroup DisplayTransform { get; }


        public bool Initiated = false;


        public double PlayerSizePixel { get; set; } //  == 0 means fixed in meter 
        public double PlayerSizeMeter { get; set; } //  == 0 means dynamic in pixel 

        private readonly RotateTransform _trfRotation;
		public RotateTransform TrfRotation {
			get => _trfRotation;
            set {
				_trfRotation.Angle = value.Angle;
				_trfRotation.CenterX = value.CenterX;
				_trfRotation.CenterY = value.CenterY;
			}
		}

		private readonly ScaleTransform _trfScale;
		public ScaleTransform TrfScale {
			get => _trfScale;
            set {
				_trfScale.ScaleX = value.ScaleX;
				_trfScale.ScaleY = value.ScaleY;
				_trfScale.CenterX = value.CenterX;
				_trfScale.CenterY = value.CenterY;
			}

		}

		private readonly TranslateTransform _trfTranslate;
		public TranslateTransform TrfTranslate {
			get => _trfTranslate;
            set {
				_trfTranslate.X = value.X;
				_trfTranslate.Y = value.Y;
			}
		}

		public BitmapSource BmpMask { get; set; }

		public readonly Canvas CanvasMapMask = new Canvas();

		public Canvas CanvasOverlay = new Canvas();
		private Image _maskImage;
		private Image _backgroundImage;

		private BitmapPalette _maskPalette;

		private double MaskOpacity { get; }

		public MapData MapData { get; set; }

		public double ScreenScaleMMperM { get; set; }

		public bool ShowPublicCursor { get; set; }
		
		public bool ShowPublicCursorTemporary { get; set; }


		public const string PublicCursorUid = "Cursor";
		public const string PublicPositionUid = "PublicPos";


		public Color MaskColor {
			get {
				var colorString = Settings.Default.MaskColor;
				try {
					var color = ColorConverter.ConvertFromString(colorString);
					Log.Info("MaskColor= " + color);
					return (Color?) color ?? Colors.Black;
				}
				catch {
					Log.Error("Failed to parse color: " + colorString);
				}
				return Colors.Black;
			}
		}

        public float ImageScaleMperPix {
			get => MapData.ImageScaleMperPix;
            set => MapData.ImageScaleMperPix = value;
        }

		public string Unit {
			get => MapData.Unit;
            set => MapData.Unit = value;
        }

		public bool IsCalibrated => ImageScaleMperPix > 0.0;

        public Window ParentWindow { get; set; }

		public bool IsLinked { get; set; }

		private bool IsPublic { get; }

        private long GroupId { get; set; }

        public long MapId { get; set; }

		public string ImageFilePath { get; set; }

		public double Scale => MapImage != null ? TrfScale.ScaleX : 1.0;

        public double ScaleDpiFix => MapImage != null ? (MapImage.PixelHeight / MapImage.Height) : 1.0;

        #endregion

		public MaskedMap(bool publicView, Window parent, long groupId) {
			IsPublic = publicView;
            ParentWindow = parent;
            GroupId = groupId;
            MapId = DateTime.Now.Ticks;

			MaskOpacity = IsPublic ? 1.0 : 0.3;
			DisplayTransform = new TransformGroup();

			_trfRotation = new RotateTransform(0.0);
			_trfScale = new ScaleTransform(1.0, 1.0);
			_trfTranslate = new TranslateTransform(0.0, 0.0);

			DisplayTransform.Children.Add(TrfScale);
			DisplayTransform.Children.Add(TrfTranslate);
			DisplayTransform.Children.Add(TrfRotation);

			MapData = new MapData(null);

            PlayerSizeMeter = 0;
            PlayerSizePixel = 20;

			if (!IsPublic) {
				CreatePalette();
			}

			IsLinked = false;
		}

		public void BitmapFromUri(Uri source) {
			MapImage = new BitmapImage();
			MapImage.BeginInit();
			MapImage.UriSource = source;
			MapImage.CacheOption = BitmapCacheOption.OnLoad;
			MapImage.EndInit();
		}

		public void LoadImage(string imagePath) {
			try {
                if (ParentWindow is PrivateWindow privateWindow && !string.Equals(imagePath, ImageFilePath) && !string.IsNullOrWhiteSpace(ImageFilePath)) {
					privateWindow.AddToMru(ImageFilePath);
				}
				ImageFilePath = imagePath;
				Log.InfoFormat("Loading image {0}", ImageFilePath);
				BitmapFromUri(new Uri(ImageFilePath));
				MapData = new MapData(CreateFilename(ImageFilePath, ".xml"));

				BmpMask = null;
				CanvasOverlay.Children.Clear();

				UpdatePublicViewRectangle();

				Deserialize();
				CreatePalette();

				if (MapData.LastFigureScaleUsed != 0) {
					ScreenScaleMMperM = 1000.0 / MapData.LastFigureScaleUsed;
				}

				if (BmpMask == null) {
					BmpMask = new WriteableBitmap(MapImage.PixelWidth + 2, MapImage.PixelHeight + 2, MapImage.DpiX, MapImage.DpiY,
						PixelFormats.Indexed8, _maskPalette);
				}
            }
			catch (Exception ex) {
				Log.Error("LoadImage", ex);
				MessageBox.Show("Failed to load image", ex.Message);
				BmpMask = null;
				MapImage = null;
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}
		}

		public void Create() {
            Initiated = true;

			// CanvasMapMask
			CanvasMapMask.Children.Clear();
			CanvasMapMask.RenderTransform = DisplayTransform;
			_backgroundImage = new Image {
				Source = MapImage,
				Uid = "Map"
			};
			CanvasMapMask.Children.Add(_backgroundImage);

			_maskImage = new Image {
				Opacity = MaskOpacity,
				Source = BmpMask,
				Uid = "Mask"

			};
			CanvasMapMask.Children.Add(_maskImage);

			// CanvasOverlay
			CanvasOverlay.RenderTransform = DisplayTransform;
            UpdatePlayerElementSizes();
		}

		public void CreatePalette() {
			var colors = new List<Color> {Colors.Transparent};
			var color = MaskColor;

			for (var i = 1; i <= 255; i++) {
				colors.Add(color);
			}
			_maskPalette = new BitmapPalette(colors);
		}

		public void PublishFrom(MaskedMap mapSource, bool scaleNeedsToRecalculate) {
			Log.InfoFormat("Publish : scaleNeedsToRecalculate={0}", scaleNeedsToRecalculate);

			Unit = mapSource.Unit;
			var newImageLoaded = MapId != mapSource.MapId;
            var newGroupLoaded = GroupId != mapSource.GroupId;

			if (mapSource.BmpMask != null && _maskImage != null) {
				try {
					BmpMask = mapSource.BmpMask.CloneCurrentValue();
				}
				catch (Exception ex) {
					Log.Error(ex.Message);
					MessageBox.Show(ex.Message);
				}
				_maskImage.Source = BmpMask;
			}
            if (mapSource.MapImage != null) {
                if (newImageLoaded) {
                    MapImage = mapSource.MapImage.CloneCurrentValue();
                    _backgroundImage.Source = MapImage;
                    ImageFilePath = mapSource.ImageFilePath;
                    mapSource.CanvasOverlay.CopyingCanvas(CanvasOverlay);
                    MapId = mapSource.MapId;
                }

                if (newGroupLoaded) {
                    ClearTransformExceptRotation();
                    GroupId = mapSource.GroupId;
                }
            }

            MapData.ImageScaleMperPix = mapSource.MapData.ImageScaleMperPix;

			if (IsLinked) {
				ScaleToLinked(mapSource);
			}
			else if (scaleNeedsToRecalculate) {
				ScaleToReal();
			}
		}
        
        public void CopyTransform(MaskedMap source) {
            TrfTranslate = source.TrfTranslate;
            TrfRotation = source.TrfRotation;
            TrfScale = source.TrfScale;
        }

        private void ClearTransformExceptRotation() {
			TrfScale.ScaleX = 1;
			TrfScale.ScaleY = 1;
			TrfScale.CenterX = 0;
			TrfScale.CenterY = 0;
			TrfTranslate.X = 0;
			TrfTranslate.Y = 0;
		}

		public Rect VisibleRectInMap() {
			var rect = new Rect(0.0, 0.0, CanvasMapMask.ActualWidth, CanvasMapMask.ActualHeight);

			var inverse = DisplayTransform.Inverse;
			if (inverse != null) {
				var rectOut = inverse.TransformBounds(rect);
				return rectOut;
			}
			return new Rect();
		}

        public Point CenterInMap() {
            var pos = new Point(CanvasMapMask.ActualWidth / 2, CanvasMapMask.ActualHeight / 2);
            var inverse = DisplayTransform.Inverse;
            if (inverse != null) {
                var posOut = inverse.Transform(pos);
                return posOut;
            }
            return new Point(); 
        }

        public Point CenterInCanvas() {
            var pos = new Point(CanvasMapMask.ActualWidth / 2, CanvasMapMask.ActualHeight / 2);
            return pos;
        }


        public Point GetPosInMap(Point pos) {
            var inverse = DisplayTransform.Inverse;
            if (inverse != null) {
                var posOut = inverse.Transform(pos);
                return posOut;
            }
            return new Point();
        }
        
        public void RotateClockwise() {
			TrfRotation.Angle += 90.0;
			var winSizePix = CanvasMapMask.RenderSize;
			TrfRotation.CenterX = winSizePix.Width / 2;
			TrfRotation.CenterY = winSizePix.Height / 2;
		}

        public void ScaleToWindow(UIElement element) {
            if (IsPublic || MapImage == null) {
                return;
            }

            var scale = GetMinScale(element);
            TrfScale.ScaleX = scale;
            TrfScale.ScaleY = scale;
            TrfTranslate.X = 0;
            TrfTranslate.Y = 0;
            UpdatePublicViewRectangle();
        }


        public double GetMinScale(UIElement element) {
            if (element == null || element.RenderSize.Width == 0) {
                return Math.Min(500 / MapImage.Width, 500 / MapImage.Height);
            }
            return Math.Min(element.RenderSize.Width / MapImage.Width, element.RenderSize.Height / MapImage.Height);
        }

		public void Zoom(double scale, Point pos) {
            var posCenterBefore = CenterInMap();
            TrfScale.ScaleX *= scale;
			TrfScale.ScaleY *= scale;

            if (TrfScale.ScaleX < GetMinScale(CanvasMapMask)) {
                ScaleToWindow(CanvasMapMask);
                UpdatePlayerElementSizes();
            }
            else {
                TrfTranslate = new TranslateTransform();
                var offs = CenterInMap() - posCenterBefore;
                UpdatePlayerElementSizes();

                TrfTranslate = new TranslateTransform(offs.X * TrfScale.ScaleX, offs.Y * TrfScale.ScaleY);
            }
            //UpdatePublicViewRectangle();
 		}

		public void Translate(Vector move) {
			TrfTranslate.X += move.X;
			TrfTranslate.Y += move.Y;
		}

		private void UpdatePublicViewRectangle() {
            if (ParentWindow is PrivateWindow privateWin && privateWin.MapPrivate != null) {
                bool equalImages = !string.Equals(ImageFilePath, privateWin.MapPrivate.ImageFilePath);

                if (!IsPublic && !IsLinked && equalImages) {
                    UpdateVisibleRectangle(privateWin.MapPublic.VisibleRectInMap());
                }
            }
        }


		public void DumpDisplayTransform(string label) {
			foreach (var trf in DisplayTransform.Children) {
				Log.DebugFormat("{0}: Matrix={1}", label, trf.Value);
			}

			Log.DebugFormat("{0}: Value={1}", label, DisplayTransform.Value);			
		}

		private void ScaleToReal() {
			if (IsPublic && MapImage != null) {
				Log.DebugFormat("ScaleToReal {0} {1}", ScreenScaleMMperM, ImageScaleMperPix);

				if (MapData.ImageScaleMperPix < 1E-15) {
					MessageBox.Show("Image not calibrated!");
					return;
				}

                if (ParentWindow is PublicWindow publicWindow) {	
					var cp = new Point(CanvasOverlay.ActualWidth / 2, CanvasOverlay.ActualHeight / 2);
					Point center;
					if ((TrfScale.Value.IsIdentity  &&  TrfTranslate.Value.IsIdentity) || !DisplayTransform.Value.HasInverse) {
						center = new Point(MapImage.Width/2, MapImage.Height/2);
					}
					else {
						// ReSharper disable once PossibleNullReferenceException
						center = DisplayTransform.Inverse.Transform(cp);
					}
					Log.DebugFormat("ScaleToReal1 MonitorScaleMMperPixel={0}", publicWindow.MonitorScaleMMperPixel);
					Log.DebugFormat("ScaleToReal2 CanvasOverlay.ActualWidth={0} MapImage.Width={1}", CanvasOverlay.ActualWidth, MapImage.Width);
					Log.DebugFormat("ScaleToReal3 center={0} ", center);

					var scale = ScreenScaleMMperM * ImageScaleMperPix /  publicWindow.MonitorScaleMMperPixel;

					TrfScale.CenterX =0;
					TrfScale.CenterY = 0;
					TrfScale.ScaleX = scale;
					TrfScale.ScaleY = scale;
					TrfTranslate.X = (CanvasOverlay.ActualWidth / 2) - scale * center.X;
					TrfTranslate.Y = (CanvasOverlay.ActualHeight / 2) - scale * center.Y;
				}
			}
		}

		private void ScaleToLinked(MaskedMap mapSource) {
			if (IsPublic && MapImage != null) {

				var privateWindow = mapSource.ParentWindow as PrivateWindow;
				if (privateWindow == null) {
					return;
				}
				var privateWidth = privateWindow.DrawingSpace.ActualWidth;
				var privateHeight = privateWindow.DrawingSpace.ActualHeight;
				var privateAspectRatio = privateWidth/privateHeight;

				var publicWinSizePix = ParentWindow.RenderSize;
				var publicAspectRatio = publicWinSizePix.Width / publicWinSizePix.Height;

				Log.DebugFormat("ScaleToLinked private aspect ratio={0}", privateAspectRatio);
				Log.DebugFormat("ScaleToLinked public aspect ratio={0}", publicAspectRatio);

				if (privateAspectRatio > publicAspectRatio) {
					publicWinSizePix.Height = publicWinSizePix.Width / privateAspectRatio;
				}
				else {
					publicWinSizePix.Width = publicWinSizePix.Height * privateAspectRatio;
				}

				var scale = Math.Min(publicWinSizePix.Width / privateWidth, publicWinSizePix.Height / privateHeight);
				Log.DebugFormat("ScaleToLinked public scale={0}", scale);

                TrfScale = mapSource.TrfScale;
				TrfScale.ScaleX *= scale;
				TrfScale.ScaleY *= scale;

                TrfTranslate = new TranslateTransform();
                var offs = CenterInMap() - mapSource.CenterInMap();
                TrfTranslate = new TranslateTransform(offs.X * TrfScale.ScaleX, offs.Y * TrfScale.ScaleY);

                CanvasMapMask.RenderTransform = DisplayTransform;
			}
		}

		private static int Between(int val, int min, int max) {
			return Math.Max(Math.Min(val, max), min);
		}

		private static byte[] CreateColorData(int byteCount, byte colorIndex) {
			var colorData = new byte[byteCount];
			for (var i = 0; i < byteCount; i ++) {
				colorData[i] = colorIndex;	// B
			}
			return colorData;
		}

		public void MaskCircle(int centerX, int centerY, int radius, byte colorIndex) {

			centerX = (int)(centerX * ScaleDpiFix);
			centerY = (int)(centerY * ScaleDpiFix);
			radius = (int)(radius * ScaleDpiFix);

			var bitmap = BmpMask as WriteableBitmap;
			if (bitmap == null) {
				return;
			}

			var y0 = Between(centerY - radius, 0, bitmap.PixelHeight);
			var yMax = Between(centerY + radius, 0, bitmap.PixelHeight);

			var byteCount = (2 * radius);
			var colorData = CreateColorData(byteCount, colorIndex);

			for (var y = y0; y < yMax; y++) {
				var corda = (int)Math.Sqrt(radius * radius - (y - centerY) * (y - centerY));
				var x0 = Between(centerX - corda, 0, bitmap.PixelWidth);
				var xMax = Between(centerX + corda, 0, bitmap.PixelWidth);
				var rectLine = new Int32Rect(x0, y, xMax - x0, 1);

				bitmap.WritePixels(rectLine, colorData, rectLine.Width, 0);
			}
		}

		public void MaskRectangle(Int32Rect rect, byte colorIndex) {
			rect.X = (int)(rect.X * ScaleDpiFix);
			rect.Y = (int)(rect.Y * ScaleDpiFix);
			rect.Width = (int)(rect.Width * ScaleDpiFix);
			rect.Height = (int)(rect.Height * ScaleDpiFix);

			var bitmap = BmpMask as WriteableBitmap;
			if (bitmap == null) {
				return;
			}

			var x0 = Between(rect.X, 0, bitmap.PixelWidth);
			var y0 = Between(rect.Y, 0, bitmap.PixelHeight);
			var xMax = Between(rect.X + rect.Width, 0, bitmap.PixelWidth);
			var yMax = Between(rect.Y + rect.Height, 0, bitmap.PixelHeight);

			var byteCount = (xMax - x0);
			var colorData = CreateColorData(byteCount, colorIndex);

			var rectLine = new Int32Rect(x0, y0, xMax - x0, 1);
			for (var y = y0; y < yMax; y++) {
				rectLine.Y = y;
				bitmap.WritePixels(rectLine, colorData, byteCount, 0);
			}
		}

		public void MaskPolygon(PointCollection pnts, byte colorIndex) {
			var bitmap = BmpMask as WriteableBitmap;
			if (bitmap == null) {
				return;
			}

			var intPoints = new int[pnts.Count * 2];
			for (var i = 0; i < pnts.Count; i++) {
				intPoints[i * 2] = (int)(ScaleDpiFix * pnts[i].X);
				intPoints[i * 2 + 1] = (int)(ScaleDpiFix * pnts[i].Y);
			}

			bitmap.FillPolygon(intPoints, colorIndex);	
		}

		public static Color GetPixelColor(BitmapSource bitmap, int x, int y) {
			if (x < 0 || x >= bitmap.PixelWidth || y < 0 || y >= bitmap.PixelHeight) {
				return Colors.Black;
			}

			const int side = 1;
			var bytesPerPixel = (bitmap.Format.BitsPerPixel + 7) / 8;
			var stride = side * bytesPerPixel;
			var bytes = new byte[side * side * bytesPerPixel];
			var rect = new Int32Rect(x, y, side, side);

			bitmap.CopyPixels(rect, bytes, stride, 0);

			if (bitmap.Format == PixelFormats.Pbgra32) {
				return Color.FromArgb(bytes[3], bytes[2], bytes[1], bytes[0]);
			}
			if (bitmap.Format == PixelFormats.Bgr32) {
				return Color.FromArgb(0xFF, bytes[2], bytes[1], bytes[0]);
			}
			// handle other required formats
			return Colors.Black;
		}

		const int DotRadius = 2;
		const int DotSize = DotRadius * 2 + 1;

		public static void WritePixel(WriteableBitmap bitmap, int x, int y, byte[] colorArr) {

			if (x - DotRadius < 0 || y - DotRadius < 0 || x + DotRadius >= bitmap.PixelWidth || y + DotRadius >= bitmap.PixelHeight) {
				return;
			}
			var rect = new Int32Rect(x-DotRadius, y-DotRadius, DotSize, DotSize);
			bitmap.WritePixels(rect, colorArr, rect.Width, 0);
		}

		public void MaskLineOfSight(double centerX, double centerY, double radius, byte colorIndex) {
			centerX = (int)(centerX * ScaleDpiFix);
			centerY = (int)(centerY * ScaleDpiFix);
			radius = (int)(radius * ScaleDpiFix);

			var bitmap = BmpMask as WriteableBitmap;
			if (bitmap == null) {
				return;
			}

			var colorData = CreateColorData(DotSize * DotSize, colorIndex);
			for (var angle = 0.0; angle <= 2 * Math.PI; angle+=0.005) {
				for (var rad = 0.0; rad <= radius; rad += 4.0) {
					var pntX = (int)(centerX + Math.Cos(angle) * rad);
					var pntY = (int)(centerY + Math.Sin(angle) * rad);
					var col = GetPixelColor(MapImage, pntX, pntY);
					if ((col.R + col.G + col.B) < 128) {
						break;
					}
					WritePixel(bitmap, pntX, pntY, colorData);
				}
			}
		}

		public void ClearMask() {
			if (BmpMask != null) {
				var rect = new Int32Rect(0, 0, (int)MapImage.Width, (int)MapImage.Height);
				MaskRectangle(rect, 0);
			}
		}

		public void Serialize() {
			MapData.Serialize();
			BitmapUtils.Serialize(BmpMask as WriteableBitmap, CreateFilename(ImageFilePath, ".mask.png"));
			CanvasOverlay.SerializeXaml(CreateFilename(ImageFilePath, ".xaml"));
		}

		public void Deserialize() {
			MapData.Deserialize();
			BmpMask = BitmapUtils.Deserialize(CreateFilename(ImageFilePath, ".mask.png"));
			CanvasOverlay.DeserializeXaml(CreateFilename(ImageFilePath, ".xaml"));
			var shape = CanvasOverlay.FindElementByUid(PublicPositionUid);
			if (shape != null) {
				CanvasOverlay.Children.Remove(shape);
			}
		}




		private const string FolderName = "MapViewerFiles";

		private static string CreateFilename(string original, string extension) {
			var originalFolder = Path.GetDirectoryName(original);
			var originalFilename = Path.GetFileName(original);

			if (originalFolder != null) {
				var folder = Path.Combine(originalFolder, FolderName);
				if (!Directory.Exists(folder)) {
					try {
						Directory.CreateDirectory(folder);
					}
					catch (Exception ex) {
						MessageBox.Show(ex.Message);
						return "";
					}
				}

				return Path.Combine(folder, originalFilename + extension);
			}
			return "";
		}
	}
}
