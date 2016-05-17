using System.Globalization;
using System.Windows;

namespace MapViewer.Dialogs {
	public partial class DialogGetSingleValue {

		public string LeadText {
			set { LabelHint.Content = value; }
		}

		public float FloatValue {
			get {
				float val;
				return float.TryParse(TextBoxValue.Text, out val) ? val : 0;
			}
			set { TextBoxValue.Text = value.ToString(CultureInfo.InvariantCulture); }
		}

		public string TextValue {
			get { return TextBoxValue.Text; }
			set {  TextBoxValue.Text =value; }
		}

		public DialogGetSingleValue() {
			InitializeComponent();
		}

		private void BtnOk_Click(object sender, RoutedEventArgs e) {
			DialogResult = true;
			Close();
		}
	}
}
