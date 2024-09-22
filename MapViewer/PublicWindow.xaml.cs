using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using MapViewer.Maps;
using MapViewer.Properties;

namespace MapViewer {

    public partial class PublicWindow {

        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

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
                    var scaleX = (MonitorSize.Width / MonitorResolution.Width);
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

        public PublicMaskedMap Map { get; }

        public PublicWindow()
        {
            InitializeComponent();

            Map = new PublicMaskedMap(this, DateTime.Now.Ticks);
          
            Layer1_Map.Content = Map.CanvasMap;
            Layer2_Symbol.Content = Map.CanvasOverlay;
            Layer3_Mask.Content = Map.CanvasMask;
            Layer4_Ruler.Content = Map.CanvasRuler;
        }

        public void PlaceOnSelectedMonitor()
        {
            var screen = SelectScreen();
            if (screen == null) {
                return;
            }

            WindowState = WindowState.Normal;

            var rect = screen.WorkingArea;
            Top = rect.Top;
            Left = rect.Left;
            Width = rect.Width;
            Height = rect.Height;

            PublicBorder.Background = new SolidColorBrush(WritableBitmapUtils.MaskColor);
        }

        public void MaximizeToCurrentMonitor()
        {
            WindowState = WindowState.Maximized;
        }

        public Screen SelectScreen()
        {
            var deviceName = Settings.Default.DisplayPublicName;
            var screen = Screen.AllScreens.FirstOrDefault(s => s.DeviceName == deviceName);
            if (screen == null) {
                Log.Error("Unknown display: " + deviceName);
            }
            else {
                Log.Info("Selecting display: " + screen);
            }
            return screen;
        }

        public void RotateClockwise()
        {
            Map.RotateClockwise();
        }

        private void PublicWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }

    }
}
