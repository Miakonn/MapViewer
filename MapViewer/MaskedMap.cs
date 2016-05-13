using System;
using System.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Image = System.Windows.Controls.Image;
using Path = System.IO.Path;
using Point = System.Windows.Point;

namespace MapViewer {
	public class MaskedMap {
		#region Properties
		private string _imagePath;

		public BitmapImage MapImage;

		public MatrixTransform DisplayTransform { get; set; }

		public BitmapSource BmpMask { get; set; }

		public readonly Canvas CanvasMapMask = new Canvas();

		public Canvas CanvasOverlay = new Canvas();

		private double MaskOpacity { get; set; }

		public float ScreenScaleMMperPix { get; set; }

		public MapData MapData { get; set; }

		public double ScreenScaleMMperM { get; set; }

		public float ImageScaleMperPix {
			get {
				return MapData.ImageScaleMperPix;
			}
			set {
				MapData.ImageScaleMperPix = value;
			}
		}

		public Window ParentWindow { get; set; }

		public bool Linked { get; set; }

		public string ImageFile {
			get { return _imagePath; }
			set {
				_imagePath = value;
				MapImage = new BitmapImage(new Uri(_imagePath));
				MapData = new MapData(CreateFilename(_imagePath, ".xml"));

				Deserialize();

				if (BmpMask == null) {
					BmpMask = new WriteableBitmap((int)MapImage.Width, (int)MapImage.Height, MapImage.DpiX, MapImage.DpiY, PixelFormats.Pbgra32, null);
				}


				ScaleToWindow();
			}
		}

		public BitmapImage Image {
			get { return MapImage; }
		}

		private bool PublicView { get; set; }

		public double Scale {
			get { return DisplayTransform.Matrix.M11; }
		}

		#endregion

		public MaskedMap(bool publicView) {
			PublicView = publicView;
			MaskOpacity = PublicView ? 1.0 : 0.3;
			DisplayTransform = new MatrixTransform(1.0, 0.0, 0.0, 1.0, 1.0, 1.0);

			MapData = new MapData(null);

			Linked = false;

			if (PublicView) {
				var screenWidthMM = float.Parse(ConfigurationManager.AppSettings["PublicScreenWidthMM"]);
				var screenWidthPix = float.Parse(ConfigurationManager.AppSettings["PublicScreenWidthPix"]);
				ScreenScaleMMperPix = screenWidthMM / screenWidthPix;
			}
		}

		public void Draw() {
			// CanvasMapMask
			CanvasMapMask.Children.Clear();
			CanvasMapMask.RenderTransform = DisplayTransform;
			var backgroundImage = new Image {
				Source = MapImage,
			};
			CanvasMapMask.Children.Add(backgroundImage);

			var maskImage = new Image {
				Opacity = MaskOpacity,
				Source = BmpMask
			};
			CanvasMapMask.Children.Add(maskImage);

			// CanvasOverlay
			CanvasOverlay.RenderTransform = DisplayTransform;
		}

		public Rect VisibleRectInMap() {
			var rect = new Rect(0.0, 0.0, CanvasMapMask.ActualWidth, CanvasMapMask.ActualHeight);
	
			var inverse = DisplayTransform.Clone().Inverse;
			if (inverse != null) {
				var rectOut = inverse.TransformBounds(rect);
				return rectOut;
			}
			return new Rect();
		}

		public Point CenterInMap() {
			var pos = new Point(CanvasMapMask.ActualWidth / 2, CanvasMapMask.ActualHeight/ 2);

			var inverse = DisplayTransform.Clone().Inverse;
			if (inverse != null) {
				var posOut = inverse.Transform(pos);
				return posOut;
			}
			return new Point();
		}

		public void RotateClockwise() {
			var mat = DisplayTransform.Matrix;

			var center= CenterInMap();
			mat.RotateAtPrepend(90, center.X, center.Y);
			DisplayTransform.Matrix = mat;
		}

