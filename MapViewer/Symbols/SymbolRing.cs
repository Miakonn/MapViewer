using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Point = System.Windows.Point;
using MapViewer.Maps;

namespace MapViewer.Symbols {
   
    public class SymbolRing : Symbol {

        public override void Draw(Canvas canvas, MapDrawSettings settings) {
  
            var shape = new Ellipse {
                Uid = Uid,
                Width = SizeMeter / settings.ImageScaleMperPix,
                Height = SizeMeter / settings.ImageScaleMperPix,
              
                Stroke = new SolidColorBrush(FillColor),
                StrokeThickness = 10 / settings.ZoomScale,
                Opacity = 0.6
            };

            Canvas.SetLeft(shape, StartPoint.X - shape.Width / 2);
            Canvas.SetTop(shape, StartPoint.Y - shape.Height / 2);  

            canvas.Children.Add(shape);
        }

        public override bool OpenDialogProp(Point dialogPos, SymbolsViewModel symbolsVM) {
            return false;
        }

        public override Symbol Copy() {
            var newSymbol = new SymbolCircle();
            newSymbol.CopyBase(this);
            return newSymbol;
        }
    }
}