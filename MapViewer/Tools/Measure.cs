using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls.Ribbon;

namespace MapViewer.Tools {
	class Measure : ICanvasTool {

		private readonly MainWindow _mainWindow;
		private readonly Canvas _canvas;
		private readonly MaskedMap _map;
		private ToolTip _tooltip;
		private RibbonToggleButton _button;
		private Line _line;

		public Measure(MainWindow mainWindow, object button) {
			_mainWindow = mainWindow;
			_map = mainWindow.MapPrivate;
			_canvas = _map.CanvasOverlay;
			_button = (RibbonToggleButton)button;
		}

		#region ICanvasTool
		public void Activate() {
			_line = null;
		}

		public void MouseDown(object sender, MouseButtonEventArgs e) {
			if (_line == null) {
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
			if (_line == null) {
				return;
			}
			UpdateDraw(e.GetPosition(_canvas));
			_tooltip.Content = string.Format(CalculateDistance() + " m");
		}

		public void MouseUp(object sender, MouseButtonEventArgs e) { }

		public void KeyDown(object sender, KeyEventArgs e) { }

		public void Deactivate() {
			if (_line != null) {
				_canvas.Children.Remove(_line);
			}
			_line = null;
			_canvas.ToolTip = null;
			if (_button != null) {
				_button.IsChecked = false;
			}
			_button = null;

		}

		#endregion

		private string CalculateDistance() {
			if (_line == null) {
				return "0.0";
			}
			var length = new Vector(_line.X1 - _line.X2, _line.Y1 - _line.Y2).Length;
			var dist = _map.ImageScaleMperPix * length;
			return dist.ToString("N1");
		}

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
			_mainWindow.ActiveTool = null;
		}

	}
}
