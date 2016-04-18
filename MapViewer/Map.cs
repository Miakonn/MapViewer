﻿using System;
using System.Reflection;
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
        private string _imagePath;

        public  BitmapImage BackgroundImage;
		public MatrixTransform DisplayTransform { get; set; }
	    public BitmapSource BmpMask { get; set; }

	    public readonly Canvas CanvasMapMask = new Canvas();
		public Canvas CanvasOverlay = new Canvas();

		private double MaskOpacity { get; set; }

        public string ImageFile { 
            get { return _imagePath;  }
            set {
                _imagePath = value;
                BackgroundImage = new BitmapImage(new Uri(_imagePath));
				BmpMask = new WriteableBitmap((int)BackgroundImage.Width, (int)BackgroundImage.Height, 96, 96, PixelFormats.Pbgra32, null);
            }
        }

        public BitmapImage Image {
            get { return BackgroundImage;  }
        }

		private bool PublicView { get; set; }

		public Map(bool publicView) {
			PublicView = publicView;
			MaskOpacity = PublicView ? 1.0 : 0.3;
			DisplayTransform = new MatrixTransform(1.0, 0.0, 0.0, 1.0, 1.0, 1.0);
		}


        public void Draw()
        {
			CanvasMapMask.Children.Clear();

            var backgroundImage = new Image {
                RenderTransform = DisplayTransform,
                RenderTransformOrigin = new Point(0.5, 0.5),
                Margin = new Thickness(0, 0, 0, 0),
                Source = BackgroundImage,
            };

            CanvasMapMask.Background = new ImageBrush();

			CanvasMapMask.Children.Add(backgroundImage);

            var maskImage = new Image {
                RenderTransform = DisplayTransform,
                RenderTransformOrigin = new Point(0.5, 0.5),
                Margin = new Thickness(0, 0, 0, 0),
                Opacity = MaskOpacity,
                Source = BmpMask
            };


			CanvasMapMask.Children.Add(maskImage);
        }

		public void RenderRectangle(Int32Rect rect, byte opacity) {
			var bitmap = BmpMask as WriteableBitmap;
			if (bitmap == null) {
				return;
			}

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
				bitmap.WritePixels(rectLine, colorData, 4 * width, 0);
			}
		}

        public void Publish(Map mapSource) {
            BmpMask = mapSource.BmpMask.CloneCurrentValue();
	        BackgroundImage = mapSource.BackgroundImage.CloneCurrentValue();
	        DisplayTransform = mapSource.DisplayTransform.CloneCurrentValue();
			Zoom(2.0, new Point(0,0));
        }

		public void ClearMask() {
			var rect = new Int32Rect(0, 0, BmpMask.PixelWidth, BmpMask.PixelHeight);
			RenderRectangle(rect, 0);
		}

	    public void Zoom(double scale, Point pnt) {
		    var matrix = DisplayTransform.Matrix;
			matrix.ScaleAtPrepend(scale, scale, pnt.X, pnt.Y);
		    DisplayTransform.Matrix = matrix;

	    }

	    public void Translate(Vector move) {
			var matrix = DisplayTransform.Matrix;
			matrix.Translate(move.X, move.Y);
			DisplayTransform.Matrix = matrix;
	    }


    }
}
