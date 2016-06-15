using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls.Ribbon;

namespace MapViewer.Tools {
	class MaskPolygon : ICanvasTool {

		private readonly PrivateWindow _privateWindow;
		private readonly Canvas _canvas;
		private readonly MaskedMap _map;
		private RibbonToggleButton _button;
		private Polygon _shape;
		private readonly bool _mask;
		private PointCollection _pnts = new PointCollection();

		public MaskPolygon(PrivateWindow privateWindow, object button, bool mask) {
			_privateWindow = privateWindow;
			_mask = mask;
			_map = privateWindow.MapPrivate;
			_canvas = _map.CanvasOverlay;
			_button = (RibbonToggleButton)button;
		}

		#region ICanvasTool
		public void Activate() {
			_shape = null;
		}

		public void MouseDown(object sender, MouseButtonEventArgs e) {
			if (_shape == null) {
				InitDraw(e.GetPosition(_canvas));
			}
			else if (IsCloseStartingPoint()) {
				EndDraw();
			}
			else {
				_pnts.Add(e.GetPosition(_canvas));
				if (_pnts.Count > 2) {
					_shape.StrokeThickness = 0;
				}
			}
		}

		private bool IsCloseStartingPoint() {
			if (_pnts.Count < 3) {
				return false;
			}
			var last = _pnts.Count -1;
			double length = new Vector(_pnts[0].X - _pnts[last].X, _pnts[0].Y - _pnts[last].Y).Length;
			return (length < 20);
		}

		public void MouseMove(object sender, MouseEventArgs e) {
			if (_shape == null) {
				return;
			}
			UpdateDraw(e.GetPosition(_canvas));
		}

		public void MouseUp(object sender, MouseButtonEventArgs e) { }

		public void KeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Space) {
				EndDraw();
			}
		}

		public void Deactivate() {
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

			_shape = new Polygon {
				Points = _pnts,
				Fill = (_mask ? Brushes.Black : Brushes.White),
				FillRule = FillRule.EvenOdd,
				StrokeThickness = 2,
				Stroke = (_mask ? Brushes.Black : Brushes.White),
				Opacity = 0.5
			};

			_canvas.Children.Add(_shape);
		}

		private void UpdateDraw(Point pt) {
			_pnts[_pnts.Count - 1] = pt;
		}

		private void EndDraw() {
			_pnts.Add(_pnts[0]);
			_pnts.RemoveAt(0);

			_map.CanvasOverlay.Children.Remove(_shape);

			_map.MaskPolygon(_pnts, (byte)(_mask ? 255 : 0));
			_privateWindow.ActiveTool = null;
		}

	}
}
