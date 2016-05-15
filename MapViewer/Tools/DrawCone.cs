using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls.Ribbon;

namespace MapViewer.Tools {
	class DrawCone : ICanvasTool {

		private readonly PrivateWindow _privateWindow;
		private readonly Canvas _canvas;
		private readonly MaskedMap _map;
		private RibbonToggleButton _button;
		private Polygon _shape;
		private Point _pnt1;
		private Point _pnt2;

		public DrawCone(PrivateWindow privateWindow, object button) {
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
				InitDraw(e.GetPosition(_canvas));
			}
			else {
				UpdateDraw(e.GetPosition(_canvas));
				EndDraw();
			}
		}

		public void MouseMove(object sender, MouseEventArgs e) {
			if (_shape == null) {
				return;
			}
			UpdateDraw(e.GetPosition(_canvas));
			_privateWindow.DisplayPopup(string.Format(CalculateDistance() + " m"));
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
			var points = new PointCollection(3);
			var vector = new Vector(_pnt1.X - _pnt2.X, _pnt1.Y - _pnt2.Y);
			var vectorPerp = new Vector(vector.Y / 2, -vector.X / 2);

			points.Add(_pnt1);
			points.Add(_pnt2 + vectorPerp);
			points.Add(_pnt2 - vectorPerp);

			return points;
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
			var points = CreatePointCollection();
			_map.OverlayPolygon(points, Colors.Green, "Cone");
			if (_privateWindow.PublicWindow.IsVisible) {
				_privateWindow.MapPublic.OverlayPolygon(points, Colors.Green, "Cone");
			}

			_privateWindow.ActiveTool = null;
		}

	}
}
