using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using MapViewer.Maps;
using Point = System.Windows.Point;

namespace MapViewer.Symbols {
    [Serializable]
    public class SymbolFrame : Symbol {
        public double WidthMeter { get; set; }
        
        public double RotationDegree { get; set; }

        public override void Draw(CanvasOverlay canvas, MapDrawSettings settings) {
            if (Hidden) {
                return;
            }

            var thickness = 10 / settings.ZoomScale;
            double lengthPixel = SizeMeter / settings.ImageScaleMperPix + thickness * 2;
            double widthPixel = WidthMeter / settings.ImageScaleMperPix + thickness * 2;
            
            var shape = new Rectangle {
                Uid = Uid,
                Width = lengthPixel,
                Height = widthPixel,
                Stroke = new SolidColorBrush(FillColor),
                StrokeThickness = thickness,
                Opacity = Opacity,
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

        public override bool OpenDialogProp(Window owner, Point dialogPos, SymbolsViewModel symbolsVM) {
            return false;
        }

        public override Symbol Copy(Vector offset) {
            var newSymbol = new SymbolRectangle();
            newSymbol.CopyBase(this, offset);
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