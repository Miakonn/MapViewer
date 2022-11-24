using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Serialization;
using MapViewer.Maps;

namespace MapViewer.Symbols {
    [Serializable]
    [XmlInclude(typeof(Symbol))]
    public class SymbolLine : Symbol {
        public double Width { get; set; }
        public Point EndPoint { get; set; }

        public override void CreateElements(Canvas canvas, MapDrawingSettings drawingSettings)
        {
            var shape = new Line {
                Uid = Uid,
                X1 = 0,
                Y1 = 0,
                X2 = EndPoint.X,
                Y2 = EndPoint.Y,
                StrokeThickness = Width / drawingSettings.ImageScaleMperPix,
                Stroke = new SolidColorBrush(FillColor),
                Opacity = 0.4

            };
            Canvas.SetLeft(shape, StartPoint.X);
            Canvas.SetTop(shape, StartPoint.Y);
            canvas.Children.Add(shape);
        }

        public override Symbol Copy() {
            var newSymbol = new SymbolLine();
            newSymbol.CopyBase(this);
            newSymbol.Width = Width;
            newSymbol.EndPoint = EndPoint;
            return newSymbol;
        }
    }
}