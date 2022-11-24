using System.Globalization;
using System.Windows;

namespace MapViewer.Dialogs {
	public partial class DialogCreatureProp {


		public string Caption {
            get => TextBoxValue1.Text;
            set => TextBoxValue1.Text = value;
        }

        public double SizeMeter {
            get {
                var str = TextBoxValue2.Text.Replace(',', '.');
                if (double.TryParse(str, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var val)) {
                    return val;
                }

                return 0.8;
            }
            set => TextBoxValue2.Text = value.ToString("N1", CultureInfo.InvariantCulture);
        }

        public Point StartPosition {
            set { 
                Left = value.X;
                Top = value.Y;
            }
        }


        public DialogCreatureProp() {
			InitializeComponent();
            TextBoxValue1.Focus();
        }

		private void BtnOk_Click(object sender, RoutedEventArgs e) {
			DialogResult = true;
			Close();
		}
	}
}
