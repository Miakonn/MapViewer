using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls.Ribbon;

namespace MapViewer.Tools {
	public class DrawSquare : ICanvasTool {

		private readonly PrivateWindow _privateWindow;
		private readonly Canvas _canvas;
		private readonly MaskedMap _map;
		private RibbonToggleButton _button;
		private Polygon _shape;

		private Point _pnt1;
		private Point _pnt2;

		public DrawSquare(PrivateWindow privateWindow, object button) {
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
				UpdateDraw(e.GetPosition(_canvas));
				EndDraw();
			}
		}

		public void MouseMove(object sender, MouseEventArgs e) {
			if (_shape == null) {
				return;
			} UpdateDraw(e.GetPosition(_canvas));
			_privateWindow.DisplayPopup(string.Format("side={0} m", CalculateDistance()));
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
			_privateWindow.HidePopup();
		}

		#endregion

		private void InitDraw(Point pt1) {
			_pnt1 = pt1;
			_pnt2 = pt1;

			_shape = new Polygon {
				Points = CreatePointCollection(),
				Fill = Brushes.Green,
				Opacity = 0.5
			};

			_canvas.Children.Add(_shape);

		}

		private void UpdateDraw(Point pt2) {
			_pnt2 = pt2;
			if (_shape == null) {
				return;
			}

			_shape.Points = CreatePointCollection();
		}


		private PointCollection CreatePointCollection() {
			var points = new PointCollection(4);
			var vector = new Vector(_pnt1.X - _pnt2.X, _pnt1.Y - _pnt2.Y);
			var vectorPerp = new Vector(vector.Y, -vector.X);

			points.Add(_pnt1 + vector);
			points.Add(_pnt1 + vectorPerp);
			points.Add(_pnt1 - vector);
			points.Add(_pnt1 - vectorPerp);

			return points;
		}


		private string CalculateDistance() {
			if (_shape == null) {
				return "0.0";
			}
			var length = 1.414 * new Vector(_pnt1.X - _pnt2.X, _pnt1.Y - _pnt2.Y).Length;
			var dist = _map.ImageScaleMperPix * length;
			return dist.ToString("N1");
		}

		private void EndDraw() {
			var points = CreatePointCollection();
			_map.OverlayPolygon(points, Colors.Green, "Rect");
			if (_privateWindow.PublicWindow.IsVisible) {
				_privateWindow.MapPublic.OverlayPolygon(points, Colors.Green, "Rect");
			}
			_privateWindow.ActiveTool = null;
		}

	}
}
