using System.ComponentModel;
using System.Windows;
using MapViewer.Utilities;
using SizeInt = System.Drawing.Size;

namespace MapViewer.Dialogs {
	public partial class DialogCalibrateDisplay : INotifyPropertyChanged {

		public event PropertyChangedEventHandler PropertyChanged;

		public double MonitorSizePixelWidth { get; set; }
		public double MonitorSizePixelHeight { get; set; }

		public double MonitorSizeMmWidth { get; set; }
		public double MonitorSizeMmHeight { get; set; }


		public Size MonitorResolution {
			get { return new Size(MonitorSizePixelWidth, MonitorSizePixelHeight); }
			set {
				MonitorSizePixelWidth = value.Width;
				MonitorSizePixelHeight = value.Height;
				OnPropertyChanged("MonitorSizePixelWidth");
				OnPropertyChanged("MonitorSizePixelHeight");
			}
		}

		public Size MonitorSize {
			get { return new Size(MonitorSizeMmWidth, MonitorSizeMmHeight); }
			set {
				MonitorSizeMmWidth = value.Width;
				MonitorSizeMmHeight = value.Height;
				OnPropertyChanged("MonitorSizeMmHeight");
				OnPropertyChanged("MonitorSizeMmWidth");
			}
		}		
		public DialogCalibrateDisplay() {
			InitializeComponent();

			CalibrateWindow.DataContext = this;
		}

		private void BtnOk_Click(object sender, RoutedEventArgs e) {
			DialogResult = true;
			Close();
		}

		private void BtnFetch_OnClick(object sender, RoutedEventArgs e) {
			var mm = new MonitorManager();
			var monitor = mm.GetLargestActiveMonitor();
			if (monitor != null) {
				if (monitor.ImageSize.HasValue) {
					MonitorSize = monitor.ImageSize.Value;
				}
				if (monitor.MaximumResolution.HasValue) {
					MonitorResolution = monitor.MaximumResolution.Value;
				}
			}

		}
		// Create the OnPropertyChanged method to raise the event
		protected void OnPropertyChanged(string name) {
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) {
				handler(this, new PropertyChangedEventArgs(name));
			}
		}


	}
}
