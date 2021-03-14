using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MapViewer.Dialogs {
	/// <summary>
	/// Interaction logic for DialogColorPicker.xaml
	/// </summary>
	public partial class DialogPlayer : Window {

		public Brush SelectedBrush { get; set; }

		private readonly string[] _selectedColors = { "Red", "Orange", "Yellow", "YellowGreen", "Green", "Turquoise", "Blue", "Purple" };

	
		public DialogPlayer() {
			InitializeComponent();
		}

        public string LeadText {
            set => LabelHint.Content = value;
        }

        public string TextValue {
            get => TextBoxValue.Text;
            set => TextBoxValue.Text = value;
        }

        private void Player_Loaded(object sender, RoutedEventArgs e) {
			var list = typeof(Brushes).GetProperties();

			var listSelected = (from colorName in _selectedColors from color in list where color.Name == colorName select color).ToList();
			colorList.ItemsSource = listSelected;
            colorList.SelectedValue = listSelected[2];
            TextBoxValue.Focus();
        }

		private void PlayerColorPicker_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			var propertyInfo = e.AddedItems[0] as PropertyInfo;
			if (propertyInfo == null) {
				return;
			}

            SelectedBrush = (Brush) propertyInfo.GetValue(null, null);

            if (!string.IsNullOrWhiteSpace(TextValue)) {
                DialogResult = true;
                Close();
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
            Close();
        }
    }
}
