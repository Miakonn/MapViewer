using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls.Ribbon;


namespace MapViewer.Tools {
	public class SelectSymbols : CanvasTool {
        private readonly PrivateWindow _privateWindow;
		private readonly Canvas _canvas;
		private readonly Maps.PrivateMaskedMap _map;
        private RibbonToggleButton _button;

		private Rectangle _rect1;
        private Rectangle _rect2;

        private Point _pnt1;
        private Point _pnt2;
		
        public SelectSymbols(PrivateWindow privateWindow, object button, bool mask) {
            _privateWindow = privateWindow;
            _map = privateWindow.MapPrivate;
			_canvas = _map.CanvasOverlay;
			_button = button as RibbonToggleButton;
            _rect1 = null;
			_rect2 = null;
        }

		#region CanvasTool

        public override void MouseDown(object sender, MouseButtonEventArgs e) {
            bool shiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            if (_rect1 == null) {
				_pnt1 = e.GetPosition(_canvas);
				InitDraw(_pnt1);
			}
			else if (!shiftPressed) {
				UpdateDraw(e.GetPosition(_canvas));
				EndDraw();
			}
		}

        public override void MouseUp(object sender, MouseButtonEventArgs e) {
            bool shiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            if (shiftPressed) {
                UpdateDraw(e.GetPosition(_canvas));
                EndDraw();
            }
        }

        public override void MouseMove(object sender, MouseEventArgs e) {
            UpdateDraw(e.GetPosition(_canvas));
		}

		public override void Deactivate() {
			if (_rect1 != null) {
				_canvas.Children.Remove(_rect1);
			}
			_rect1= null;

            if (_rect2 != null) {
                _canvas.Children.Remove(_rect2);
            }
            _rect2 = null;

            if (_button != null) {
				_button.IsChecked = false;
			}
			_button = null;
		}

		#endregion

		private void InitDraw(Point pt1) {
			_pnt1 = pt1;

			_rect1 = new Rectangle {
				Width = 5,
				Height = 5,
				Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 1
			};

			Canvas.SetLeft(_rect1, pt1.X);
			Canvas.SetTop(_rect1, pt1.Y);
			_canvas.Children.Add(_rect1);

            _rect2 = new Rectangle {
                Width = 5,
                Height = 5,
                Stroke = new SolidColorBrush(Colors.White),
                StrokeDashArray = DoubleCollection.Parse("3, 3"),
                StrokeThickness = 1
            };

            Canvas.SetLeft(_rect2, pt1.X);
            Canvas.SetTop(_rect2, pt1.Y);
            _canvas.Children.Add(_rect2);
        }

		private void UpdateDraw(Point pt2) {
			if (_rect1 == null) { 
				return; 
			}
            _pnt2 = pt2;

            var rect = new Rect(_pnt1, _pnt2);

            Canvas.SetLeft(_rect1, rect.Left);
			Canvas.SetTop(_rect1, rect.Top);
			_rect1.Width = rect.Width;
			_rect1.Height = rect.Height;

            Canvas.SetLeft(_rect2, rect.Left);
            Canvas.SetTop(_rect2, rect.Top);
            _rect2.Width = rect.Width;
            _rect2.Height = rect.Height;
        }

        private void EndDraw() {
            _map.SymbolsPM.SelectSymbolRectangle(new Rect(_pnt1, _pnt2));
            _canvas.Children.Remove(_rect1);
            _rect1 = null;
            _canvas.Children.Remove(_rect2);
            _rect2 = null;

            _privateWindow.ActiveTool = null;
            _privateWindow.SetSelected(_map.SymbolsPM.GetSelected());
        }
    }
}
