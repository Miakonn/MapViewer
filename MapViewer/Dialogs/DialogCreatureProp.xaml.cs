using MapViewer.Symbols;
using System.Globalization;
using System.Windows;

namespace MapViewer.Dialogs {
	public partial class DialogCreatureProp {


        private SymbolCreature _symbol;
        
        public SymbolsViewModel SymbolsVM { get; set; }

        public SymbolCreature Symbol {
            get => _symbol;
            set {
                _symbol = value;
                CaptionValue.Text = Symbol.Caption;
                SizeValue.Text = Symbol.SizeMeter.ToString("N1", CultureInfo.InvariantCulture);
            }
        }



        private void ApplyChanges() {
            if (Symbol == null) {
                return;
            }
            Symbol.Caption = CaptionValue.Text;

            var str = SizeValue.Text.Replace(',', '.');
            if (double.TryParse(str, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var sizeM)) {
                Symbol.SizeMeter = sizeM;
                SizeValue.Text = Symbol.SizeMeter.ToString("N1", CultureInfo.InvariantCulture);
            }

            SymbolsVM?.RaiseSymbolsChanged();
        }


        public Point DialogPos {
            set { 
                Left = value.X + 10;
                Top = value.Y + 10;
            }
        }

        public DialogCreatureProp() {
			InitializeComponent();
            CaptionValue.Focus();
        }

		private void BtnOk_Click(object sender, RoutedEventArgs e) {
            ApplyChanges();
			DialogResult = true;
			Close();
		}


        private void BtnApply_Click(object sender, RoutedEventArgs e) {
            ApplyChanges();
        }
    }
}
