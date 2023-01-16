using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using MapViewer.Symbols;

namespace MapViewer.Dialogs {
	public partial class DialogConeProp
    {
        private SymbolCone _symbol;

        private Color _color;
        public double Angle { get; set; }

        public SymbolsViewModel SymbolsVM { get; set; }

        public SymbolCone Symbol {
            get => _symbol;
            set {
                _symbol = value;
                SizeValue.Text = Symbol.SizeMeter.ToString("N1", CultureInfo.InvariantCulture);
                Angle = Symbol.RotationDegree;
                WidthValue.Text = Symbol.WidthDegrees.ToString("N1", CultureInfo.InvariantCulture);
                _color = Symbol.FillColor;
                BtnColor.Background = new SolidColorBrush(_color);
            }
        }

        private void ApplyChanges() {
            if (Symbol == null) {
                return;
            }
            Symbol.RotationDegree = Angle;
            Symbol.FillColor = _color;

            var str = SizeValue.Text.Replace(',', '.');
            if (double.TryParse(str, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var sizeValue)) {
                Symbol.SizeMeter = Math.Max(0.0, sizeValue);
                SizeValue.Text = Symbol.SizeMeter.ToString("N0", CultureInfo.InvariantCulture);
            }

            str = WidthValue.Text.Replace(',', '.');
            if (double.TryParse(str, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var widthValue)) {
                Symbol.WidthDegrees = Math.Max(0.0, Math.Min(widthValue, 360.0));
                WidthValue.Text = Symbol.WidthDegrees.ToString("N1", CultureInfo.InvariantCulture);
            }

            SymbolsVM?.RaiseSymbolsChanged();
        }

        public Point DialogPos {
            set {  }
        }

        public DialogConeProp() {
			InitializeComponent();
            SizeValue.Focus();
        }

		private void BtnOk_Click(object sender, RoutedEventArgs e) {
			DialogResult = true;
            ApplyChanges();
            Close();
		}
        
        private void cmdRotateCCW_Click(object sender, RoutedEventArgs e) {
            Angle-= 22.5;
            ApplyChanges();
        }

        private void cmdRotateCW_Click(object sender, RoutedEventArgs e) {
            Angle+= 22.5;
            ApplyChanges();
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e) {
            ApplyChanges();
        }

        private void BtnColor_Click(object sender, RoutedEventArgs e) {
            var dialog = new DialogColorPicker { Owner = this };
            var result = dialog.ShowDialog();
            if (result == true) {
                _color = dialog.SelectedColor;
                BtnColor.Background = new SolidColorBrush(_color);
            }
        }
    }
}
