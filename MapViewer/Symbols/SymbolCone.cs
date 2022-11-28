using MapViewer.Maps;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Serialization;
using MapViewer.Dialogs;

namespace MapViewer.Symbols {
    [Serializable]
    [XmlInclude(typeof(Symbol))]
    public class SymbolCone : Symbol {
        public double WidthDegrees { get; set; }

        public double RotationDegree { get; set; }

        public override void Draw(Canvas canvas, MapDrawingSettings drawingSettings) {

            double sizePixel = SizeMeter / drawingSettings.ImageScaleMperPix;

            var corners = new PointCollection { new Point() };
            for (var a = -WidthDegrees / 2; a < WidthDegrees / 2; a+= 5.0) {
                var aRadian = (a + RotationDegree) * (Math.PI / 180.0);
                var pnt = new Point( sizePixel * Math.Cos(aRadian),  sizePixel * Math.Sin(aRadian) );
                corners.Add(pnt);
            }

            var aRadianLast = (WidthDegrees / 2.0 + RotationDegree) * (Math.PI / 180) ;
            var pntLast = new Point(sizePixel * Math.Cos(aRadianLast), sizePixel * Math.Sin(aRadianLast));
            corners.Add(pntLast);

            var shape = new Polygon {
                Uid = Uid,
                Points = corners,
                Fill = new SolidColorBrush(FillColor),
                Opacity = 0.4,
                Cursor = SymbolCursor
            };
            Canvas.SetLeft(shape, StartPoint.X);
            Canvas.SetTop(shape, StartPoint.Y);
            canvas.Children.Add(shape);

            base.Draw(canvas, drawingSettings);
        }

        public override bool OpenDialogProp(Point dialogPos, SymbolsViewModel symbolsVM) {
            var dlg = new DialogConeProp {
                Symbol = this,
                SymbolsVM = symbolsVM,
                DialogPos = dialogPos
            };

            var result = dlg.ShowDialog();
            return result != null && result.Value;
        }

        public override Symbol Copy() {
            var newSymbol = new SymbolCone();
            newSymbol.CopyBase(this);
            newSymbol.WidthDegrees = WidthDegrees;
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
