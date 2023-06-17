using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls.Ribbon;
using System;
using MapViewer.Symbols;

namespace MapViewer.Tools {
	class DrawRectangle : CanvasTool {

		private readonly PrivateWindow _privateWindow;
		private readonly Canvas _canvas;
		private readonly Maps.PrivateMaskedMap _map;
		private RibbonToggleButton _button;
		private Polygon _shape;
		private Point _pnt1;
		private Point _pnt2;
		private Point _pnt3;
		private int _index;

		public DrawRectangle(PrivateWindow privateWindow, object button) {
			_privateWindow = privateWindow;
			_map = privateWindow.MapPrivate;
			_canvas = _map.CanvasOverlay;
			_button = (RibbonToggleButton)button;
			_shape = null;
		}

		#region CanvasTool

        public override void MouseDown(object sender, MouseButtonEventArgs e) {
            var pnt = e.GetPosition(_canvas);

			if (_shape == null) {
				InitDraw(pnt);
				_index = 1;
				return;
			}
			if (_index == 1) {
                var length = new Vector(_pnt1.X - pnt.X, _pnt1.Y - pnt.Y).Length * _map.ZoomScale;
                if (length > MinimumMoveScreenPixel) {
                    _index++;
                    UpdateDraw(pnt);
                }
            }
			else {
                var length = new Vector(_pnt2.X - pnt.X, _pnt2.Y - pnt.Y).Length * _map.ZoomScale;
                if (length > MinimumMoveScreenPixel) {
                    _index++;
                    UpdateDraw(pnt); 
                    EndDraw();
                }
            }
		}

		public override void MouseMove(object sender, MouseEventArgs e) {
			if (_shape == null) {
				return;
			}
			UpdateDraw(e.GetPosition(_canvas));
			_privateWindow.DisplayPopup(CalculateDistance() + " " + _map.Unit);
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
			_privateWindow.HidePopup(3);
		}

		public override bool ShowPublicCursor() {
			return true;
		}
		#endregion

		private void InitDraw(Point pt1) {
			_pnt1 = pt1;
			_pnt2 = pt1;
			_pnt3 = pt1;

			_shape = new Polygon {
				Points = CreatePointCollection(),
				Fill = Brushes.Green,
				Opacity = 0.5
			};

			_canvas.Children.Add(_shape);
		}

		private void UpdateDraw(Point pt) {
			if (_index == 1) {
				_pnt2 = pt;
				_pnt3 = pt;
			}
			else if (_index == 2) {
				_pnt3 = pt;
			}

			if (_shape == null) {
				return;
			}
			_shape.Points = CreatePointCollection();
		}

        private Vector PerpVectorFromLine(Point pntL1, Point pntL2, Point pnt3) {
            var  line = new Vector(pntL2.X - pntL1.X, pntL2.Y - pntL1.Y);
			var  slant = new Vector(pnt3.X - pntL1.X, pnt3.Y - pntL1.Y);

            // Construct a unit vector pointing 90 degrees right of our lineDirection.
            var perpendicular = (new Vector(line.Y, -line.X)) / line.Length;

            // Extract the component of our offset pointing in this perpendicular direction.
            double length = slant.X * perpendicular.X + slant.Y * perpendicular.Y;

            return perpendicular * length;
        }

        private PointCollection CreatePointCollection() {
			var line = new Vector(_pnt1.X - _pnt2.X, _pnt1.Y - _pnt2.Y);

            if (_pnt3 == _pnt2) {
                var minWidthVector = new Vector(line.Y / 10, -line.X / 10);
                _pnt3 = _pnt2 + minWidthVector;
			}

            var vectWidth = PerpVectorFromLine(_pnt1, _pnt2, _pnt3);
            var points = new PointCollection {
                _pnt1,
                _pnt2,
                _pnt2 + vectWidth,
                _pnt1 + vectWidth
            };

            return points;
		}

		private string CalculateDistance() {
			if (_shape == null) {
				return "0.0";
			}
			
			double length;
			if (_index == 1) {
				length = new Vector(_pnt1.X - _pnt2.X, _pnt1.Y - _pnt2.Y).Length;
			}
			else {
				length = new Vector(_pnt2.X - _pnt3.X, _pnt2.Y - _pnt3.Y).Length;
			}
			var dist = _map.ImageScaleMperPix * length;
			return dist.ToString("N1");
		}

        private void EndDraw() {
            var vectLength = new Vector(_pnt2.X - _pnt1.X, _pnt2.Y - _pnt1.Y);

            var angleDegree = SymbolsViewModel.ToDegrees(Math.Atan2(vectLength.Y, vectLength.X));
            var lengthMeter = Math.Max(vectLength.Length * _privateWindow.MapPrivate.ImageScaleMperPix, 0.2);


            var vectWidth = PerpVectorFromLine(_pnt1, _pnt2, _pnt3);
            var widthMeter = Math.Max(vectWidth.Length * _privateWindow.MapPrivate.ImageScaleMperPix, 0.2);

            var center = _pnt1 + vectLength * 0.5 + vectWidth * 0.5;

            _map.SymbolsPM.CreateSymbolRect(center, lengthMeter, widthMeter, angleDegree, Colors.Green);

            _privateWindow.ActiveTool = null;
        }

    }
}
