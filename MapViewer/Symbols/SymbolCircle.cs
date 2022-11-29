using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Serialization;
using MapViewer.Dialogs;
using MapViewer.Maps;

namespace MapViewer.Symbols {
    [Serializable]
    [XmlInclude(typeof(Symbol))]
    public class SymbolCircle : Symbol {

        public override void Draw(Canvas canvas, MapDrawingSettings settings)
        {
            var brush = new SolidColorBrush(FillColor);

            var shape = new Ellipse {
                Uid = Uid,
                Width = SizeMeter / settings.ImageScaleMperPix,
                Height = SizeMeter / settings.ImageScaleMperPix,
                Fill = brush,
                Opacity = 1.0,
                Cursor = (settings.IsToolActive ? null: SymbolCursor)
            };

            Canvas.SetLeft(shape, StartPoint.X - shape.Width / 2);
            Canvas.SetTop(shape, StartPoint.Y - shape.Height / 2);  

            canvas.Children.Add(shape);

            base.Draw(canvas, settings);
        }

        public override bool OpenDialogProp(Point dialogPos, SymbolsViewModel symbolsVM) {
            var dlg = new DialogBaseSymbolProp() {
                Symbol = this,
                SymbolsVM = symbolsVM,
                DialogPos = dialogPos
            };

            var result = dlg.ShowDialog();
            return result != null && result.Value;
        }

        public override Symbol Copy() {
            var newSymbol = new SymbolCircle();
            newSymbol.CopyBase(this);
            return newSymbol;
        }
    }
}