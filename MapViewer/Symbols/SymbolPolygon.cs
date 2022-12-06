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
    public class SymbolPolygon : Symbol {
        public PointCollection Corners { get; set; }
        
        public override void Draw(Canvas canvas, MapDrawSettings settings) {
            var shape = new Polygon {
                Uid = Uid,
                Points = Corners,
                Fill = new SolidColorBrush(FillColor),
                Opacity = 0.4,
                Cursor = (settings.IsToolActive ? null : SymbolCursor)
            };

            Canvas.SetLeft(shape, StartPoint.X);
            Canvas.SetTop(shape, StartPoint.Y);
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
            var newSymbol = new SymbolPolygon();
            newSymbol.CopyBase(this);
            newSymbol.Corners = Corners.Clone();
            return newSymbol;
        }
    }
}