		public void ScaleToWindow() {
			if (!PublicView && MapImage != null) {
				var winSizePix = CanvasMapMask.RenderSize;
				var scale = Math.Min(winSizePix.Width / MapImage.PixelWidth, winSizePix.Height / MapImage.PixelHeight);
				DisplayTransform.Matrix = new Matrix(scale, 0, 0, scale, 0, 0);
			}
		}

		private void ScaleToReal() {
			if (PublicView && MapImage != null) {

				if (MapData.ImageScaleMperPix < 0.005) {
					MessageBox.Show("Image not calibrated");
					return;
				}

				var scale = ScreenScaleMMperM * ImageScaleMperPix / ScreenScaleMMperPix;
				DisplayTransform.Matrix = new Matrix(scale, 0, 0, scale, -MapImage.PixelWidth / 2.0, -MapImage.PixelHeight / 2.0);
			}
		}

		private void ScaleToLinked(MaskedMap mapSource) {
			if (PublicView && MapImage != null) {
				var thisWinSizePix = ParentWindow.RenderSize;
				var otherWinSizePix = mapSource.ParentWindow.RenderSize;

				var scale = Math.Min(thisWinSizePix.Width / otherWinSizePix.Width, thisWinSizePix.Height / otherWinSizePix.Height);

				DisplayTransform = mapSource.DisplayTransform.CloneCurrentValue();

				var matrix = DisplayTransform.Matrix;
				matrix.Scale(scale, scale);
				DisplayTransform.Matrix = matrix;
			}			
		}

		private static int Between(int val, int min, int max) {
			return Math.Max(Math.Min(val, max), min);
		}


		private byte[] CreateColorData(int byteCount, byte opacity) {
			var colorData = new byte[byteCount];
			for (var i = 0; i < byteCount; i += 4) {
				colorData[i] = 0;	// B
				colorData[i + 1] = 0; // G
				colorData[i + 2] = 0; // R
				colorData[i + 3] = opacity; // A
			}
			return colorData;
		}

		public void MaskCircle(int centerX, int centerY, int radius, byte opacity) {
			var bitmap = BmpMask as WriteableBitmap;
			if (bitmap == null) {
				return;
			}

			var y0 = Between(centerY - radius, 0, bitmap.PixelHeight);
			var yMax = Between(centerY + radius, 0, bitmap.PixelHeight);

			var byteCount = 4 * (2 * radius);
			var colorData = CreateColorData(byteCount, opacity);

			for (var y = y0; y < yMax; y++) {
				var corda = (int)Math.Sqrt(radius * radius - (y - centerY) * (y - centerY));
				var x0 = Between(centerX - corda, 0, bitmap.PixelWidth);
				var xMax = Between(centerX + corda, 0, bitmap.PixelWidth);
				var rectLine = new Int32Rect(x0, y, xMax - x0, 1);

				bitmap.WritePixels(rectLine, colorData, rectLine.Width * 4, 0);
			}
		}

		public void MaskRectangle(Int32Rect rect, byte opacity) {
			var bitmap = BmpMask as WriteableBitmap;
			if (bitmap == null) {
				return;
			}

			var x0 = Between(rect.X, 0, bitmap.PixelWidth);
			var y0 = Between(rect.Y, 0, bitmap.PixelHeight);
			var xMax = Between(rect.X + rect.Width, 0, bitmap.PixelWidth);
			var yMax = Between(rect.Y + rect.Height, 0, bitmap.PixelHeight);

			var byteCount = 4 * (xMax - x0);
			var colorData = CreateColorData(byteCount, opacity);

			var rectLine = new Int32Rect(x0, y0, xMax - x0, 1);
			for (var y = y0; y < yMax; y++) {
				rectLine.Y = y;
				bitmap.WritePixels(rectLine, colorData, byteCount, 0);
			}
		}

		public void PublishFrom(MaskedMap mapSource, bool scaleNeedsToRecalculate) {
			if (mapSource.BmpMask != null) {
				BmpMask = mapSource.BmpMask.CloneCurrentValue();
			}
			if (mapSource.MapImage != null) {
				MapImage = mapSource.MapImage.CloneCurrentValue();
			}

			MapData.ImageScaleMperPix = mapSource.MapData.ImageScaleMperPix;
		
			BitmapUtils.CopyingCanvas(mapSource.CanvasOverlay, CanvasOverlay);

			if (Linked) {
				ScaleToLinked(mapSource);
			}
			else if (scaleNeedsToRecalculate) {
				ScaleToReal();
			}
		}

