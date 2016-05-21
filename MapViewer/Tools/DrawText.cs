using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls.Ribbon;
using MapViewer.Dialogs;

namespace MapViewer.Tools {
	class DrawText : ICanvasTool {

		private readonly PrivateWindow _privateWindow;
		private readonly Canvas _canvas;
		private readonly MaskedMap _map;
		private RibbonToggleButton _button;
		private Line _line;

		public DrawText(PrivateWindow privateWindow, object button) {
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

		#endregion

		private void InitDraw(Point pt1) {
			_line = new Line {
				X1 = pt1.X,
				Y1 = pt1.Y,
				X2 = pt1.X,
				Y2 = pt1.Y,
				Stroke = Brushes.Blue,
				StrokeThickness = 10 / _map.Scale,
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

			var dialog = new DialogGetSingleValue {
				LeadText = "Text",
				Owner = _privateWindow
			};

			var result = dialog.ShowDialog();
			if (!result.HasValue || !result.Value) {
				_privateWindow.ActiveTool = null;
				return;
			}
			var angle = Math.Atan2(_line.Y2 - _line.Y1, _line.X2 - _line.X1) * (180 / Math.PI);

			_map.OverlayText(dialog.TextValue, _line.X1, _line.Y1, angle, Colors.Blue, "Text");

			if (_privateWindow.PublicWindow.IsVisible) {
				_privateWindow.MapPublic.OverlayText(dialog.TextValue,  _line.X1, _line.Y1,  angle, Colors.Blue, "Text");
			}

			_privateWindow.ActiveTool = null;
		}

	}
}
