using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using MapViewer.Symbols;

namespace MapViewer.Dialogs {
	public partial class DialogBaseSymbolProp {
        private Symbol _symbol;


        private Color _color;

        public SymbolsViewModel SymbolsVM { get; set; }

        public Symbol Symbol {
            get => _symbol;
            set {
                _symbol = value;
                SizeValue.Text = Symbol.SizeMeter.ToString("N1", CultureInfo.InvariantCulture);
                CaptionValue.Text = Symbol.Caption;
                _color = Symbol.FillColor;
                BtnColor.Background = new SolidColorBrush(_color);
                ControlOpacity.ComboBoxOpacity.Text = Symbol.OpacityPercent;
            }
        }
        
        public DialogBaseSymbolProp() {
            InitializeComponent();
            SizeValue.Focus();
        }

        private void ApplyChanges() {
            if (Symbol == null) {
                return;
            }

            Symbol.Caption = CaptionValue.Text.Trim();
            Symbol.FillColor = _color;
            Symbol.OpacityPercent = ControlOpacity.ComboBoxOpacity.Text;

            var str = SizeValue.Text.Replace(',', '.');
            if (double.TryParse(str, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var sizeValue)) {
                Symbol.SizeMeter = Math.Max(0.0, sizeValue);
                SizeValue.Text = Symbol.SizeMeter.ToString("N0", CultureInfo.InvariantCulture);
            }


            SymbolsVM?.RaiseSymbolsChanged();
        }

        public Point DialogPos {
            set {             }
        }

		private void BtnOk_Click(object sender, RoutedEventArgs e) {
			DialogResult = true;
            ApplyChanges();
            Close();
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
