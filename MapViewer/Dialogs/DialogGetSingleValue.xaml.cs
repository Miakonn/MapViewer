using System.Globalization;
using System.Windows;

namespace MapViewer.Dialogs {
	public partial class DialogGetSingleValue {

		public string LeadText {
			set => LabelHint.Content = value;
        }

		public float FloatValue {
			get => float.TryParse(TextBoxValue.Text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var val) ? val : 0;
            set => TextBoxValue.Text = value.ToString(CultureInfo.InvariantCulture);
        }

		public string TextValue {
			get => TextBoxValue.Text;
            set => TextBoxValue.Text =value;
        }

		public DialogGetSingleValue() {
			InitializeComponent();
            TextBoxValue.Focus();
		}

		private void BtnOk_Click(object sender, RoutedEventArgs e) {
			DialogResult = true;
			Close();
		}
	}
}
