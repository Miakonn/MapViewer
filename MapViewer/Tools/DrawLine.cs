using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls.Ribbon;

namespace MapViewer.Tools {
	class DrawLine : ICanvasTool {

		private readonly PrivateWindow _privateWindow;
		private readonly Canvas _canvas;
		private readonly Maps.MaskedMap _map;
		private RibbonToggleButton _button;
		private Line _line;

		public DrawLine(PrivateWindow privateWindow, object button) {
			_privateWindow = privateWindow;
			_map = privateWindow.MapPrivate;
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
			_privateWindow.DisplayPopup(CalculateDistance() + " " + _map.Unit);
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
			_privateWindow.HidePopup(3);
		}


		public bool ShowPublicCursor() {
			return true;
		}
		#endregion

		private void InitDraw(Point pt1) {
			_line = new Line {
				X1 = pt1.X,
				Y1 = pt1.Y,
				X2 = pt1.X,
				Y2 = pt1.Y,
				Stroke = Brushes.OrangeRed,
				StrokeThickness = 1 / _map.ImageScaleMperPix,
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

		private string CalculateDistance() {
			if (_line == null) {
				return "0.0";
			}
			var length = new Vector(_line.X1 - _line.X2, _line.Y1 - _line.Y2).Length;
			var dist = _map.ImageScaleMperPix * length;
			return dist.ToString("N1");
		}

		private void EndDraw() {
			_map.OverlayLine(_line.X1, _line.Y1, _line.X2, _line.Y2, 1 / _map.ImageScaleMperPix, Colors.OrangeRed, "Line");
			if (_privateWindow.PublicWindow.IsVisible) {
				_privateWindow.MapPublic.OverlayLine(_line.X1, _line.Y1, _line.X2, _line.Y2, 2 / _map.ImageScaleMperPix, Colors.OrangeRed, "Line");
			}

			_privateWindow.ActiveTool = null;
		}

	}
}
