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

			elem.Uid = uid;
			if (CanvasOverlay.FindElementByUid(elem.Uid) == null) {
				CanvasOverlay.Children.Add(elem);
				return;
			}

			var indx = 1;
			do {
				elem.Uid = uid + indx;
				if (CanvasOverlay.FindElementByUid(elem.Uid) == null) {
					CanvasOverlay.Children.Add(elem);
					break;
				}
				indx++;
			} while (true);
		}

		public void MoveElement(UIElement elem, Vector move) {
			Canvas.SetLeft(elem, Canvas.GetLeft(elem) - move.X);
			Canvas.SetTop(elem, Canvas.GetTop(elem) - move.Y);
		}

		public void MoveElement(string uid, Vector move) {
			var elem = CanvasOverlay.FindElementByUid(uid);
			if (elem != null) {
				MoveElement(elem, move);
			}
		}

		#endregion

		#region Elements

		public void OverlayCircle(Point pos, double radius, Color color, string uid) {
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

		public void OverlayRing(Point pos, double radius, Color color, string uid) {
			var shape = new Ellipse {
				Width = 2 * radius,
				Height = 2 * radius,
				Stroke = new SolidColorBrush(color),
				StrokeThickness = 10 / Scale,
				Opacity = 0.6
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

		public void OverlayLine(double x1, double y1, double x2, double y2, float width, Color color, string uid) {
			var shape = new Line {
				X1 = x1,
				Y1 = y1,
				X2 = x2,
				Y2 = y2,
				StrokeThickness = width,
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
			if (ShowPublicCursor) {
				var elemCursor = CanvasOverlay.FindElementByUid(PublicCursorUid);

				var radius = 30 / Scale;
				if (elemCursor == null) {
					OverlayRing(pnt, radius, Colors.Red, PublicCursorUid);
				}
				else {
					Canvas.SetLeft(elemCursor, pnt.X - radius);
					Canvas.SetTop(elemCursor, pnt.Y - radius);
				}
			}
			else {
				DeleteShape(PublicCursorUid);
			}
		}

		public void DeleteShape(string uid) {
			var shape = CanvasOverlay.FindElementByUid(uid);
			if (shape != null) {
				CanvasOverlay.Children.Remove(shape);
			}
		}

		public void UpdateVisibleRectangle(Rect rect) {
			var shape = (Rectangle)CanvasOverlay.FindElementByUid(PublicPositionUid);
			if (shape == null) {
				OverlayRectangle(rect, Colors.Red, PublicPositionUid);
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
