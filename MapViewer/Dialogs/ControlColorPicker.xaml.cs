using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MapViewer.Annotations;
using static System.Windows.Media.ColorConverter;

namespace MapViewer.Dialogs {
    /// <summary>
    /// Interaction logic for ControloColorPicker.xaml
    /// </summary>
    public partial class ControlColorPicker : UserControl {
        private static readonly int NoOfColumns = 4;     
        
        private readonly string[] _selectedColors = {
            "LightBlue",  "Blue", "Purple", "Red", "IndianRed", "Sienna",  "Orange", "Yellow",
            "Khaki", "OliveDrab", "YellowGreen", "Green", "Black", "Gray",  "Silver", "White",
        };

        public ControlColorPicker() {
            InitializeComponent();
            ComboColors.ItemsSource = typeof(Colors).GetProperties().Where(c => _selectedColors.Contains(c.Name));
            //ComboColors.ItemsSource = _selectedColors;
        }

        private void ItemsPanel_Loaded(object sender, RoutedEventArgs e) {
            if (sender is Grid grid) {
                if (grid.RowDefinitions.Count == 0) {
                    for (var r = 0; r <= ComboColors.Items.Count / NoOfColumns; r++) {
                        grid.RowDefinitions.Add(new RowDefinition());
                    }
                }
                if (grid.ColumnDefinitions.Count == 0) {
                    for (var c = 0; c < Math.Min(ComboColors.Items.Count, NoOfColumns); c++) {
                        grid.ColumnDefinitions.Add(new ColumnDefinition());
                    }
                }
                for (var i = 0; i < grid.Children.Count; i++) {
                    Grid.SetColumn(grid.Children[i], i % NoOfColumns);
                    Grid.SetRow(grid.Children[i], i / NoOfColumns);
                }
            }
        }

        public Color SelectedColor {
            get {
                var propertyInfo = ComboColors.SelectedItem as PropertyInfo;
                if (propertyInfo != null) {
                    return (Color)ConvertFromString(propertyInfo.Name);
                }

                return Colors.Blue;
            }
            set {
                int i = 0;
                foreach (PropertyInfo propertyInfo in ComboColors.ItemsSource) {
                    var color = (Color)ConvertFromString(propertyInfo.Name);
                    if (color.ToString() == value.ToString()) {
                        ComboColors.SelectedIndex = i;
                        return;
                    }
                    i++;
                }

            }
        }
    }
}
