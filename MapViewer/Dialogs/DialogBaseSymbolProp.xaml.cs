using System;
using System.Globalization;
using System.Windows;
using MapViewer.Symbols;

namespace MapViewer.Dialogs {
	public partial class DialogBaseSymbolProp {
        private Symbol _symbol;

        public SymbolsViewModel SymbolsVM { get; set; }

        public Symbol Symbol {
            get => _symbol;
            set {
                _symbol = value;
                SizeValue.Text = Symbol.SizeMeter.ToString("N1", CultureInfo.InvariantCulture);
                CaptionValue.Text = Symbol.Caption;
                ControlColorPicker.SelectedColor = Symbol.FillColor;
                ControlOpacity.ComboBoxOpacity.Text = Symbol.OpacityPercent;
                LockedPos.IsChecked = Symbol.LockedPosition;
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
            Symbol.FillColor = ControlColorPicker.SelectedColor;
            Symbol.OpacityPercent = ControlOpacity.ComboBoxOpacity.Text;
            Symbol.LockedPosition = LockedPos.IsChecked ?? false;

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
    }
}
