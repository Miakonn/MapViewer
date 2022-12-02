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

        private PointCollection _corners;

        public override void Draw(Canvas canvas, MapDrawSettings settings) {

            double sizePixel = SizeMeter / settings.ImageScaleMperPix;

            _corners = new PointCollection { new Point() };
            for (var a = -WidthDegrees / 2; a < WidthDegrees / 2; a+= 5.0) {
                var aRadian = (a + RotationDegree) * (Math.PI / 180.0);
                var pnt = new Point( sizePixel * Math.Cos(aRadian),  sizePixel * Math.Sin(aRadian) );
                _corners.Add(pnt);
            }

            var aRadianLast = (WidthDegrees / 2.0 + RotationDegree) * (Math.PI / 180) ;
            var pntLast = new Point(sizePixel * Math.Cos(aRadianLast), sizePixel * Math.Sin(aRadianLast));
            _corners.Add(pntLast);

            var shape = new Polygon {
                Uid = Uid,
                Points = _corners,
                Fill = new SolidColorBrush(FillColor),
                Opacity = 0.4,
                Cursor = (settings.IsToolActive ? null : SymbolCursor)
            };

            Canvas.SetLeft(shape, StartPoint.X);
            Canvas.SetTop(shape, StartPoint.Y);
            canvas.Children.Add(shape);

            base.Draw(canvas, settings);
        }


        public override void DrawSelected(Canvas canvas, MapDrawSettings settings) {
            if (!IsSelected) {
                return;
            }

            var shape = new Polygon {
                Uid = Uid + "_Selected1",
                Points = _corners,
                Stroke = new SolidColorBrush(Colors.Yellow),
                StrokeThickness = 2 / settings.ZoomScale,
                Opacity = 1.0,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(shape, StartPoint.X);
            Canvas.SetTop(shape, StartPoint.Y);
            canvas.Children.Add(shape);

            shape = new Polygon {
                Uid = Uid + "_Selected2",
                Points = _corners,
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 2 / settings.ZoomScale,
                StrokeDashArray = DoubleCollection.Parse("3, 3"),
                Opacity = 1.0,
                IsHitTestVisible = false
            };

            Canvas.SetLeft(shape, StartPoint.X);
            Canvas.SetTop(shape, StartPoint.Y);
            canvas.Children.Add(shape);
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
