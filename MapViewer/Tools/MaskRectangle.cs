using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls.Ribbon;


namespace MapViewer.Tools {
	public class MaskRectangle : ICanvasTool {

		private MainWindow _mainWindow;
		private Canvas _canvas;
		private MaskedMap _map;
		private bool _mask;
		private RibbonToggleButton _button;

		private Rectangle _rect;

		private Point _pnt1;
		private Point _pnt2;

		public MaskRectangle(MainWindow mainWindow, object button, bool mask) {
			_mask = mask;
			_mainWindow = mainWindow;
			_map = mainWindow._mapPrivate;
			_canvas = _map.CanvasOverlay;
			_button = button as RibbonToggleButton;
		}


		#region ICanvasTool
		public void Activate() {
			_rect = null;
		}

		public void MouseDown(object sender, MouseButtonEventArgs e) {
			if (_rect == null) {
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
				Fill = new SolidColorBrush(_mask ? Colors.Black : Colors.White),
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
			_map.MaskRectangle(GetElementRect(_rect), (byte)(_mask ? 255 : 0));
			_canvas.Children.Remove(_rect);
			_rect = null;
		}

		private Int32Rect GetElementRect(FrameworkElement element) {
			var buttonTransform = element.TransformToVisual(_map.CanvasMapMask);
			var point = buttonTransform.Transform(new Point());
			return new Int32Rect((int)point.X, (int)point.Y, (int)element.ActualWidth, (int)element.ActualHeight);
		}
	}
}
