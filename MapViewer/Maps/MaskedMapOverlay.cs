using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;
using Size = System.Windows.Size;

namespace MapViewer.Maps {
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
				elem.Uid = uid + "#" + indx;
				if (CanvasOverlay.FindElementByUid(elem.Uid) == null) {
					CanvasOverlay.Children.Add(elem);
					break;
				}
				indx++;
			} while (true);
		}

        
        public UIElement FindElement(string uid) {
            return CanvasOverlay.FindElementByUid(uid);
        }

        #endregion

        #region Elements

        public virtual void MoveElement(UIElement elem, Vector move)
        {
            Canvas.SetLeft(elem, Canvas.GetLeft(elem) - move.X);
            Canvas.SetTop(elem, Canvas.GetTop(elem) - move.Y);
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

        
		public void RemoveElement(string uid) {
            RemoveElement(CanvasOverlay.FindElementByUid(uid));
		}

        public void RemoveElement(UIElement elem) {
            if (elem == null) {
                return;
            }
            CanvasOverlay.Children.Remove(elem);
            if (elem.IsPlayer()) {
                RemoveElement(CanvasOverlay.GetPlayerNameElement(elem));
            }
        }

        public UIElement GetPlayerNamElement(UIElement elem) {
            return CanvasOverlay.GetPlayerNameElement(elem);
        }

        public void UpdateVisibleRectangle(Rect rect) {
			var shape = (Rectangle)CanvasOverlay.FindElementByUid(MaskedMap.PublicPositionUid);
	
			if (shape == null) {
				OverlayRectangle(rect, Colors.Red, MaskedMap.PublicPositionUid);
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
            if (Scale == 0) {
                return;
            }
            var elemPlayers = CanvasOverlay.FindPlayerElements();
            
            foreach (var ellipse in elemPlayers) {
                TextBlock textBlock = CanvasOverlay.GetPlayerNameElement(ellipse);
                if (textBlock != null) {
                    // Resize name
                    var oldFontSize = textBlock.FontSize;
                    double newFontSize = textBlock.FontSize;

                    if (PlayerSizeMeter != 0) {
                        newFontSize = 20 / Scale;
                    }
                    else if (textBlock.FontSize * Scale < PlayerSizePixel || textBlock.FontSize * Scale > PlayerSizePixel) {
                        newFontSize = PlayerSizePixel / Scale;
                    }

                    if (Math.Abs(newFontSize - oldFontSize) > 1.0E-9) {
                        textBlock.FontSize = newFontSize;
                    }
                }

                double oldSize = ellipse.Width;
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
                }
                MoveElement(ellipse, new Vector((newSize - oldSize) / 2, (newSize - oldSize) / 2));
            }
            
        }

        #endregion


    }
}
