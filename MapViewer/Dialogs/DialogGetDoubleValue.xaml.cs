using System.Globalization;
using System.Windows;

namespace MapViewer.Dialogs {
	public partial class DialogGetDoubleValue {

		public string LeadText1 {
			set => LabelHint1.Content = value;
        }

		public string LeadText2 {
			set => LabelHint2.Content = value;
        }

		public string DefaultText2 {
			set => TextBoxValue2.Text = value;
        }

		public float FloatValue1 {
			get => float.TryParse(TextBoxValue1.Text, out var val) ? val : 0;
            set => TextBoxValue1.Text = value.ToString(CultureInfo.InvariantCulture);
        }

		public string TextValue2 {
			get => TextBoxValue2.Text;
            set => TextBoxValue2.Text =value;
        }

		public DialogGetDoubleValue() {
			InitializeComponent();
            TextBoxValue1.Focus();
        }

		private void BtnOk_Click(object sender, RoutedEventArgs e) {
			DialogResult = true;
			Close();
		}
	}
}
