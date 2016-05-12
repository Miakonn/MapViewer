using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;


namespace MapViewer.Tools {
	class Measure : ICanvasTool {

		private MainWindow _mainWindow;
		private Canvas _canvas;
		private MaskedMap _map;

		private Line _line;

		public Measure(MainWindow mainWindow) {
			_mainWindow = mainWindow;
			_map = mainWindow._mapPrivate;
			_canvas = _map.CanvasOverlay;
		}

		#region ICanvasTool
		public void Activate() {
			_line = null;
		}

		public void MouseDown(object sender, MouseButtonEventArgs e) {
			if (_line == null) {
				InitDraw(e.GetPosition(_canvas));
			}
			else {
				UpdateDraw(e.GetPosition(_canvas)); 
				EndDraw();
			}
		}

		public void MouseMove(object sender, MouseEventArgs e) {
			UpdateDraw(e.GetPosition(_canvas));
		}

		public void MouseUp(object sender, MouseButtonEventArgs e) { }

		public void KeyDown(object sender, KeyEventArgs e) { }

		public void Deactivate() {
			_mainWindow.ActiveTool = null;
		}

		#endregion

		private void InitDraw(Point pt1) {
			_line = new Line {
				X1 = pt1.X,
				Y1 = pt1.Y,
				X2 = pt1.X,
				Y2 = pt1.Y,
				Stroke = Brushes.Blue,
				StrokeThickness = 10,
				Opacity = 0.5
			};

			_canvas.Children.Add(_line);
		}

		private void UpdateDraw(Point pt2) {
			if (_line == null) {
				return;
			}
			_line.X2 = pt2.X;
			_line.Y2 = pt2.Y;
		}

		private void EndDraw() {
			_canvas.Children.Remove(_line);

			var length = new Vector(_line.X1 - _line.X2, _line.Y1 - _line.Y2).Length;
			var dist = _map.ImageScaleMperPix * length;
			MessageBox.Show(string.Format("Length is {0} m", dist.ToString("N1")));

			Deactivate();
		}

	}
}
