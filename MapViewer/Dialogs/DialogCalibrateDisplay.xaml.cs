using System.Globalization;
using System.Windows;

namespace MapViewer.Dialogs {
	public partial class DialogCalibrateDisplay {

		public float ScreenWidthPix {
			get {
				float val;
				return float.TryParse(TextBoxValue1.Text, out val) ? val : 0;
			}
			set { TextBoxValue1.Text = value.ToString(CultureInfo.InvariantCulture); }
		}

		public float ScreenWidthMM {
			get {
				float val;
				return float.TryParse(TextBoxValue2.Text, out val) ? val : 0;
			}
			set { TextBoxValue2.Text = value.ToString(CultureInfo.InvariantCulture); }
		}

		public DialogCalibrateDisplay() {
			InitializeComponent();
		}

		private void BtnOk_Click(object sender, RoutedEventArgs e) {
			DialogResult = true;
			Close();
		}
	}
}
