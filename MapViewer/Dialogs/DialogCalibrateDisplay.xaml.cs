using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using MapViewer.Properties;
using MapViewer.Utilities;
using ComboBox = System.Windows.Controls.ComboBox;

namespace MapViewer.Dialogs {
    public partial class DialogCalibrateDisplay : INotifyPropertyChanged {

		public event PropertyChangedEventHandler PropertyChanged;

		public double MonitorSizePixelWidth { get; set; }
		public double MonitorSizePixelHeight { get; set; }

		public double MonitorSizeMmWidth { get; set; }
		public double MonitorSizeMmHeight { get; set; }

		private readonly MonitorManager _monitorManager;

		public Size MonitorResolution {
			get => new Size(MonitorSizePixelWidth, MonitorSizePixelHeight);
            set {
				MonitorSizePixelWidth = value.Width;
				MonitorSizePixelHeight = value.Height;
				OnPropertyChanged("MonitorSizePixelWidth");
				OnPropertyChanged("MonitorSizePixelHeight");
			}
		}

		public Size MonitorSize {
			get => new Size(MonitorSizeMmWidth, MonitorSizeMmHeight);
            set {
				MonitorSizeMmWidth = value.Width;
				MonitorSizeMmHeight = value.Height;
				OnPropertyChanged("MonitorSizeMmHeight");
				OnPropertyChanged("MonitorSizeMmWidth");
			}
		}

		public DialogCalibrateDisplay() {
			InitializeComponent();

			_monitorManager = new MonitorManager();
			CalibrateWindow.DataContext = this;
		}

		private void BtnOk_Click(object sender, RoutedEventArgs e) {
			DialogResult = true;
			Close();
		}

		// Create the OnPropertyChanged method to raise the event
		protected void OnPropertyChanged(string name) {
			var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(name));
        }

		private void ComboBoxEdid_Loaded(object sender, RoutedEventArgs e) {
            if (sender is ComboBox comboBox) {
                var list = _monitorManager.MonitorList;
				list.Insert(0, "Select monitor");
				comboBox.ItemsSource = list;
				comboBox.SelectedIndex = 0;
			}
		}

		private void ComboBoxEdid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			var comboBox = sender as ComboBox;

			if ((comboBox == null) || (comboBox.SelectedIndex < 0)) {
				return;
			}

			var monitor = _monitorManager.Monitors.Find(s => s.Name == (string) comboBox.SelectedValue);
			if (monitor != null) {
				MonitorResolution = monitor.MaximumResolution ?? new Size();
				MonitorSize = monitor.ImageSize ?? new Size();
			}
		}

        private void ComboBoxDisplayNumber_Loaded(object sender, RoutedEventArgs e) {
            if (sender is ComboBox comboBox) {
                var listName = Screen.AllScreens.Select(s => s.DeviceName);
 
                comboBox.ItemsSource = listName;
                comboBox.SelectedItem = Settings.Default.DisplayPublicName;
            }
        }

        private void ComboBoxDisplayNumbed_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var comboBox = sender as ComboBox;

            if ((comboBox == null) || (comboBox.SelectedIndex < 0)) {
                return;
            }

            Settings.Default.DisplayPublicName = comboBox.SelectedItem as string;
            Settings.Default.Save();
        }
    }
}
