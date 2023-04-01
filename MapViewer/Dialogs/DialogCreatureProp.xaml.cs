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
                CommentValue.Text = Symbol.Comment;
                ControlColorPicker.SelectedColor = Symbol.FillColor;
                ControlOpacity.ComboBoxOpacity.Text = Symbol.OpacityPercent;
            }
        }

        private void ApplyChanges() {
            if (Symbol == null) {
                return;
            }
            Symbol.Caption = CaptionValue.Text;
            Symbol.Comment = CommentValue.Text;
            Symbol.FillColor = ControlColorPicker.SelectedColor;
            Symbol.OpacityPercent = ControlOpacity.ComboBoxOpacity.Text;

            var str = SizeValue.Text.Replace(',', '.');
            if (double.TryParse(str, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var sizeM)) {
                Symbol.SizeMeter = sizeM;
                SizeValue.Text = Symbol.SizeMeter.ToString("N1", CultureInfo.InvariantCulture);
            }

            SymbolsVM?.RaiseSymbolsChanged();
        }


        public Point DialogPos {
            set {   }
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