		public void AddOverlayElement(UIElement elem, string uid) {
			int indx = 0;
			do {
				elem.Uid = uid + indx.ToString();
				if (BitmapUtils.FindElementByUid(CanvasOverlay, elem.Uid) == null) {
					CanvasOverlay.Children.Add(elem);
					break;
				}
				indx++;
			} while (true);
		}

		public void OverlayCircle(Point pos, float radius, Color color, string uid) {
			var shape = new Ellipse {
				Width = 2 * radius,
				Height = 2 * radius,
				Fill = new SolidColorBrush(color),
				Opacity = 0.4
			};

			Canvas.SetLeft(shape, pos.X - radius);
			Canvas.SetTop(shape, pos.Y - radius);
			AddOverlayElement(shape, uid);
		}

		public Rectangle OverlayRectPixel(Rect rect, Color color) {
			var shape = new Rectangle {
				Width = rect.Width,
				Height = rect.Height,
				Stroke = new SolidColorBrush(color),
				StrokeThickness = 20,
				Opacity = 0.5,
				Uid = "PublicView"
			};

			Canvas.SetLeft(shape, rect.X);
			Canvas.SetTop(shape, rect.Y);
			CanvasOverlay.Children.Add(shape); 
			return shape;
		}

		public void OverlayLine(double x1, double y1, double x2, double y2, float widthM, Color color, string uid) {
			var size = widthM / ImageScaleMperPix;
			var shape = new Line {
				X1 = x1,
				Y1 = y1,
				X2 = x2,
				Y2 = y2,
				StrokeThickness = size,
				Stroke = new SolidColorBrush(color),
				Opacity = 0.4

			};
			AddOverlayElement(shape, uid);
		}

		public void OverlayPolygon(PointCollection points, Color color, string uid) {
			var shape = new Polygon {
				Points = points,
				Fill = new SolidColorBrush(color),
				Opacity = 0.4

			};
			AddOverlayElement(shape, uid);
		}

		public void ClearMask() {
			if (BmpMask != null) {
				var rect = new Int32Rect(0, 0, BmpMask.PixelWidth, BmpMask.PixelHeight);
				MaskRectangle(rect, 0);
			}
		}

		public void ClearOverlay() {
			CanvasOverlay.Children.Clear();
		}

		public void Zoom(double scale, Point pos) {
			var matrix = DisplayTransform.Matrix;
			matrix.ScaleAt(scale, scale, pos.X, pos.Y); 
			DisplayTransform.Matrix = matrix;

		}

		public void Translate(Vector move) {

			var matrix = DisplayTransform.Matrix;
			var x = (move.X * matrix.M11 + move.Y * matrix.M21) / Scale;
			var y = (move.X * matrix.M12 + move.Y * matrix.M22) / Scale;
			
			matrix.Translate(x, y);
			DisplayTransform.Matrix = matrix;
		}

		public Point TransformPoint(Point pos) {
			pos.X += (int)DisplayTransform.Matrix.OffsetX;
			pos.Y += (int)DisplayTransform.Matrix.OffsetY;
			return pos;
		}

		public void Serialize() {
			MapData.Serialize();
			BmpMask.Freeze();
			BitmapUtils.Serialize(BmpMask as WriteableBitmap, CreateFilename(_imagePath, ".mask"));
			BitmapUtils.SerializeXaml(CanvasOverlay, CreateFilename(_imagePath, ".xaml"));
		}

		public void Deserialize() {
			MapData.Deserialize();
			BmpMask = BitmapUtils.Deserialize(CreateFilename(_imagePath, ".mask"));
			BitmapUtils.DeserializeXaml(CanvasOverlay, CreateFilename(_imagePath, ".xaml"));

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
