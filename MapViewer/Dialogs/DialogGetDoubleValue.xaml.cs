using System.Globalization;
using System.Windows;

namespace MapViewer.Dialogs {
	public partial class DialogGetDoubleValue {

		public string LeadText1 {
			set => LabelHint1.Content = value;
        }

		public double DoubleValue {
			get => double.TryParse(TextBoxValue1.Text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture,  out var val) ? val : 0;
            set => TextBoxValue1.Text = value.ToString(CultureInfo.InvariantCulture);
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
