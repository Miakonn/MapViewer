using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;

namespace MapViewer
{
    internal class Map
    {
        private BitmapImage _backgroundImage;
        private Transform _displayTransform = new ScaleTransform(1.0, 1.0);
        private string _imagePath;


		private WriteableBitmap _bmpMaskMain;
        private BitmapSource _bmpMaskSide;


        public string ImageFile { 
            get { return _imagePath;  }
            set {
                _imagePath = value;
                _backgroundImage = new BitmapImage(new Uri(_imagePath));
				_bmpMaskMain = new WriteableBitmap((int)_backgroundImage.Width, (int)_backgroundImage.Height, 96, 96, PixelFormats.Pbgra32, null);
            }
        }

        public int Transparency { get; set; }

        public BitmapImage Image {
            get { return _backgroundImage;  }
        }

        public void DrawMain(Canvas canvas)
        {
            var backgroundImage = new Image {
                RenderTransform = _displayTransform,
                RenderTransformOrigin = new Point(0.5, 0.5),
                Margin = new Thickness(0, 0, 0, 0),
                Source = _backgroundImage,
            };

            canvas.Background = new ImageBrush();
            
            canvas.Children.Add(backgroundImage);

            var maskImage = new Image {
                RenderTransform = _displayTransform,
                RenderTransformOrigin = new Point(0.5, 0.5),
                Margin = new Thickness(0, 0, 0, 0),
                Opacity = 0.3,
                Source = _bmpMaskMain
            };


            canvas.Children.Add(maskImage);
        }

        public void DrawSide(Canvas canvas)
        {
            var backgroundImage = new Image {
                RenderTransform = _displayTransform,
                RenderTransformOrigin = new Point(0.5, 0.5),
                Margin = new Thickness(0, 0, 0, 0),
                Source = _backgroundImage,
            };

            canvas.Background = new ImageBrush();
            canvas.Children.Add(backgroundImage);

            var maskImage = new Image {
                RenderTransform = _displayTransform,
                RenderTransformOrigin = new Point(0.5, 0.5),
                Margin = new Thickness(0, 0, 0, 0),
                Opacity = 1.0,
                Source = _bmpMaskSide
            };

            canvas.Children.Add(maskImage);
        }

		public void RenderRectangle(Int32Rect rect, byte opacity) {
			var width = rect.Width;
			var x = rect.X;
			var byteCount = 4*width;

			var colorData = new byte[byteCount];
			for (var i = 0; i < byteCount; i += 4) {
				colorData[i] = 0;	// B
				colorData[i+1] = 0; // G
				colorData[i+2] = 0; // R
				colorData[i+3] = opacity; // A
			}

			var rectLine = new Int32Rect(x, rect.Y, width, 1);
			for (var y = rect.Y; y < rect.Y + rect.Height; y++) {
				rectLine.Y = y;
				_bmpMaskMain.WritePixels(rectLine, colorData, 4 * width, 0);
			}
		}

        public void Publish() {
            _bmpMaskSide = _bmpMaskMain.CloneCurrentValue();
        }

		public void ClearMask() {
			var rect = new Int32Rect(0, 0, _bmpMaskMain.PixelWidth, _bmpMaskMain.PixelHeight);
			RenderRectangle(rect, 0);
		}

    }
}
