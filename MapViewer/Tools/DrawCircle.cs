using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls.Ribbon;

namespace MapViewer.Tools {
	public class DrawCircle : ICanvasTool {

		private readonly PrivateWindow _privateWindow;
		private readonly Canvas _canvas;
		private readonly MaskedMap _map;
		private RibbonToggleButton _button;
		private Ellipse _shape;

		private Point _pnt1;
		private Point _pnt2;

		public DrawCircle(PrivateWindow privateWindow, object button) {
			_privateWindow = privateWindow;
			_map = privateWindow.MapPrivate;
			_canvas = _map.CanvasOverlay;
			_button = (RibbonToggleButton)button;
		}


		#region ICanvasTool
		public void Activate() {
			_shape = null;
		}

		public void MouseDown(object sender, MouseButtonEventArgs e) {
			if (_shape == null) {
				_pnt1 = e.GetPosition(_canvas);
				InitDraw(_pnt1);
			}
			else {
				UpdateDraw(_pnt1, e.GetPosition(_canvas));
				EndDraw();
			}
		}

		public void MouseMove(object sender, MouseEventArgs e) {
			if (_shape == null) {
				return;
			}
			UpdateDraw(_pnt1, e.GetPosition(_canvas));
			_privateWindow.DisplayPopup(string.Format("r={0} {1}", CalculateDistance(), _map.Unit));
		}

		public void MouseUp(object sender, MouseButtonEventArgs e) { }

		public void KeyDown(object sender, KeyEventArgs e) { }

		public void Deactivate() {
			if (_shape != null) {
				_canvas.Children.Remove(_shape);
			}
			_shape = null;

			if (_button != null) {
				_button.IsChecked = false;
			}
			_button = null;
			_privateWindow.HidePopup(3);

		}

		public bool ShowPublicCursor() {
			return true;
		}
		#endregion

		private void InitDraw(Point pt1) {
			_pnt1 = pt1;

			_shape = new Ellipse {
				Width = 1,
				Height = 1,
				Fill = new SolidColorBrush(Colors.Blue),
				Opacity = 0.5
			};

			Canvas.SetLeft(_shape, pt1.X);
			Canvas.SetTop(_shape, pt1.Y);
			_canvas.Children.Add(_shape);

		}

		private void UpdateDraw(Point pt1, Point pt2) {
			if (_shape == null) {
				return;
			}

			_pnt2 = pt2;
			var radius = new Vector(pt1.X - pt2.X, pt1.Y - pt2.Y).Length;
			var x = pt1.X - radius;
			var y = pt1.Y - radius;
			Canvas.SetLeft(_shape, x);
			Canvas.SetTop(_shape, y);
			_shape.Width = 2 * radius;
			_shape.Height = 2 * radius;
		}

		private string CalculateDistance() {
			if (_shape == null) {
				return "0.0";
			}
			var length = new Vector(_pnt1.X - _pnt2.X, _pnt1.Y - _pnt2.Y).Length;
			var dist = _map.ImageScaleMperPix * length;
			return dist.ToString("N1");
		}

		private void EndDraw() {
			var center = GetElementCenter(_shape);
			var radius = (int)(_shape.ActualWidth / 2);
			_map.OverlayCircle(center, radius, Colors.Blue, "Circle");
			if (_privateWindow.PublicWindow.IsVisible) {
				_privateWindow.MapPublic.OverlayCircle(center, radius, Colors.Blue, "Circle");
			}
			_privateWindow.ActiveTool = null;
		}

		private Point GetElementCenter(FrameworkElement element) {
			var transform = element.TransformToVisual(_map.CanvasMapMask);
			return transform.Transform(new Point(element.ActualWidth / 2.0, element.ActualHeight / 2.0));
		}
	}
}
