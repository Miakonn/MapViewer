using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MapViewer
{
    /// <summary>
    /// Interaction logic for SideWindow.xaml
    /// </summary>
    public partial class PublicWindow {

		private readonly MaskedMap _map;

	    private readonly Canvas _canvasRuler;

	    public MaskedMap Map {
		    get { return _map;  }
	    }

	    public PublicWindow() {
            InitializeComponent();
			
			_map = new MaskedMap(true) {
				ParentWindow = this,
			};
			_canvasRuler = new Canvas();

			ContentPresenter1.Content = _map.CanvasMapMask;
			ContentPresenter2.Content = _map.CanvasOverlay;
			ContentPresenter3.Content = _canvasRuler;
	    }

	    public void MaximizeToSecondaryMonitor() {
		    var screen2 = Screen.AllScreens.FirstOrDefault(screen => screen.Primary == false);
		    if (screen2 == null) {
			    return;
		    }

		    var rect = screen2.WorkingArea;
			Top = rect.Top;
		    Left = rect.Left;
		    Width = rect.Width;
		    Height = rect.Height;
		    if (IsLoaded) {
			    WindowState = WindowState.Maximized;
		    }
	    }

	    private static double CalcStep(double length, ref int count) {
			for (var i = count; i > 0; i--) {
				var step = Math.Floor(length / i);
				if (step > 0) {
					count = i;
					return step;
				}
			}
		    return 0.0;
	    }

		public void SetRuler(double screenScaleMperPix) {
			_canvasRuler.Children.Clear();

			if (screenScaleMperPix < 0.0001) {
				return;
			}

		    var height = 0.8 * ActualHeight;
			var length = height * screenScaleMperPix;
			var count = 10;
			var step = CalcStep(length, ref count);

			if ((int)step == 0) {
				return;
			}
			if (step == 2) {
				step = 1;
				count *= 2;
			}

			var x0 = 20;
			var y0 = ActualHeight / 10;
			var stepPix = step / screenScaleMperPix;

		    for (var i = 0; i < count; i++) {
				var shape = new Line {
					X1 = x0,
					Y1 = y0 + stepPix * i,
					X2 = x0,
					Y2 = y0 + stepPix * i + stepPix,
					StrokeThickness = 20,
					Stroke = new SolidColorBrush((i %2 == 0) ? Colors.WhiteSmoke : Colors.Red),
					Opacity = 0.8

				};
			    _canvasRuler.Children.Add(shape);
		    }

			var text = new TextBlock {
				RenderTransform = new RotateTransform(-90),
				Text = string.Format("{0} m", count * step),
				FontSize = 25,
				Foreground = Brushes.Red,
				FontWeight = FontWeights.UltraBold
			};
			Canvas.SetLeft(text, x0 - 15);
			Canvas.SetTop(text, y0 - 5);

			_canvasRuler.Children.Add(text);
		}

		private void PublicWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			e.Cancel = true;
			Visibility = Visibility.Hidden;
		} 

    }
}
