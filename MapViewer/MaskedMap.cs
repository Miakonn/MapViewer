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

		private readonly TransformGroup _displayTransform;
		public TransformGroup DisplayTransform {
			get { return _displayTransform; }
		}

		private readonly RotateTransform _trfRotation;
		public RotateTransform TrfRotation {
			get { return _trfRotation; }
			set {
				_trfRotation.Angle = value.Angle;
				_trfRotation.CenterX = value.CenterX;
				_trfRotation.CenterY = value.CenterY;
			}
		}

		private readonly ScaleTransform _trfScale;
		public ScaleTransform TrfScale {
			get { return _trfScale; }
			set {
				_trfScale.ScaleX = value.ScaleX;
				_trfScale.ScaleY = value.ScaleY;
				_trfScale.CenterX = value.CenterX;
				_trfScale.CenterY = value.CenterY;
			}

		}

		private readonly TranslateTransform _trfTranslate;
		public TranslateTransform TrfTranslate {
			get { return _trfTranslate; }
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

		private double MaskOpacity { get; set; }

		public float ScreenScaleMMperPix { get; set; }

		public MapData MapData { get; set; }

		public double ScreenScaleMMperM { get; set; }

		public bool ShowPublicCursor { get; set; }

		public const string PublicCursorUid = "Cursor";
		public const string PublicPositionUid = "PublicPos";


		public Color MaskColor {
			get {
				var colorString = Settings.Default.MaskColor;
				try {
					var color = ColorConverter.ConvertFromString(colorString);
					Log.Error("MaskColor= " + color);
					return (Color?) color ?? Colors.Black;
				}
				catch (Exception ex) {
					Log.Error("Failed to parse color: " + colorString);
				}
				return Colors.Black;
			}
		}


		public float ImageScaleMperPix {
			get {
				return MapData.ImageScaleMperPix;
			}
			set {
				MapData.ImageScaleMperPix = value;
			}
		}

		public string Unit {
			get {
				return MapData.Unit;
			}
			set {
				MapData.Unit = value;
			}
		}

		public bool IsCalibrated {
			get { return ImageScaleMperPix > 0.0; } 
		}

		public Window ParentWindow { get; set; }

		public bool IsLinked { get; set; }

		private bool IsPublic { get; set; }

		public string ImageFilePath { get; set; }

		public double Scale {
			get {
				return MapImage != null ? TrfScale.ScaleX : 1.0;
			}
		}

		public double ScaleDpiFix {
			get { return MapImage != null ? (MapImage.PixelHeight / MapImage.Height) : 1.0; }
		}

		#endregion

		public MaskedMap(bool publicView) {
			IsPublic = publicView;
			MaskOpacity = IsPublic ? 1.0 : 0.3;
			_displayTransform = new TransformGroup();

			_trfRotation = new RotateTransform(0.0);
			_trfScale = new ScaleTransform(1.0, 1.0);
			_trfTranslate = new TranslateTransform(0.0, 0.0);

			DisplayTransform.Children.Add(TrfScale);
			DisplayTransform.Children.Add(TrfTranslate);
			DisplayTransform.Children.Add(TrfRotation);

			MapData = new MapData(null);

			if (!IsPublic) {
				CreatePalette();
			}

			IsLinked = false;
		}

		public static BitmapImage BitmapFromUri(Uri source) {
			var bitmap = new BitmapImage();
			bitmap.BeginInit();
			bitmap.UriSource = source;
			bitmap.CacheOption = BitmapCacheOption.OnLoad;
			bitmap.EndInit();
			return bitmap;
		}

		public void LoadImage(string imagePath) {
			var privateWindow = ParentWindow as PrivateWindow;
			if (privateWindow != null && !string.Equals(imagePath, ImageFilePath) && !string.IsNullOrWhiteSpace(ImageFilePath)) {
				privateWindow.AddToMru(ImageFilePath);
			}
			ImageFilePath = imagePath;
			Log.InfoFormat("Loading image {0}", ImageFilePath);
			MapImage = BitmapFromUri(new Uri(ImageFilePath));
			MapData = new MapData(CreateFilename(ImageFilePath, ".xml"));

			BmpMask = null;
			CanvasOverlay.Children.Clear();

			UpdatePublicViewRectangle();

			Deserialize();
			CreatePalette();

			if (BmpMask == null) {
				BmpMask = new WriteableBitmap(MapImage.PixelWidth, MapImage.PixelHeight, MapImage.DpiX, MapImage.DpiY,
					PixelFormats.Indexed8, _maskPalette);
			}

			ScaleToWindow();			
		}

		public void Create() {

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
			var changeImage = !string.Equals(ImageFilePath, mapSource.ImageFilePath);

			if (mapSource.BmpMask != null && _maskImage != null) {
				BmpMask = mapSource.BmpMask.CloneCurrentValue();
				_maskImage.Source = BmpMask;
			}
			if (mapSource.MapImage != null && changeImage) {
				MapImage = mapSource.MapImage.CloneCurrentValue();
				_backgroundImage.Source = MapImage;

				ImageFilePath = mapSource.ImageFilePath;
				mapSource.CanvasOverlay.CopyingCanvas(CanvasOverlay);

				ClearTransformExceptRotation();
			}


			MapData.ImageScaleMperPix = mapSource.MapData.ImageScaleMperPix;


			if (IsLinked) {
				ScaleToLinked(mapSource);
			}
			else if (scaleNeedsToRecalculate) {
				ScaleToReal();
			}
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
			var pos = new Point(CanvasMapMask.ActualWidth / 2, CanvasMapMask.ActualHeight/ 2);

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

		public void ScaleToWindow() {
			if (IsPublic || MapImage == null) {
				return;
			}

			var winSizePix = CanvasMapMask.RenderSize;
			var scale = Math.Min(winSizePix.Width / MapImage.Width, winSizePix.Height / MapImage.Height);
			TrfScale.ScaleX = scale;
			TrfScale.ScaleY = scale;
			TrfTranslate.X = 0;
			TrfTranslate.Y = 0;
			UpdatePublicViewRectangle();
		}

		public void Zoom(double scale, Point pos) {
			TrfScale.ScaleX *= scale;
			TrfScale.ScaleY *= scale;
			//TrfScale.CenterX = pos.X;
			//TrfScale.CenterY = pos.Y;

			UpdatePublicViewRectangle();
		}

		public void Translate(Vector move) {
			TrfTranslate.X += move.X;
			TrfTranslate.Y += move.Y;
		}

		private void UpdatePublicViewRectangle() {
			var privateWin = ParentWindow as PrivateWindow;
			if (!IsPublic && !IsLinked && privateWin != null && !string.Equals(ImageFilePath, privateWin.MapPrivate.ImageFilePath)) {
				UpdateVisibleRectangle(privateWin.MapPublic.VisibleRectInMap());
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

				var publicWindow = ParentWindow as PublicWindow;
				
				if (publicWindow != null) {	
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

				TrfTranslate = mapSource.TrfTranslate;


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
