using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls.Ribbon;

namespace MapViewer.Tools {
	class MaskPolygon : CanvasTool {

		private readonly PrivateWindow _privateWindow;
		private readonly Canvas _canvas;
		private readonly Maps.PrivateMaskedMap _map;
		private RibbonToggleButton _button;
		private Polygon _shape;
		private readonly bool _mask;
		private readonly PointCollection _pnts = new PointCollection();
        private int _noOfClicks;

		public MaskPolygon(PrivateWindow privateWindow, object button, bool mask) {
			_privateWindow = privateWindow;
			_mask = mask;
			_map = privateWindow.MapPrivate;
			_canvas = _map.CanvasOverlay;
			_button = (RibbonToggleButton)button;
            _noOfClicks = 0;
            _shape = null;
        }

		#region CanvasTool

        public override void MouseDown(object sender, MouseButtonEventArgs e) {
			if (_shape == null) {
				InitDraw(e.GetPosition(_canvas));
			}
			else if (IsCloseStartingPoint()) {
				EndDraw();
			}
			else {
                if (_noOfClicks < 2) {
                   _pnts[_noOfClicks] = e.GetPosition(_canvas);
                }
                else {
                    _pnts.Add(e.GetPosition(_canvas));
                }
                _noOfClicks++;
			}
		}

        public override void MouseMove(object sender, MouseEventArgs e) {
			if (_shape == null) {
				return;
			}
            _pnts[_noOfClicks] = e.GetPosition(_canvas);

            if (_noOfClicks == 2) {
                _shape.StrokeThickness = 0;
            }
        }


		public override void KeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Space) {
				EndDraw();
			}
		}

		public override void Deactivate() {
			if (_shape != null) {
				_canvas.Children.Remove(_shape);
			}
			_shape = null;

			if (_button != null) {
				_button.IsChecked = false;
			}
			_button = null;
		}


		#endregion

		private void InitDraw(Point pnt) {
			_pnts.Add(pnt);
			_pnts.Add(pnt);
			_pnts.Add(pnt);
            _noOfClicks = 1;
			_shape = new Polygon {
				Points = _pnts,
				Fill = new SolidColorBrush(_mask ? WritableBitmapUtils.MaskColor : Colors.White),
				FillRule = FillRule.EvenOdd,
				StrokeThickness = 3,
                StrokeEndLineCap = PenLineCap.Flat,
                StrokeStartLineCap = PenLineCap.Flat,
                StrokeLineJoin = PenLineJoin.Bevel,
                Stroke = new SolidColorBrush(_mask ? WritableBitmapUtils.MaskColor : Colors.White),
				Opacity = 0.5
			};

			_canvas.Children.Add(_shape);
		}

		private void EndDraw() {
			_pnts.Add(_pnts[0]);

			_map.CanvasOverlay.Children.Remove(_shape);

			_map.MaskPolygon(_pnts, WritableBitmapUtils.ColorIndex(_mask));
			_privateWindow.ActiveTool = null;
		}

		private bool IsCloseStartingPoint() {
            if (_noOfClicks < 3) {
				return false;
			}
			var last = _pnts.Count -1;
			double length = new Vector(_pnts[0].X - _pnts[last].X, _pnts[0].Y - _pnts[last].Y).Length;
			return (length < 20);
		}
    }
}
