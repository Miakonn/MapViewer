using System;
using System.Globalization;
using System.Windows;
using MapViewer.Symbols;

namespace MapViewer.Dialogs {
	public partial class DialogTextProp
    {
        private SymbolText _symbol;

        public double Angle { get; set; }

        public SymbolsViewModel SymbolsVM { get; set; }

        public SymbolText Symbol {
            get => _symbol;
            set {
                _symbol = value;
                SizeValue.Text = Symbol.SizeMeter.ToString("N1", CultureInfo.InvariantCulture);
                Angle = Symbol.RotationDegree;
                CaptionValue.Text = Symbol.Caption;
            }
        }


        public DialogTextProp() {
            InitializeComponent();
            SizeValue.Focus();
        }

        private void ApplyChanges() {
            if (Symbol == null) {
                return;
            }
            Symbol.RotationDegree = Angle;
            Symbol.Caption = CaptionValue.Text.Trim();

            var str = SizeValue.Text.Replace(',', '.');
            if (double.TryParse(str, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var sizeValue)) {
                Symbol.SizeMeter = Math.Max(0.0, sizeValue);
                SizeValue.Text = Symbol.SizeMeter.ToString("N0", CultureInfo.InvariantCulture);
            }

            SymbolsVM?.RaiseSymbolsChanged();
        }

        public Point StartPosition {
            set { 
                Left = value.X;
                Top = value.Y;
            }
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
