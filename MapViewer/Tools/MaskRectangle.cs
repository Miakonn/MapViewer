using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls.Ribbon;


namespace MapViewer.Tools {
	public class MaskRectangle : CanvasTool {
		private readonly Canvas _canvas;
		private readonly Maps.PrivateMaskedMap _map;
		private readonly bool _mask;
		private RibbonToggleButton _button;

		private Rectangle _rect;

		private Point _pnt1;
        private Point _pnt2;
		
        public MaskRectangle(PrivateWindow privateWindow, object button, bool mask) {
			_mask = mask;
			_map = privateWindow.MapPrivate;
			_canvas = _map.CanvasOverlay;
			_button = button as RibbonToggleButton;
            _rect = null;
        }

		#region CanvasTool

        public override void MouseDown(object sender, MouseButtonEventArgs e) {
			if (_rect == null) {
				_pnt1 = e.GetPosition(_canvas);
				InitDraw(_pnt1);
			}
			else {
				UpdateDraw(e.GetPosition(_canvas));
				EndDraw();
			}
		}

		public override void MouseMove(object sender, MouseEventArgs e) {
			UpdateDraw(e.GetPosition(_canvas));
		}

		public override void Deactivate() {
			if (_rect != null) {
				_canvas.Children.Remove(_rect);
			}
			_rect = null;

			if (_button != null) {
				_button.IsChecked = false;
			}
			_button = null;
		}

		#endregion

		private void InitDraw(Point pt1) {
			_pnt1 = pt1;

			_rect = new Rectangle {
				Width = 5,
				Height = 5,
				Fill = new SolidColorBrush(_mask ? _map.MaskColor : Colors.White),
				Opacity = 0.5
			};

			Canvas.SetLeft(_rect, pt1.X);
			Canvas.SetTop(_rect, pt1.Y);
			_canvas.Children.Add(_rect);

		}

		private void UpdateDraw( Point pt2) {
			if (_rect == null) { 
				return; 
			}
            _pnt2 = pt2;
            Canvas.SetLeft(_rect, Math.Min(_pnt1.X, _pnt2.X));
			Canvas.SetTop(_rect, Math.Min(_pnt1.Y, _pnt2.Y));
			_rect.Width = Math.Abs(_pnt1.X - _pnt2.X);
			_rect.Height = Math.Abs(_pnt1.Y - _pnt2.Y);
        }

        private void EndDraw() {
            var pntTL = new Point((int)Math.Min(_pnt1.X, _pnt2.X), (int)Math.Min(_pnt1.Y, _pnt2.Y));
            var pntBR = new Point((int)Math.Max(_pnt1.X, _pnt2.X), (int)Math.Max(_pnt1.Y, _pnt2.Y));
            _map.MaskRectangle(pntTL, pntBR, (byte)(_mask ? 255 : 0));
            _canvas.Children.Remove(_rect);

            _rect = null;
        }
    }
}
