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
			var indx = 0;
			do {
				elem.Uid = uid + indx;
				if (CanvasOverlay.FindElementByUid(elem.Uid) == null) {
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

		public void OverlayRectangle(Rect rect, Color color, string uid) {
			var shape = new Rectangle {
				Width = rect.Width,
				Height = rect.Height,
				Stroke = new SolidColorBrush(color),
				StrokeThickness = 10 / Scale,
				Opacity = 0.5,
				Uid = uid
			};

			Canvas.SetLeft(shape, rect.X);
			Canvas.SetTop(shape, rect.Y);
			CanvasOverlay.Children.Add(shape);
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

		public void MovePublicCursor(Point pnt) {
			var elemCursor = CanvasOverlay.FindElementByUid(MaskedMap.PublicCursorUid);

			var size = 30 / Scale;

			if (elemCursor == null) {
				elemCursor = new Ellipse {
					Width = size * 2,
					Height = size * 2,
					Stroke = new SolidColorBrush(Colors.Red),
					StrokeThickness = (5 / Scale),
					Opacity = 0.6,
					Uid = MaskedMap.PublicCursorUid
				};
				Canvas.SetLeft(elemCursor, pnt.X - size);
				Canvas.SetTop(elemCursor, pnt.Y - size);

				CanvasOverlay.Children.Add(elemCursor);
			}
			else {
				Canvas.SetLeft(elemCursor, pnt.X - size);
				Canvas.SetTop(elemCursor, pnt.Y - size);
			}
		}

		public void DeleteShape(string uid) {
			var shape = CanvasOverlay.FindElementByUid(uid);
			if (shape != null) {
				CanvasOverlay.Children.Remove(shape);
			}
		}

		public void UpdateVisibleRectangle(Rect rect) {
			var shape = (Rectangle)CanvasOverlay.FindElementByUid(MaskedMap.PublicPositionUid);
			if (shape == null) {
				OverlayRectangle(rect, Colors.Red, MaskedMap.PublicPositionUid);
			}
			else {
				Canvas.SetLeft(shape, rect.X);
				Canvas.SetTop(shape, rect.Y);
				shape.Width = rect.Width;
				shape.Height = rect.Height;
				shape.StrokeThickness = 10 / Scale;
			}
		}



		#endregion

		
	}
}
