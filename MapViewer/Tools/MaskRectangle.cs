using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls.Ribbon;
using log4net;


namespace MapViewer.Tools {
	public class MaskRectangle : ICanvasTool {
		private readonly Canvas _canvas;
		private readonly Maps.PrivateMaskedMap _map;
		private readonly bool _mask;
		private RibbonToggleButton _button;

		private Rectangle _rect;

		private Point _pnt1;

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MaskRectangle(PrivateWindow privateWindow, object button, bool mask) {
			_mask = mask;
			_map = privateWindow.MapPrivate;
			_canvas = _map.CanvasOverlay;
			_button = button as RibbonToggleButton;
            _rect = null;
        }

		#region ICanvasTool

        public void MouseDown(object sender, MouseButtonEventArgs e) {
			if (_rect == null) {
				_pnt1 = e.GetPosition(_canvas);
				InitDraw(_pnt1);
			}
			else {
				UpdateDraw(_pnt1, e.GetPosition(_canvas));
				EndDraw();
			}
		}

		public void MouseMove(object sender, MouseEventArgs e) {
			UpdateDraw(_pnt1, e.GetPosition(_canvas));
		}

		public void MouseUp(object sender, MouseButtonEventArgs e) { }

		public void KeyDown(object sender, KeyEventArgs e) { }

		public void Deactivate() {
			if (_rect != null) {
				_canvas.Children.Remove(_rect);
			}
			_rect = null;

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

		private void UpdateDraw(Point pt1, Point pt2) {
			if (_rect == null) { 
				return; 
			}
			var x = Math.Min(pt1.X, pt2.X);
			var y = Math.Min(pt1.Y, pt2.Y);
			var width = Math.Abs(pt1.X - pt2.X);
			var height = Math.Abs(pt1.Y - pt2.Y);
			Canvas.SetLeft(_rect, x);
			Canvas.SetTop(_rect, y);
			_rect.Width = width;
			_rect.Height = height;
		}

		private void EndDraw() {
            try {
                _map.MaskRectangle(GetElementRect(_rect), (byte) (_mask ? 255 : 0));
                _canvas.Children.Remove(_rect);
            }
            catch (Exception ex) {
                Log.Error("EndDraw:" + ex.Message);
            }
            _rect = null;
		}

		private Int32Rect GetElementRect(FrameworkElement element) {
            if (_map?.CanvasMap == null) {
                Log.Error("_map.CanvasMap == null");
                return new Int32Rect();
            }
			var buttonTransform = element.TransformToVisual(_map.CanvasMap);
			var point = buttonTransform.Transform(new Point());
			return new Int32Rect((int)point.X, (int)point.Y, (int)element.ActualWidth, (int)element.ActualHeight);
		}
	}
}
