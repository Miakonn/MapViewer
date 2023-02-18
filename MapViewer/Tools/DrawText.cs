using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls.Ribbon;

namespace MapViewer.Tools {
	class DrawText : CanvasTool {

		private readonly PrivateWindow _privateWindow;
		private readonly Canvas _canvas;
		private readonly Maps.PrivateMaskedMap _map;
		private RibbonToggleButton _button;
		private Line _line;

        public DrawText(PrivateWindow privateWindow, object button) {
			_privateWindow = privateWindow;
			_map = privateWindow.MapPrivate;
			_canvas = _map.CanvasOverlay;
			_button = (RibbonToggleButton)button;
            _line = null;
        }

		#region CanvasTool

        public override void MouseDown(object sender, MouseButtonEventArgs e) {
			if (_line == null) {
				InitDraw(e.GetPosition(_canvas));
				return;
			}
            var length = new Vector(_line.X1 - _line.X2, _line.Y1 - _line.Y2).Length;
            if (length > MinimumMove) {
                EndDraw();
            }
        }

		public override void MouseMove(object sender, MouseEventArgs e) {
			if (_line == null) {
				return;
			}
			UpdateDraw(e.GetPosition(_canvas));
		}

		public override void Deactivate() {
			if (_line != null) {
				_canvas.Children.Remove(_line);
			}
			_line = null;
			if (_button != null) {
				_button.IsChecked = false;
			}
			_button = null;

		}

		public override bool ShowPublicCursor() {
			return true;
		}

		#endregion

		private void InitDraw(Point pt1) {
			_line = new Line() {
				X1 = pt1.X,
				Y1 = pt1.Y,
				X2 = pt1.X,
				Y2 = pt1.Y,
				Stroke = Brushes.Blue,
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
            var angle = Math.Atan2(_line.Y2 - _line.Y1, _line.X2 - _line.X1) * (180 / Math.PI);
            var center = new Point((_line.X1 + _line.X2) / 2, (_line.Y1 + _line.Y2) / 2);
            var length = new Vector(_line.X1 - _line.X2, _line.Y1 - _line.Y2).Length * _map.ImageScaleMperPix;

            var symbol = _map.SymbolsPM.CreateSymbolText(center, angle, length , Colors.Blue, "");
            symbol.OpenDialogProp(_privateWindow, center, _privateWindow.MapPrivate.SymbolsPM);

            _privateWindow.ActiveTool = null;
		}
    }
}
