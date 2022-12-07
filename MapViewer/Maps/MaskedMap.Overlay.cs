using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;

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
				StrokeThickness = 10 / ZoomScale,
				Opacity = 0.6
			};

			Canvas.SetLeft(shape, pos.X - radius);
			Canvas.SetTop(shape, pos.Y - radius);
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
        }

        #endregion
    }
}
