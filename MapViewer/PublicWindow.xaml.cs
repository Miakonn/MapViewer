﻿using System;
using System.Configuration;
using System.Globalization;
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

		public float ScreenWidthMM {
			get { return float.Parse(ConfigurationManager.AppSettings["PublicScreenWidthMM"]); }
			set {
				var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				config.AppSettings.Settings["PublicScreenWidthMM"].Value = value.ToString(CultureInfo.InvariantCulture);
				config.Save(ConfigurationSaveMode.Modified);
				ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
			}
		}

		public float ScreenWidthPix {
			get { return float.Parse(ConfigurationManager.AppSettings["PublicScreenWidthPix"]); }
			set {
				var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				config.AppSettings.Settings["PublicScreenWidthPix"].Value = value.ToString(CultureInfo.InvariantCulture);
				config.Save(ConfigurationSaveMode.Modified);
				ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
			}
		}

		public bool IsCalibrated {
			get { return (ScreenWidthMM > 0) && (ScreenWidthPix > 0); }
		}

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

	    private static double CalcStep(double length, out int count) {

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
			return 5 * factor;
	    }

	    private const double RulerMarginX = 20;

	    private void WriteRulerText(double length, double yPos, bool topJustify ) {
			var text = new TextBlock {
				RenderTransform = new RotateTransform(-90),
				Text = string.Format("{0} m", length),
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
					Opacity = 1.0

				};
				_canvasRuler.Children.Add(shape);
			}		    
	    }

		public void SetRuler(double screenScaleMperPix) {
			_canvasRuler.Children.Clear();

			if (screenScaleMperPix < 0.0001) {
				return;
			}
			var y0 = ActualHeight/10;
		    var height = ActualHeight - 2 * y0;
			var length = height * screenScaleMperPix;
			int count;
			var stepM = CalcStep(length, out count);
			var stepPix = stepM / screenScaleMperPix;

			DrawRuler(count, stepPix, y0);
			WriteRulerText(count * stepM, y0 - 5, false);
			WriteRulerText(stepM, y0 + count * stepPix + 25, true);
		}

		private void PublicWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			e.Cancel = true;
			Visibility = Visibility.Hidden;
		} 

    }
}
