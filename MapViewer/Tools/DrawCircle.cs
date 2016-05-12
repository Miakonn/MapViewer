using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MapViewer.Tools {
	public class DrawCircle : ICanvasTool {

		private MainWindow _mainWindow;
		private Canvas _canvas;
		private MaskedMap _map;

		private Ellipse _circle;

		private Point _pnt1;
		private Point _pnt2;

		public DrawCircle(MainWindow mainWindow) {
			_mainWindow = mainWindow;
			_map = mainWindow._mapPrivate;
			_canvas = _map.CanvasOverlay;
		}


		#region ICanvasTool
		public void Activate() {
			_circle = null;
		}

		public void MouseDown(object sender, MouseButtonEventArgs e) {
			if (_circle == null) {
				_pnt1 = e.GetPosition(_canvas);
				InitDraw(_pnt1);
			}
			else {
				_pnt2 = e.GetPosition(_canvas);
				EndDraw();
			}
		}

		public void MouseMove(object sender, MouseEventArgs e) {
			UpdateDraw(_pnt1, e.GetPosition(_canvas));
		}

		public void MouseUp(object sender, MouseButtonEventArgs e) { }

		public void KeyDown(object sender, KeyEventArgs e) { }

		public void Deactivate() {
			_mainWindow.ActiveTool = null;
			if (_circle != null) {
				_canvas.Children.Remove(_circle);
			}
			_circle = null;
		}

		#endregion

		private void InitDraw(Point pt1) {
			_pnt1 = pt1;

			_circle = new Ellipse {
				Width = 1,
				Height = 1,
				Fill = new SolidColorBrush(Colors.Blue),
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
			var radius = (int)(_circle.ActualWidth / 2);
			_map.OverlayCircle(center, radius, Colors.Blue, "Circle");
			if (_mainWindow._publicWindow.IsVisible) {
				_mainWindow._publicWindow.Map.OverlayCircle(center, radius, Colors.Blue, "Circle");
			}
			Deactivate();
		}

		private Point GetElementCenter(FrameworkElement element) {
			var transform = element.TransformToVisual(_map.CanvasMapMask);
			return transform.Transform(new Point(element.ActualWidth / 2.0, element.ActualHeight / 2.0));
		}
	}
}
