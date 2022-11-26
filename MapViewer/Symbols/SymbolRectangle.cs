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
    public class SymbolRectangle : Symbol {
        public double WidthMeter { get; set; }
        
        public double RotationDegree { get; set; }
        public override void DrawElements(Canvas canvas, MapDrawingSettings drawingSettings) {
            var corners = new PointCollection();

            double lengthPixel = SizeMeter / drawingSettings.ImageScaleMperPix * 0.5;
            double widthPixel = WidthMeter / drawingSettings.ImageScaleMperPix * 0.5;

            var angleRadians = SymbolsViewModel.ToRadians(RotationDegree);

            var vectLength = new Vector(lengthPixel * Math.Cos(angleRadians), lengthPixel * Math.Sin(angleRadians));
            var vectWidth = new Vector(-widthPixel * Math.Sin(angleRadians), widthPixel * Math.Cos(angleRadians));

            var c1 =  vectLength + vectWidth;
            corners.Add(new Point(c1.X, c1.Y));
            var c2 = -vectLength + vectWidth;
            corners.Add(new Point(c2.X, c2.Y));
            var c3 = -vectLength - vectWidth;
            corners.Add(new Point(c3.X, c3.Y));
            var c4 = vectLength - vectWidth;
            corners.Add(new Point(c4.X, c4.Y));

            var shape = new Polygon {
                Uid = Uid,
                Points = corners,
                Fill = new SolidColorBrush(FillColor),
                Opacity = 0.4
            };
            Canvas.SetLeft(shape, StartPoint.X);
            Canvas.SetTop(shape, StartPoint.Y);
            canvas.Children.Add(shape);

            DrawTextElement(Caption, canvas, drawingSettings);
        }

        public override bool OpenDialogProp(Point dialogPos, SymbolsViewModel symbolsVM) {
            var dlg = new DialogRectProp {
                Symbol = this,
                SymbolsVM = symbolsVM,
                DialogPos = dialogPos
            };

            var result = dlg.ShowDialog();
            return result != null && result.Value;
        }

        public override Symbol Copy() {
            var newSymbol = new SymbolRectangle();
            newSymbol.CopyBase(this);
            newSymbol.WidthMeter = WidthMeter;
            newSymbol.RotationDegree = RotationDegree;
            newSymbol.Caption = SymbolsViewModel.CountUpCaption(Caption);
            return newSymbol;
        }
    }
}