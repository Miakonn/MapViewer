using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using MapViewer.Dialogs;
using MapViewer.Maps;

namespace MapViewer.Symbols {
    public class SymbolCreature : Symbol {

        public override void DrawElements(Canvas canvas, MapDrawingSettings drawingSettings) {
            var brush = new SolidColorBrush(FillColor);

            double sizePixel = SizeMeter / drawingSettings.ImageScaleMperPix;
            double minSizeScaled = drawingSettings.MinCreatureSizePixel / drawingSettings.ZoomScale;
            sizePixel = Math.Max(sizePixel, minSizeScaled);

            var shape = new Ellipse {
                Uid = Uid,
                Width = sizePixel,
                Height = sizePixel,
                Fill = brush,
                Opacity = 1.0
            };

            Canvas.SetLeft(shape, StartPoint.X - shape.Width / 2);
            Canvas.SetTop(shape, StartPoint.Y - shape.Height / 2);

            canvas.Children.Add(shape);

            if (string.IsNullOrWhiteSpace(Caption)) {
                return;
            }

            var fontSize = 20 / drawingSettings.ZoomScale;
            var fontColor = Colors.Black;

            if (FillColor.Equals(Colors.Black) || FillColor.Equals(Colors.Purple) || FillColor.Equals(Colors.Blue)) {
                fontColor = Colors.Orange;
            }

            var textBlock = new TextBlock {
                Uid = Uid + "_1",
                Text = Caption,
                FontSize = fontSize,
                Foreground = new SolidColorBrush(fontColor),
                FontWeight = FontWeights.Normal,
                IsHitTestVisible = false
            };

            var textSize = canvas.GetTextSize(textBlock);
            Canvas.SetLeft(textBlock, StartPoint.X - textSize.Width / 2.0);
            Canvas.SetTop(textBlock, StartPoint.Y - textSize.Height / 2.0);
            canvas.Children.Add(textBlock);
        }


        public override bool OpenEditor(Point dialogPos, SymbolsViewModel symbolsVM) {
            var dlg = new DialogCreatureProp {
                Symbol = this,
                SymbolsVM = symbolsVM,
                DialogPos = dialogPos
            };


            var result = dlg.ShowDialog();
            return result != null && result.Value;
        }

        public override Symbol Copy() {
            var newSymbol = new SymbolCreature();
            newSymbol.CopyBase(this);
            newSymbol.Caption = SymbolsViewModel.CountUpCaption(Caption);

            return newSymbol;
        }
    }
}