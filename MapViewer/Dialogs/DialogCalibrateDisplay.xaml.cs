using System.Globalization;
using System.Windows;

namespace MapViewer.Dialogs {
	public partial class DialogCalibrateDisplay {

		public int ScreenWidthPix { get; set; }

		public int ScreenWidthMM { get; set; }

		
		public DialogCalibrateDisplay() {
			InitializeComponent();

			CalibrateWindow.DataContext = this;
		}

		private void BtnOk_Click(object sender, RoutedEventArgs e) {
			DialogResult = true;
			Close();
		}

		private void BtnFetch_OnClick(object sender, RoutedEventArgs e) {
			
		}
	}
}
