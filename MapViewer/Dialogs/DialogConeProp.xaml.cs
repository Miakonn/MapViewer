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
                SizeValue.Text = Symbol.SizeMeter.ToString(CultureInfo.InvariantCulture);
                Angle = Symbol.RotationAngle;
                WidthValue.Text = Symbol.WidthDegrees.ToString(CultureInfo.InvariantCulture);
            }
        }

        private void ApplyChanges() {
            if (Symbol == null) {
                return;
            }
            Symbol.RotationAngle = Angle;

            var str = SizeValue.Text.Replace(',', '.');
            if (double.TryParse(str, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var sizeValue)) {
                Symbol.SizeMeter = Math.Max(0.0, sizeValue);
                SizeValue.Text = Symbol.SizeMeter.ToString(CultureInfo.InvariantCulture);
            }

            str = WidthValue.Text.Replace(',', '.');
            if (double.TryParse(str, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var widthValue)) {
                Symbol.WidthDegrees = Math.Max(0.0, Math.Min(widthValue, 360.0));
                WidthValue.Text = Symbol.WidthDegrees.ToString(CultureInfo.InvariantCulture);
            }

            SymbolsVM?.RaiseSymbolsChanged();
        }

        public Point StartPosition {
            set { 
                Left = value.X;
                Top = value.Y;
            }
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
    }
}
