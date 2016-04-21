using System.Globalization;
using System.Windows;

namespace MapViewer {
	/// <summary>
	/// Interaction logic for DialogGetCalibration.xaml
	/// </summary>
	public partial class DialogGetFloatValue {
		private float _value;

		public string LeadText {
			set { LabelHint.Content = value; }
		}

		public float Value {
			get { return _value; }
			set {
				_value = value;
				TextBoxValue.Text = _value.ToString(CultureInfo.InvariantCulture);
			}
		}

		public DialogGetFloatValue() {
			InitializeComponent();
			
		}

		private void BtnCancel_Click(object sender, RoutedEventArgs e) {
			DialogResult = false;
			Close();
		}

		private void BtnOk_Click(object sender, RoutedEventArgs e) {
			if (float.TryParse(TextBoxValue.Text, out _value)) {
				DialogResult = true;
				Close();
			}
		}
	}
}
