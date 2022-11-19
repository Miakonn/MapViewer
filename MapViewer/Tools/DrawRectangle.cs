using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls.Ribbon;

namespace MapViewer.Tools {
	class DrawRectangle : ICanvasTool {

		private readonly PrivateWindow _privateWindow;
		private readonly Canvas _canvas;
		private readonly Maps.PrivateMaskedMap _map;
		private RibbonToggleButton _button;
		private Polygon _shape;
		private Point _pnt1;
		private Point _pnt2;
		private Point _pnt3;
		private int _index;

		public DrawRectangle(PrivateWindow privateWindow, object button) {
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
				_index = 1;
			}
			else if (_index == 1) {
				_index++;
				UpdateDraw(e.GetPosition(_canvas));
			}
			else {
				_index++;
				UpdateDraw(e.GetPosition(_canvas));
				EndDraw();
			}
		}

		public void MouseMove(object sender, MouseEventArgs e) {
			if (_shape == null) {
				return;
			}
			UpdateDraw(e.GetPosition(_canvas));
			_privateWindow.DisplayPopup(CalculateDistance() + " " + _map.Unit);
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
			_pnt2 = pt1;
			_pnt3 = pt1;

			_shape = new Polygon {
				Points = CreatePointCollection(),
				Fill = Brushes.Green,
				Opacity = 0.5
			};

			_canvas.Children.Add(_shape);
		}

		private void UpdateDraw(Point pt) {
			if (_index == 1) {
				_pnt2 = pt;
				_pnt3 = pt;
			}
			else if (_index == 2) {
				_pnt3 = pt;
			}

			if (_shape == null) {
				return;
			}
			_shape.Points = CreatePointCollection();
		}

		private PointCollection CreatePointCollection() {
			var points = new PointCollection(4);
			var vector = new Vector(_pnt1.X - _pnt2.X, _pnt1.Y - _pnt2.Y);
			var vectorPerp = new Vector(vector.Y / 10, -vector.X / 10);

			if (_pnt3 == _pnt2) {
				_pnt3 = _pnt2 + vectorPerp;
			}
			
			points.Add(_pnt1);
			points.Add(_pnt2);
			points.Add(_pnt3);
			points.Add(_pnt3 + vector);

			return points;
		}

		private string CalculateDistance() {
			if (_shape == null) {
				return "0.0";
			}
			
			double length;
			if (_index == 1) {
				length = new Vector(_pnt1.X - _pnt2.X, _pnt1.Y - _pnt2.Y).Length;
			}
			else {
				length = new Vector(_pnt2.X - _pnt3.X, _pnt2.Y - _pnt3.Y).Length;
			}
			var dist = _map.ImageScaleMperPix * length;
			return dist.ToString("N1");
		}

		private void EndDraw() {
			var points = CreatePointCollection();
            _map.SymbolsPM.CreateSymbolPolygon(points, Colors.Green);


            _privateWindow.ActiveTool = null;
		}

	}
}
