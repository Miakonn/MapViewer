using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls.Ribbon;
using MapViewer.Dialogs;

namespace MapViewer.Tools {
	class Calibrate : ICanvasTool {

		private readonly PrivateWindow _privateWindow;
		private readonly Canvas _canvas;
		private readonly Maps.MaskedMap _map;
		private RibbonToggleButton _button;
		private Line _line;

		public Calibrate(PrivateWindow privateWindow, object button) {
			_privateWindow = privateWindow;
			_map = privateWindow.MapPrivate;
			_canvas = _map.CanvasOverlay;
			_button = (RibbonToggleButton)button;
            _line = null;
        }

		#region ICanvasTool

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
			if (_line != null) {
				_canvas.Children.Remove(_line);
			}
			_line = null;

			if (_button != null) {
				_button.IsChecked = false;
			}
			_button = null;
		}

		public bool ShowPublicCursor() {
			return false;
		}
		#endregion

		private void InitDraw(Point pt1) {
			_line = new Line {
				X1 = pt1.X,
				Y1 = pt1.Y,
				X2 = pt1.X,
				Y2 = pt1.Y,
				Stroke = Brushes.Green,
				StrokeThickness = 10 / _map.ZoomScale,
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
					
			var dialog = new DialogGetDoubleValue() {
				LeadText1 = "Length (m)",
				Owner = _privateWindow
			};

			var result = dialog.ShowDialog();
			if (!result.HasValue || !result.Value) {
				_privateWindow.ActiveTool = null;
				return;
			}

			var length = new Vector(_line.X1 - _line.X2, _line.Y1 - _line.Y2).Length;

			_map.ImageScaleMperPix = dialog.DoubleValue / length;

			_privateWindow.ActiveTool = null;
		}
	}
}
