using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using MapViewer.Dialogs;
using MapViewer.Maps;
using Point = System.Windows.Point;

namespace MapViewer.Symbols {
    [Serializable]
    public class SymbolFrame : Symbol {
        public double WidthMeter { get; set; }
        
        public double RotationDegree { get; set; }

        public override void Draw(Canvas canvas, MapDrawSettings settings) {
            var thickness = 10 / settings.ZoomScale;
            double lengthPixel = SizeMeter / settings.ImageScaleMperPix + thickness * 2;
            double widthPixel = WidthMeter / settings.ImageScaleMperPix + thickness * 2;
            
            var shape = new Rectangle {
                Uid = Uid,
                Width = lengthPixel,
                Height = widthPixel,
                Stroke = new SolidColorBrush(FillColor),
                StrokeThickness = thickness,
                Opacity = 0.5,
                RenderTransform = new RotateTransform(RotationDegree, lengthPixel * 0.5, widthPixel * 0.5),
                Cursor = (settings.IsToolActive ? null : SymbolCursor)
            };

            Canvas.SetLeft(shape, StartPoint.X - lengthPixel * 0.5);
            Canvas.SetTop(shape, StartPoint.Y - widthPixel * 0.5);
            canvas.Children.Add(shape);

            base.Draw(canvas, settings);
        }

        
        public override void DrawSelected(Canvas canvas, MapDrawSettings settings) {
            return;
        }


        public override bool OpenDialogProp(Point dialogPos, SymbolsViewModel symbolsVM) {
            return false;
        }

        public override Symbol Copy() {
            var newSymbol = new SymbolRectangle();
            newSymbol.CopyBase(this);
            newSymbol.WidthMeter = WidthMeter;
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