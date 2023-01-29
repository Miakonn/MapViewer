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

        public override void Draw(CanvasOverlay canvas, MapDrawSettings settings)
        {
            var fontSize = 25 / settings.ZoomScale;
            var textBlock = new TextBlock {
                Uid = Uid,
                Text = Caption,
                FontSize = fontSize,
                Foreground = new SolidColorBrush(FillColor),
                FontWeight = FontWeights.UltraBold,
                Cursor = (settings.IsToolActive ? null : SymbolCursor)
            };

            var textSize = canvas.GetTextSize(textBlock);
            double scaleLength = SizeMeter / settings.ImageScaleMperPix / textSize.Width;

            // Reset size and angle
            textBlock.RenderTransform = new RotateTransform(RotationDegree, scaleLength * textSize.Width * 0.5, scaleLength * textSize.Height * 0.5);
            textBlock.FontSize = fontSize * scaleLength;

            textSize = canvas.GetTextSize(textBlock);

            Canvas.SetLeft(textBlock, StartPoint.X - textSize.Width / 2);
            Canvas.SetTop(textBlock, StartPoint.Y - textSize.Height / 2);
            canvas.Children.Add(textBlock);

            base.Draw(canvas, settings);
        }

        public override bool OpenDialogProp(Window owner, Point dialogPos, SymbolsViewModel symbolsVM) {
            var dlg = new DialogTextProp() {
                Owner = owner,
                Symbol = this,
                SymbolsVM = symbolsVM,
                DialogPos = dialogPos
            };

            var result = dlg.ShowDialog();
            return result != null && result.Value;
        }

        public override Symbol Copy(Vector offset) {
            var newSymbol = new SymbolText();
            newSymbol.CopyBase(this, offset);
            newSymbol.RotationDegree = RotationDegree;
            return newSymbol;
        }

        public override void Rotate(RotationDirection direction) {
            if (direction == RotationDirection.Clockwise) {
                RotationDegree += RotationStep;
            }
            else {
                RotationDegree -= RotationStep;
            }
        }
    }
}