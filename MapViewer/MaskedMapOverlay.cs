using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using MapViewer.Utilities;

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

        public void OverlayPlayer(Point pos, Color color, string text) {
            var brush = new SolidColorBrush(color);
            double size;
            if (PlayerSizeMeter != 0) {
                size = PlayerSizeMeter / ImageScaleMperPix;
            }
            else {
                size = PlayerSizePixel / Scale;
            }

            var shape = new Ellipse {
                Width = size,
                Height = size,
                Fill = brush,
                Opacity = 1.0
            };

            shape.Fill = UiElementUtils.CreateTextBrush(text, size, brush);
            Canvas.SetLeft(shape, pos.X - size / 2.0);
            Canvas.SetTop(shape, pos.Y - size / 2.0);
            string uid = "Player" + "_" + text;
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

			if (rect.Width == 0 && rect.Height == 0) {
				return;
			}

			var thickness = 10 / Scale;
			var shape = new Rectangle {
				Width = rect.Width + 2 * thickness,
				Height = rect.Height + 2 * thickness,
				Stroke = new SolidColorBrush(color),
				StrokeThickness = thickness,
				Opacity = 0.5,
				Uid = uid,
				Visibility = (rect.Size == new Size()) ?  Visibility.Hidden : Visibility.Visible
			};

			Canvas.SetLeft(shape, rect.X - thickness);
			Canvas.SetTop(shape, rect.Y - thickness);
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
			Canvas.SetLeft(shape, 0);
			Canvas.SetTop(shape, 0);
			AddOverlayElement(shape, uid);
		}

		public void OverlayPolygon(PointCollection points, Color color, string uid) {
			var shape = new Polygon {
				Points = points,
				Fill = new SolidColorBrush(color),
				Opacity = 0.4

			};
			Canvas.SetLeft(shape, 0);
			Canvas.SetTop(shape, 0);
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
				var elemCursor = CanvasOverlay.FindElementByUid(PublicCursorUid) as Ellipse;

				var radius = 30 / Scale;
				if (elemCursor == null) {
					OverlayRing(pnt, radius, Colors.Red, PublicCursorUid);
				}
				else {
					elemCursor.Width = 2 * radius;
					elemCursor.Height = 2 * radius;
					elemCursor.StrokeThickness = 10 / Scale;
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
				var thickness = 10 / Scale; 
				Canvas.SetLeft(shape, rect.X - thickness);
				Canvas.SetTop(shape, rect.Y - thickness);
				shape.Width = rect.Width + 2 * thickness;
				shape.Height = rect.Height + 2 * thickness;
				shape.StrokeThickness = thickness;
				shape.Visibility = (rect.Size == new Size()) ? Visibility.Hidden : Visibility.Visible;
			}
		}

        public void UpdatePlayerElementSizes() {
            var shapes = CanvasOverlay.FindElementsByUidPrefix("Player");
            foreach (var element in shapes) {
                if (element is Ellipse ellipse) {
                    var oldSize = ellipse.Width;
                    double newSize = ellipse.Width;
                    if (PlayerSizeMeter != 0) {
                        newSize = PlayerSizeMeter / ImageScaleMperPix;
                    }
                    else if (ellipse.Width * Scale < PlayerSizePixel || ellipse.Width * Scale > PlayerSizePixel) {
                        newSize = PlayerSizePixel / Scale;
                    }

                    if (Math.Abs(newSize - oldSize) > 1.0E-9) {
                        ellipse.Width = newSize;
                        ellipse.Height = newSize;
                        MoveElement(ellipse, new Vector((newSize - oldSize) / 2, (newSize - oldSize) / 2));
                    }
                }
            }
        }

        #endregion


    }
}
