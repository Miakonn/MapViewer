using System;
using System.Globalization;
using System.Windows;
using MapViewer.Symbols;

namespace MapViewer.Dialogs {
	public partial class DialogConeProp
    {
        private SymbolCone _symbol;

        public double Angle { get; set; }

        public SymbolsViewModel SymbolsVM { get; set; }
        
        public SymbolCone Symbol {
            get => _symbol;
            set {
                _symbol = value;
                SizeValue.Text = Symbol.SizeMeter.ToString("N1", CultureInfo.InvariantCulture);
                Angle = Symbol.RotationDegree;
                WidthValue.Text = Symbol.WidthDegrees.ToString("N1", CultureInfo.InvariantCulture);
                ControlColorPicker.SelectedColor = Symbol.FillColor;
                ControlOpacity.ComboBoxOpacity.Text = Symbol.OpacityPercent;
                LockedPos.IsChecked = Symbol.LockedPosition;
            }
        }

        private void ApplyChanges() {
            if (Symbol == null) {
                return;
            }
            Symbol.RotationDegree = Angle;
            Symbol.FillColor = ControlColorPicker.SelectedColor;
            Symbol.OpacityPercent = ControlOpacity.ComboBoxOpacity.Text;
            Symbol.LockedPosition = LockedPos.IsChecked ?? false;

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
            DataContext = this;
        }

		private void BtnOk_Click(object sender, RoutedEventArgs e) {
			DialogResult = true;
            ApplyChanges();
            Close();
		}
        
        private void CmdRotateCCW_Click(object sender, RoutedEventArgs e) {
            Angle-= 22.5;
            ApplyChanges();
        }

        private void CmdRotateCW_Click(object sender, RoutedEventArgs e) {
            Angle+= 22.5;
            ApplyChanges();
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e) {
            ApplyChanges();
        }
    }
}
