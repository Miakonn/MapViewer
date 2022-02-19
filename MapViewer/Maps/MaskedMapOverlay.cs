using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

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

		public void MoveElement(UIElement elem, Vector move) {
			Canvas.SetLeft(elem, Canvas.GetLeft(elem) - move.X);
			Canvas.SetTop(elem, Canvas.GetTop(elem) - move.Y);

            if (!elem.IsPlayer()) {
                return;
            }

            TextBlock elemName = CanvasOverlay.GetPlayerNameElement(elem);
            if (elemName != null && elem is Ellipse elemPlayer) {
                var centerX = Canvas.GetLeft(elem) + elemPlayer.Width / 2;
                var centerY = Canvas.GetTop(elem) + elemPlayer.Height / 2;
                Canvas.SetLeft(elemName, centerX - elemName.ActualWidth / 2);
                Canvas.SetTop(elemName, centerY - elemName.ActualHeight / 2);
            }
        }

		public void MoveElement(string uid, Vector move) {
			var elem = CanvasOverlay.FindElementByUid(uid);
			if (elem != null) {
				MoveElement(elem, move);
			}
		}

        public void SendElementToBack(string uid) {
            var elem = CanvasOverlay.FindElementByUid(uid);
            if (elem == null) {
                return;
            }
            if (elem.IsPlayer()) {
                SendElementToBack(uid + ".name");
            }
            CanvasOverlay.Children.Remove(elem);
            CanvasOverlay.Children.Insert(0, elem);
         }
        
        public UIElement FindElement(string uid) {
            return CanvasOverlay.FindElementByUid(uid);
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

        public void CreateOverlayPlayer(Point pos, Color color, string text) {
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

            Canvas.SetLeft(shape, pos.X - size / 2.0);
            Canvas.SetTop(shape, pos.Y - size / 2.0);
            string uid = "Player" + "_" + text;
            AddOverlayElement(shape, uid);

            double fontSize;
            if (PlayerSizeMeter != 0) {
                fontSize = 20 / Scale;
            }
            else {
                fontSize = PlayerSizePixel / Scale;
            }
 
            var textBlock = new TextBlock {
                Text = text,
                FontSize = fontSize,
                Foreground = new SolidColorBrush(Colors.Black),
                FontWeight = FontWeights.Normal,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(textBlock, pos.X - textBlock.ActualWidth / 2.0);
            Canvas.SetTop(textBlock, pos.Y - textBlock.ActualHeight / 2.0);
            AddOverlayElement(textBlock, uid + ".name");
            MoveElement(shape, new Vector());
        }

        public void CreateOverlayPlayerNew(Point pos, Color color, string text)
        {
            var brush = new SolidColorBrush(color);
            double size;
            if (PlayerSizeMeter != 0) {
                size = PlayerSizeMeter / ImageScaleMperPix;
            }
            else {
                size = PlayerSizePixel / Scale;
            }

            var symbol = new PlayerSymbol {
                Uid = "Player" + "_" + text,
                Color = color,
                Text = text,
                Layer = 50,
                Position = pos,
                Size = size
            };

            AddSymbol(symbol);
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



		public void RemoveElement(string uid) {
			var shape = CanvasOverlay.FindElementByUid(uid);
			if (shape != null) {
				CanvasOverlay.Children.Remove(shape);
			}
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
