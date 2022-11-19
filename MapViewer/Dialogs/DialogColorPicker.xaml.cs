using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static System.Windows.Media.ColorConverter;

namespace MapViewer.Dialogs {
	/// <summary>
	/// Interaction logic for DialogColorPicker.xaml
	/// </summary>
	public partial class DialogColorPicker {
		public Brush SelectedBrush { get; set; }
        public Color SelectedColor { get; set; }

        private readonly string[] _selectedColors = { "Red", "Orange", "Yellow", "YellowGreen", "Green", "Turquoise", "Blue", "Purple", "Black", "LightBlue" };

		public DialogColorPicker() {
			InitializeComponent();
		}
		private void ColorPicker_Loaded(object sender, RoutedEventArgs e) {
			var list = typeof(Brushes).GetProperties();

			var listSelection = (from colorName in _selectedColors from color in list where color.Name == colorName select color).ToList();
			colorList.ItemsSource = listSelection;
		}

		private void ColorPicker_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			var propertyInfo = e.AddedItems[0] as PropertyInfo;
			if (propertyInfo == null) {
				return;
			}
            SelectedBrush = (SolidColorBrush)propertyInfo.GetValue(null, null);
            SelectedColor = (Color) ConvertFromString(propertyInfo.Name);
			DialogResult = true;
			Close();
		}
	}
}
