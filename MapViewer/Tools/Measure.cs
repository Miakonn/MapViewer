﻿using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls.Ribbon;

namespace MapViewer.Tools {
	class Measure : CanvasTool {

		private readonly PrivateWindow _privateWindow;
		private readonly Canvas _canvas;
		private readonly Canvas _canvasPublic;
		private readonly Maps.PrivateMaskedMap _map;
		private readonly Maps.PublicMaskedMap _mapPublic;
		private RibbonToggleButton _button;
		private Line _line;
		private Line _linePublic;

		public Measure(PrivateWindow privateWindow, object button) {
			_privateWindow = privateWindow;
			_map = privateWindow.MapPrivate;
			_mapPublic = privateWindow.MapPublic;
			_canvas = _map.CanvasOverlay;
			_canvasPublic = _mapPublic.CanvasOverlay;
			_button = (RibbonToggleButton)button;
            _line = null;
            _linePublic = null;
        }

        #region CanvasTool

        public override void MouseDown(object sender, MouseButtonEventArgs e) {
            if (_line == null) {
				InitDraw(e.GetPosition(_canvas));
				return;
            }

            UpdateDraw(e.GetPosition(_canvas));
            var length = new Vector(_line.X1 - _line.X2, _line.Y1 - _line.Y2).Length * _map.ZoomScale;
            if (length > MinimumMoveScreenPixel) {
                EndDraw();
            }
        }

		public override void MouseMove(object sender, MouseEventArgs e) {
			if (_line == null) {
				return;
			}

            UpdateDraw(e.GetPosition(_canvas));
			_privateWindow.DisplayPopup(CalculateDistance() + " " + _map.Unit);
		}

		public override void Deactivate() {
			if (_line != null) {
				_canvas.Children.Remove(_line);
				_line = null;
			}
			if (_linePublic != null) {
				_canvasPublic.Children.Remove(_linePublic);
				_linePublic = null;
			}

			if (_button != null) {
				_button.IsChecked = false;
				_button = null;
			}
			_privateWindow.HidePopup(10);
		}

		public override bool ShowPublicCursor() {
			return true;
		}

		#endregion

		private string CalculateDistance() {
			if (_line == null) {
				return "0.0";
			}
			var length = new Vector(_line.X1 - _line.X2, _line.Y1 - _line.Y2).Length;
			var dist = _map.ImageScaleMperPix * length;
            if (dist < 100) {
                return dist.ToString("N1");
            }

            double size = Math.Pow(10.0, Math.Floor(Math.Log10(dist)) - 2.0); 
            dist = Math.Round(dist / size) * size;
            return dist.ToString("N0");
        }

		private void InitDraw(Point pt1) {
			_line = new Line {
				X1 = pt1.X,
				Y1 = pt1.Y,
				X2 = pt1.X + 1,
				Y2 = pt1.Y + 1,
				Stroke = Brushes.Blue,
				StrokeThickness = 10 / _map.ZoomScale,
				Opacity = 0.5
			};
			_canvas.Children.Add(_line);

			if (_mapPublic.ShowPublicCursor) {
				_linePublic = new Line {
					X1 = pt1.X,
					Y1 = pt1.Y,
					X2 = pt1.X,
					Y2 = pt1.Y,
					Stroke = Brushes.Blue,
					StrokeThickness = 10/_map.ZoomScale,
					Opacity = 0.5
				};
				_canvasPublic.Children.Add(_linePublic);
			}
		}

		private void UpdateDraw(Point pt2) {
			if (_line == null) {
				return;
			}
			_line.X2 = pt2.X;
			_line.Y2 = pt2.Y;

			if (_mapPublic.ShowPublicCursor) {
				_linePublic.X2 = pt2.X;
				_linePublic.Y2 = pt2.Y;
			}
        }

		private void EndDraw() {
			_privateWindow.ActiveTool = null;
		}
    }
}
