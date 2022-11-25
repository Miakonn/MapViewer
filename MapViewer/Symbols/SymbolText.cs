using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Serialization;
using MapViewer.Dialogs;
using MapViewer.Maps;

namespace MapViewer.Symbols {
    [Serializable]
    [XmlInclude(typeof(Symbol))]
    public class SymbolText : Symbol {
        public double RotationDegree { get; set; }

        public override void CreateElements(Canvas canvas, MapDrawingSettings drawingSettings)
        {
            var fontSize = 25 / drawingSettings.ZoomScale;
            var textBlock = new TextBlock {
                Uid = Uid,
                Text = Caption,
                FontSize = fontSize,
                Foreground = new SolidColorBrush(FillColor),
                FontWeight = FontWeights.UltraBold,
            };

            var textSize = canvas.GetTextSize(textBlock);
            double scaleLength = SizeMeter / drawingSettings.ImageScaleMperPix / textSize.Width;

            // Reset size and angle
            textBlock.RenderTransform = new RotateTransform(RotationDegree, scaleLength * textSize.Width * 0.5, scaleLength * textSize.Height * 0.5);
            textBlock.FontSize = fontSize * scaleLength;

            textSize = canvas.GetTextSize(textBlock);

            Canvas.SetLeft(textBlock, StartPoint.X - textSize.Width / 2);
            Canvas.SetTop(textBlock, StartPoint.Y - textSize.Height / 2);
            canvas.Children.Add(textBlock);
        }

        public override bool OpenEditor(Point mouseDownPoint, SymbolsViewModel symbolsVM) {
            var dlg = new DialogTextProp() {
                Symbol = this,
                SymbolsVM = symbolsVM,
                StartPosition = mouseDownPoint
            };

            var result = dlg.ShowDialog();
            return result != null && result.Value;
        }

        public override Symbol Copy() {
            var newSymbol = new SymbolText();
            newSymbol.CopyBase(this);
            newSymbol.Caption = Caption;
            newSymbol.RotationDegree = RotationDegree;
            newSymbol.SizeMeter = SizeMeter;
            return newSymbol;
        }
    }
}