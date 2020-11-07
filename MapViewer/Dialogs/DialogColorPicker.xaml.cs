using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MapViewer.Dialogs {
	/// <summary>
	/// Interaction logic for DialogColorPicker.xaml
	/// </summary>
	public partial class DialogColorPicker {
		public Brush SelectedColor { get; set; }
		private readonly string[] _selectedColors = { "Red", "Orange", "Yellow", "YellowGreen", "Green", "Turquoise", "Blue", "Purple" };

		public DialogColorPicker() {
			InitializeComponent();
		}
		private void ColorPicker_Loaded(object sender, RoutedEventArgs e) {
			var list = typeof(Brushes).GetProperties();

			var listSelected = (from colorName in _selectedColors from color in list where color.Name == colorName select color).ToList();
			colorList.ItemsSource = listSelected;
		}

		private void ColorPicker_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			var propertyInfo = e.AddedItems[0] as PropertyInfo;
			if (propertyInfo == null) {
				return;
			}
			SelectedColor = (Brush)propertyInfo.GetValue(null, null);
			DialogResult = true;
			Close();
		}
	}
}
