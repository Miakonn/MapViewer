using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls.Ribbon;

namespace MapViewer.Tools {
	public class MaskCircle : CanvasTool {
		private readonly Canvas _canvas;
		private readonly Maps.PrivateMaskedMap _map;
		private readonly bool _mask;
		private RibbonToggleButton _button;

		private Ellipse _circle;

		private Point _pnt1;

		public MaskCircle(PrivateWindow privateWindow, object button, bool mask) {
			_mask = mask;
			_map = privateWindow.MapPrivate;
			_canvas = _map.CanvasOverlay;
			_button = (RibbonToggleButton)button;
            _circle = null;
        }

		#region CanvasTool

        public override void MouseDown(object sender, MouseButtonEventArgs e) {
			if (_circle == null) {
				_pnt1 = e.GetPosition(_canvas);
				InitDraw(_pnt1);
			}
			else {
				UpdateDraw(_pnt1, e.GetPosition(_canvas));
				EndDraw();
			}
		}

		public override void MouseMove(object sender, MouseEventArgs e) {
			UpdateDraw(_pnt1, e.GetPosition(_canvas));
		}

	
		public override void Deactivate() {
			if (_circle != null) {
				_canvas.Children.Remove(_circle);
			}
			_circle = null;

			if (_button != null) {
				_button.IsChecked = false;
			}
			_button = null;
		}

        #endregion

		private void InitDraw(Point pt1) {
			_pnt1 = pt1;
		
			_circle = new Ellipse {
				Width = 1,
				Height = 1,
				Fill = new SolidColorBrush(_mask ? WritableBitmapUtils.MaskColor : Colors.White),
				Opacity = 0.5
			};

			Canvas.SetLeft(_circle, pt1.X);
			Canvas.SetTop(_circle, pt1.Y);
			_canvas.Children.Add(_circle);

		}

		private void UpdateDraw(Point pt1, Point pt2) {
			if (_circle == null) {
				return;
			}

			var radius = new Vector(pt1.X - pt2.X, pt1.Y - pt2.Y).Length;
			var x = pt1.X - radius;
			var y = pt1.Y - radius;
			Canvas.SetLeft(_circle, x);
			Canvas.SetTop(_circle, y);
			_circle.Width = 2 * radius;
			_circle.Height = 2 * radius;
		}

		private void EndDraw() {
			var center = GetElementCenter(_circle);
			var radius = (int) (_circle.ActualWidth / 2);
			_map.MaskCircle((int)center.X, (int)center.Y, radius, (byte)(_mask ? 3 : 0));
			_canvas.Children.Remove(_circle);
			_circle = null;
		}

		private Point GetElementCenter(FrameworkElement element) {
			var transform = element.TransformToVisual(_map.CanvasMap);
			return transform.Transform(new Point(element.ActualWidth / 2.0, element.ActualHeight / 2.0));
		}
	}
}
