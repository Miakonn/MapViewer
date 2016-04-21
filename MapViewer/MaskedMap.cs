using System;
using System.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;
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

		public float ScreenScaleMMperM { get; set; }

		public float ImageScaleMperPix { get; set; }

		public Window ParentWindow { get; set; }

		public bool Linked { get; set; }

		public string ImageFile {
			get { return _imagePath; }
			set {
				_imagePath = value;
				MapImage = new BitmapImage(new Uri(_imagePath));
				BmpMask = new WriteableBitmap((int)MapImage.Width, (int)MapImage.Height, MapImage.DpiX, MapImage.DpiY, PixelFormats.Pbgra32, null);

				MapData = new MapData(_imagePath);
				MapData.Deserialize();

				ScaleToWindow();
			}
		}

		public BitmapImage Image {
			get { return MapImage; }
		}

		private bool PublicView { get; set; }

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
				ScreenScaleMMperM = float.Parse(ConfigurationManager.AppSettings["PublicScreenScaleMMperM"]);
			}
		}

		public void Draw() {
			CanvasMapMask.Children.Clear();

			var backgroundImage = new Image {
				RenderTransform = DisplayTransform,
				RenderTransformOrigin = new Point(0.0, 0.0),
				Margin = new Thickness(0, 0, 0, 0),
				Source = MapImage,
			};

			CanvasMapMask.Background = new ImageBrush();

			CanvasMapMask.Children.Add(backgroundImage);

			var maskImage = new Image {
				RenderTransform = DisplayTransform,
				RenderTransformOrigin = new Point(0.0, 0.0),
				Margin = new Thickness(0, 0, 0, 0),
				Opacity = MaskOpacity,
				Source = BmpMask
			};


			CanvasMapMask.Children.Add(maskImage);
		}

		public void ScaleToWindow() {
			if (!PublicView && MapImage != null) {
				var winSizePix = ParentWindow.RenderSize;
				var scale = Math.Min(winSizePix.Width / MapImage.PixelWidth, winSizePix.Height / MapImage.PixelHeight);
				DisplayTransform.Matrix = new Matrix(scale, 0, 0, scale, 0, 0);
			}
		}

		private void ScaleToReal() {
			if (PublicView && MapImage != null) {

				if (MapData.ImageLengthM < 0.5) {
					MessageBox.Show("Image not calibrated");
					return;
				}

				ImageScaleMperPix = MapData.ImageLengthM / MapImage.PixelWidth;
				var scale = ScreenScaleMMperM * ImageScaleMperPix / ScreenScaleMMperPix;
				DisplayTransform.Matrix = new Matrix(scale, 0, 0, scale, 0, 0);
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

		public void RenderRectangle(Int32Rect rect, byte opacity) {
			var bitmap = BmpMask as WriteableBitmap;
			if (bitmap == null) {
				return;
			}

			var scale = DisplayTransform.Matrix.M11;
			var x0 = (int)Math.Round((rect.X  - DisplayTransform.Matrix.OffsetX)/ scale);
			x0 = Between(x0, 0, bitmap.PixelWidth);
			var y0 = (int)Math.Round((rect.Y - DisplayTransform.Matrix.OffsetY)/ scale);
			y0 = Between(y0, 0, bitmap.PixelHeight);
			var xMax = x0 + (int)Math.Round(rect.Width / scale);
			xMax = Between(xMax, 0, bitmap.PixelWidth);
			var yMax = y0 + (int)Math.Round(rect.Height / scale);
			yMax = Between(yMax, 0, bitmap.PixelHeight);
			var byteCount = 4 * (xMax - x0);

			var colorData = new byte[byteCount];
			for (var i = 0; i < byteCount; i += 4) {
				colorData[i] = 0;	// B
				colorData[i + 1] = 0; // G
				colorData[i + 2] = 0; // R
				colorData[i + 3] = opacity; // A
			}

			var rectLine = new Int32Rect(x0, y0, xMax - x0, 1);
			for (var y = y0; y < yMax; y++) {
				rectLine.Y = y;
				bitmap.WritePixels(rectLine, colorData, byteCount, 0);
			}
		}

		public void PublishFrom(MaskedMap mapSource) {
			if (mapSource.BmpMask != null) {
				BmpMask = mapSource.BmpMask.CloneCurrentValue();
			}
			if (mapSource.MapImage != null) {
				MapImage = mapSource.MapImage.CloneCurrentValue();
			}

			MapData.ImageLengthM = mapSource.MapData.ImageLengthM;

			if (Linked) {
				ScaleToLinked(mapSource);
			}
			else {
				ScaleToReal();
			}

		}

		public void ClearMask() {
			if (BmpMask != null) {
				var rect = new Int32Rect(0, 0, BmpMask.PixelWidth, BmpMask.PixelHeight);
				RenderRectangle(rect, 0);
			}
		}

		public void Zoom(double scale, Point pnt) {
			var matrix = DisplayTransform.Matrix;
			matrix.Scale(scale, scale); 
			DisplayTransform.Matrix = matrix;

		}

		public void Translate(Vector move) {
			var matrix = DisplayTransform.Matrix;
			matrix.Translate(move.X, move.Y);
			DisplayTransform.Matrix = matrix;
		}

		public Point TransformPoint(Point pos) {
			pos.X += (int)DisplayTransform.Matrix.OffsetX;
			pos.Y += (int)DisplayTransform.Matrix.OffsetY;
			return pos;
		}

	}
}
