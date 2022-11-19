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

        #endregion

        #region Elements

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


        
		public void RemoveElement(string uid) {
            RemoveElement(CanvasOverlay.FindElementByUid(uid));
		}

        public void RemoveElement(UIElement elem) {
            if (elem == null) {
                return;
            }
            CanvasOverlay.Children.Remove(elem);
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
           
            // TODO
            
        }

        #endregion


    }
}
