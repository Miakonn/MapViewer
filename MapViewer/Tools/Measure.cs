using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;


namespace MapViewer.Tools {
	class Measure : ICanvasTool {

		public MainWindow OwnerWindow { get; set;}
		public Canvas CanvasPrivate { get; set; }
		public MaskedMap Map { get; set; }

		private Line _line;


		#region ICanvasTool
		public void Activate() {
			_line = null;
		}

		public void MouseDown(object sender, MouseButtonEventArgs e) {
			if (_line == null) {
				InitDraw(e.GetPosition(CanvasPrivate));
			}
			else {
				UpdateDraw(e.GetPosition(CanvasPrivate)); 
				EndDraw();
			}
		}

		public void MouseMove(object sender, MouseEventArgs e) {
			UpdateDraw(e.GetPosition(CanvasPrivate));
		}

		public void MouseUp(object sender, MouseButtonEventArgs e) { }

		public void KeyDown(object sender, KeyEventArgs e) { }

		public void Deactivate() {
			OwnerWindow.ActiveTool = null;
		}

		#endregion

		private void InitDraw(Point pt1) {
			_line = new Line {
				X1 = pt1.X,
				Y1 = pt1.Y,
				X2 = pt1.X,
				Y2 = pt1.Y,
				Stroke = Brushes.Blue,
				StrokeThickness = 10,
				Opacity = 0.5
			};

			CanvasPrivate.Children.Add(_line);
		}

		private void UpdateDraw(Point pt2) {
			if (_line == null) {
				return;
			}
			_line.X2 = pt2.X;
			_line.Y2 = pt2.Y;
		}

		private void EndDraw() {
			CanvasPrivate.Children.Remove(_line);

			var length = new Vector(_line.X1 - _line.X2, _line.Y1 - _line.Y2).Length;
			var dist = Map.ImageScaleMperPix * length;
			MessageBox.Show(string.Format("Length is {0} m", dist));

			Deactivate();
		}

	}
}
