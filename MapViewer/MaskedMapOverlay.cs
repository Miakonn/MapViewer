using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MapViewer {
	public partial class MaskedMap {
		#region Utilities

		public void ClearOverlay() {
			CanvasOverlay.Children.Clear();
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


		#endregion

		#region Elements

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

		public Rectangle OverlayRectangle(Rect rect, Color color) {
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

		public void OverlayText(string message, double x1, double y1, double angle, Color color, string uid) {

			var shape = new TextBlock {
				RenderTransform = new RotateTransform(angle),
				Text = message,
				FontSize = 25,
				Foreground = new SolidColorBrush(color),
				FontWeight = FontWeights.UltraBold,
			};
			Canvas.SetLeft(shape, x1);
			Canvas.SetTop(shape, y1);
			AddOverlayElement(shape, uid);
		}

		#endregion

		
	}
}
