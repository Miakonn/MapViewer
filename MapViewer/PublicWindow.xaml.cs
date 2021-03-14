using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MapViewer.Properties;

namespace MapViewer
{
    /// <summary>
    /// Interaction logic for SideWindow.xaml
    /// </summary>
    public partial class PublicWindow {

		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Canvas _canvasRuler;

		private readonly RotateTransform _compassTransform;

	    public Size MonitorResolution {
			get => new Size(Settings.Default.PublicMonitorResolutionWidth, Settings.Default.PublicMonitorResolutionHeight);
            set {
			    Settings.Default.PublicMonitorResolutionWidth = (int)value.Width;
				Settings.Default.PublicMonitorResolutionHeight = (int)value.Height;
				Settings.Default.Save();
		    }
	    }

		public Size MonitorSize {
			get => new Size(Settings.Default.PublicMonitorSizeWidth, Settings.Default.PublicMonitorSizeHeight);
            set {
				Settings.Default.PublicMonitorSizeWidth = (int)value.Width;
				Settings.Default.PublicMonitorSizeHeight = (int)value.Height;
				Settings.Default.Save();
			}
		}

	    public double MonitorScaleMMperPixel { 
			get { 
				if ((MonitorResolution.Width > 0) && (MonitorSize.Width > 0)) {
					var scaleX = (MonitorSize.Width/MonitorResolution.Width);
					if ((MonitorResolution.Height > 0) && (MonitorSize.Height > 0)) {
						var scaleY = (MonitorSize.Height / MonitorResolution.Height); 
						return (scaleX + scaleY) / 2.0;
					}
					return scaleX;
				}
				return 0;
			} 
		}

		public bool IsCalibrated => MonitorScaleMMperPixel > 0;

        public MaskedMap Map { get; }

        public PublicWindow() {
            InitializeComponent();

            Map = new MaskedMap(true, this);
			_canvasRuler = new Canvas();

			ContentPresenter1.Content = Map.CanvasMapMask;
			ContentPresenter2.Content = Map.CanvasOverlay;
			ContentPresenter3.Content = _canvasRuler;

		    _compassTransform = new RotateTransform(0);

	    }

	    public void MaximizeToSelectedMonitor() {
            var screen = SelectScreen();
            if (screen == null) {
                return;
            }

		    var rect = screen.WorkingArea;
			Top = rect.Top;
		    Left = rect.Left;
		    Width = rect.Width;
		    Height = rect.Height;
		    if (IsLoaded) {
			    WindowState = WindowState.Maximized;
		    }
			PublicBorder.Background = new SolidColorBrush(Map.MaskColor);
	    }
        
        public Screen SelectScreen() {
            if (Settings.Default.DisplayPublicNumber == 1) {
                return Screen.PrimaryScreen;
            }
            var screens = Screen.AllScreens.Where(screen => !screen.Primary).ToArray();
            if ((Settings.Default.DisplayPublicNumber - 2) < screens.Length) {
                return screens[Settings.Default.DisplayPublicNumber - 2];
            }
            return null;
        }


        public void RotateClockwise() {
			Map.RotateClockwise();
			_compassTransform.Angle = Map.TrfRotation.Angle;
		}

	    public void DrawCompass() {
		    var image = new BitmapImage(new Uri("pack://application:,,,/Images/Compass_rose.png"));
			
			_compassTransform.Angle = Map.TrfRotation.Angle;
			_compassTransform.CenterX = image.Width / 2;
			_compassTransform.CenterY = image.Height / 2;
		    
			var compass = new Image {
				RenderTransform = _compassTransform,
				Opacity = 1.0,
				Source = image,
				Uid = "Compass"
			};
			Canvas.SetLeft(compass, ActualWidth - image.Width - 25);
			Canvas.SetTop(compass, ActualHeight - image.Height - 25);
			_canvasRuler.Children.Add(compass);
	    }

		#region Ruler
		private static double CalcStep(double length, out int count) {
			Log.Debug("CalcStep length=" + length);

		    var factor = Math.Pow(10, Math.Floor(Math.Log10(length)) - 1);
		    length /= factor;

		    if (length <= 10) {
			    count = (int) length;
			    return factor;
		    }
			if (length <= 20) {
				count = 10;
				return factor;
			}
			if (length <= 50) {
				count = 20;
				return factor;
			}
			count = 10;
			var result = 5*factor;
			return result;
	    }

	    private const double RulerMarginX = 20;

	    private void WriteRulerText(double length, double yPos, bool topJustify ) {

			var text = new TextBlock {
				RenderTransform = new RotateTransform(-90),
				Text = $"{length} {Map.Unit}",
				FontSize = 25,
				Foreground = Brushes.Red,
				FontWeight = FontWeights.UltraBold
			};

		    if (topJustify) {
			    yPos += 10 * text.Text.Length;
		    }

			Canvas.SetLeft(text, RulerMarginX - 20);
			Canvas.SetTop(text, yPos);
			_canvasRuler.Children.Add(text);
	    }

	    private void DrawRuler(int count, double step, double y0) {
			for (var i = 0; i < count; i++) {
				var shape = new Line {
					X1 = RulerMarginX,
					Y1 = y0 + step * i,
					X2 = RulerMarginX,
					Y2 = y0 + step * i + step,
					StrokeThickness = 14,
					Stroke = new SolidColorBrush((i % 2 == 0) ? Colors.WhiteSmoke : Colors.Red),
					Opacity = 1.0,
					Uid = "Ruler"

				};
				_canvasRuler.Children.Add(shape);
			}		    
	    }

		public void SetRuler() {
			Log.Debug($"SetRuler ImageScaleMperPix={Map.ImageScaleMperPix} Scale={Map.Scale}");

			var screenScaleMperPix = Map.ImageScaleMperPix/Map.Scale;

			_canvasRuler.Children.Clear();

			if (screenScaleMperPix < 0.0001  || screenScaleMperPix > 1E6) {
				Log.Error("Screen scale out of bounds!");
				return;
			}
			var y0 = ActualHeight/10;
		    var height = ActualHeight - 2 * y0;
			var lengthM = height * screenScaleMperPix;
            var stepM = CalcStep(lengthM, out var count);
			Log.Debug($"SetRuler stepM={stepM} count={count}");
			var stepPix = stepM / screenScaleMperPix;
			Log.Debug($"SetRuler stepPix={stepPix} screenScaleMperPix={screenScaleMperPix}");

			DrawRuler(count, stepPix, y0);
			WriteRulerText(count * stepM, y0 - 5, false);
			WriteRulerText(stepM, y0 + count * stepPix + 25, true);
		}

		#endregion

		private void PublicWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			e.Cancel = true;
			Visibility = Visibility.Hidden;
		} 

    }
}
