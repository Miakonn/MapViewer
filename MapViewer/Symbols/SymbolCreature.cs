using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using MapViewer.Dialogs;
using MapViewer.Maps;

namespace MapViewer.Symbols {
    public class SymbolCreature : Symbol {

        public override void Draw(CanvasOverlay canvas, MapDrawSettings settings) {
            var brush = new SolidColorBrush(FillColor);

            var sizePixel = settings.GetMinSizePixelFromMeter(SizeMeter);

            var shape = new Ellipse {
                Uid = Uid,
                Width = sizePixel,
                Height = sizePixel,
                Fill = brush,
                Opacity = 1.0,
                Cursor = (settings.IsToolActive ? null : SymbolCursor)
            };

            Canvas.SetLeft(shape, StartPoint.X - shape.Width / 2);
            Canvas.SetTop(shape, StartPoint.Y - shape.Height / 2);

            canvas.Children.Add(shape);

            base.Draw(canvas, settings);
        }


        public override bool OpenDialogProp(Window owner, Point dialogPos, SymbolsViewModel symbolsVM) {
            var dlg = new DialogCreatureProp {
                Owner = owner,
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
            return newSymbol;
        }
    }
}