using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;


namespace MapViewer.Tools {
	class DrawCone : ICanvasTool {

		private MainWindow _mainWindow;
		private Canvas _canvas;
		private MaskedMap _map;
		private ToolTip _tooltip;
		private Polygon _shape;
		private Point _pnt1;
		private Point _pnt2;

		public DrawCone(MainWindow mainWindow) {
			_mainWindow = mainWindow;
			_map = mainWindow._mapPrivate;
			_canvas = _map.CanvasOverlay;
		}

		#region ICanvasTool
		public void Activate() {
			_shape = null;
		}

		public void MouseDown(object sender, MouseButtonEventArgs e) {
			if (_shape == null) {
				InitDraw(e.GetPosition(_canvas));
				_tooltip = new ToolTip();
				_canvas.ToolTip = _tooltip;
				_tooltip.Content = "0.0 m";
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
			_tooltip.Content = string.Format(CalculateDistance() + " m");
		}

		public void MouseUp(object sender, MouseButtonEventArgs e) { }

		public void KeyDown(object sender, KeyEventArgs e) { }

		public void Deactivate() {
			_mainWindow.ActiveTool = null;
			if (_shape != null) {
				_canvas.Children.Remove(_shape);
			}
			_shape = null;
			_canvas.ToolTip = null;
		}

		#endregion

		private void InitDraw(Point pt1) {
			_pnt1 = pt1;
			_pnt2 = pt1;

			_shape = new Polygon {
				Points = CreatePointCollection(),
				Fill = Brushes.Yellow,
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
			_map.OverlayPolygon(points, Colors.Yellow, "Cone");
			if (_mainWindow._publicWindow.IsVisible) {
				_mainWindow._publicWindow.Map.OverlayPolygon(points, Colors.Green, "Cone");
			}

			Deactivate();
		}

	}
}
