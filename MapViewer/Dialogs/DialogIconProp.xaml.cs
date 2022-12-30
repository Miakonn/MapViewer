using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using MapViewer.Symbols;

namespace MapViewer.Dialogs {
	public partial class DialogIconProp {
        private SymbolIcon _symbol;

        public double Angle { get; set; }

        public SymbolsViewModel SymbolsVM { get; set; }

        private static string _lastDirectoryUsed;

        public SymbolIcon Symbol {
            get => _symbol;
            set {
                _symbol = value;
                CaptionValue.Text = Symbol.Caption;
                CommentValue.Text = Symbol.Comment;
                SizeValue.Text = Symbol.SizeMeter.ToString("N1", CultureInfo.InvariantCulture);
                Angle = (int)Symbol.RotationDegree;
                FilenameValue.Text = Symbol.ImageFileName;
            }
        }

        private void ApplyChanges() {
            if (Symbol == null) {
                return;
            }
            Symbol.Caption = CaptionValue.Text;
            Symbol.Comment = CommentValue.Text;
            Symbol.RotationDegree = Angle;
            Symbol.ImageFileName = FilenameValue.Text;

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

        public DialogIconProp() {
			InitializeComponent();
            CaptionValue.Focus();
        }

		private void BtnOk_Click(object sender, RoutedEventArgs e) {
			DialogResult = true;
            ApplyChanges();
            Close();
		}

        private void Browse_OnClick(object sender, RoutedEventArgs e) {
            var dialog = new OpenFileDialog {
                Title = "Select Image",
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = "png",
                Filter = "Images (*.BMP;*.JPG;*.GIF,*.PNG,*.TIFF)|*.BMP;*.JPG;*.GIF;*.PNG;*.TIFF|" +
                         "All files (*.*)|*.*"
            };

            if (!string.IsNullOrWhiteSpace(_lastDirectoryUsed)) {
                dialog.InitialDirectory = _lastDirectoryUsed;
            }

            var result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK) {
                return;
            }

            FilenameValue.Text = dialog.FileName;
            var filename = Path.GetFileNameWithoutExtension(dialog.FileName);
            var parts = filename.Split(new[]{'[', ']'}, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2) {
                SizeValue.Text = parts[1];
            }

            _lastDirectoryUsed = Path.GetDirectoryName(dialog.FileName);
            ApplyChanges();
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
