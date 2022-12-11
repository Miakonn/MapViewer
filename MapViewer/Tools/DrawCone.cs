using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls.Ribbon;

namespace MapViewer.Tools {
	class DrawCone : CanvasTool {

		private readonly PrivateWindow _privateWindow;
		private readonly Canvas _canvas;
		private readonly Maps.PrivateMaskedMap _map;
		private RibbonToggleButton _button;
		private Polygon _shape;
		private Point _pnt1;
		private Point _pnt2;

		public DrawCone(PrivateWindow privateWindow, object button) {
			_privateWindow = privateWindow;
			_map = privateWindow.MapPrivate;
			_canvas = _map.CanvasOverlay;
			_button = (RibbonToggleButton)button;
            _shape = null;
        }

		#region CanvasTool

        public override void MouseDown(object sender, MouseButtonEventArgs e) {
			if (_shape == null) {
				InitDraw(e.GetPosition(_canvas));
			}
            UpdateDraw(e.GetPosition(_canvas));
            var length = new Vector(_pnt1.X - _pnt2.X, _pnt1.Y - _pnt2.Y).Length;
            if (length > MinimumMove) {
                EndDraw();
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

			_shape = new Polygon {
				Points = CreatePointCollection(),
				Fill = Brushes.Green,
				Opacity = 0.5
			};

			_canvas.Children.Add(_shape);
		}

		private void UpdateDraw(Point pt2) {
			_pnt2 = pt2;
			if (_shape == null) {
				return;
			}
			_shape.Points = CreatePointCollection();
		}

		private PointCollection CreatePointCollection() {
			var points = new PointCollection(3);
			var vector = new Vector(_pnt2.X - _pnt1.X, _pnt2.Y - _pnt1.Y);

            var angleDegree = Math.Atan2(vector.Y, vector.X) * 180.0 / Math.PI;

            points.Add(_pnt1);
            for (var a = -30.0; a <= 30.0; a += 5.0) {
                var aRadian = (a + angleDegree) * (Math.PI / 180.0);
                var pnt = new Point(_pnt1.X + vector.Length * Math.Cos(aRadian), _pnt1.Y + vector.Length * Math.Sin(aRadian));
                points.Add(pnt);
            }
			
            return points;
		}

		private string CalculateDistance() {
			if (_shape == null) {
				return "0.0";
			}
			var length = new Vector(_pnt1.X - _pnt2.X, _pnt1.Y - _pnt2.Y).Length;
			var dist = _map.ImageScaleMperPix * length;
			return dist.ToString("N1");
		}

		private void EndDraw() {
            var vector = new Vector(_pnt2.X - _pnt1.X, _pnt2.Y - _pnt1.Y);
            var angleDegree = Math.Atan2(vector.Y, vector.X) * 180.0 / Math.PI;
            var lengthMeter = vector.Length * _privateWindow.MapPrivate.ImageScaleMperPix;
			
            _map.SymbolsPM.CreateSymbolCone(_pnt1, angleDegree, lengthMeter , 60.0, Colors.Green);
            _privateWindow.ActiveTool = null;
		}
    }
}
