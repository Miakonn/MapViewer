using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using MapViewer.Symbols;

namespace MapViewer.Dialogs {
	public partial class DialogRectProp
    {
     
        private SymbolRectangle _symbol;
        private Color _color;

        public double Angle { get; set; }

        public SymbolsViewModel SymbolsVM { get; set; }

        public SymbolRectangle Symbol {
            get => _symbol;
            set {
                _symbol = value;
                CaptionValue.Text = Symbol.Caption;
                SizeValue.Text = Symbol.SizeMeter.ToString("N1", CultureInfo.InvariantCulture);
                WidthValue.Text = Symbol.WidthMeter.ToString("N1", CultureInfo.InvariantCulture);
                Angle = (int)Symbol.RotationDegree;
                _color = Symbol.FillColor;
                BtnColor.Background = new SolidColorBrush(_color);
            }
        }


        private void ApplyChanges() {
            if (Symbol == null) {
                return;
            }
            Symbol.Caption = CaptionValue.Text;
            Symbol.RotationDegree = Angle;
            Symbol.FillColor = _color;

            var str = SizeValue.Text.Replace(',', '.');
            if (double.TryParse(str, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var sizeM)) {
                Symbol.SizeMeter = sizeM;
                SizeValue.Text = Symbol.SizeMeter.ToString("N1", CultureInfo.InvariantCulture);
            }

            str = WidthValue.Text.Replace(',', '.');
            if (double.TryParse(str, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var widthM)) {
                Symbol.WidthMeter = widthM;
                WidthValue.Text = Symbol.WidthMeter.ToString("N1", CultureInfo.InvariantCulture);
            }

            SymbolsVM?.RaiseSymbolsChanged();
        }

        public Point DialogPos {
            set {      }
        }


        public DialogRectProp() {
			InitializeComponent();
            CaptionValue.Focus();
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
