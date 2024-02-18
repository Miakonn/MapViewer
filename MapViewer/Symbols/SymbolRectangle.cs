using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using MapViewer.Dialogs;
using MapViewer.Maps;
using Point = System.Windows.Point;

namespace MapViewer.Symbols {
    [Serializable]
    public class SymbolRectangle : Symbol {
        private double _widthMeter;

        public double WidthMeter {
            get => _widthMeter;
            set => _widthMeter = Math.Max(value, 0.1);
        }

        public double RotationDegree { get; set; }

        public override void Draw(CanvasOverlay canvas, MapDrawSettings settings) {
            if (Hidden) {
                return;
            }

            double lengthPixel = SizeMeter / settings.ImageScaleMperPix;
            double widthPixel = WidthMeter / settings.ImageScaleMperPix;
            
            var shape = new Rectangle {
                Uid = Uid,
                Width = lengthPixel,
                Height = widthPixel,
                Fill = new SolidColorBrush(FillColor),
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
            if (Hidden) {
                return;
            }

            if (!IsSelected) {
                return;
            }

            double lengthPixel = SizeMeter / settings.ImageScaleMperPix;
            double widthPixel = WidthMeter / settings.ImageScaleMperPix;
            var shape = new Rectangle {
                Uid = Uid+ "_Selected1",
                Width = lengthPixel,
                Height = widthPixel,
                Stroke = new SolidColorBrush(Colors.Yellow),
                StrokeThickness = 2 / settings.ZoomScale,
                Opacity = 1.0,
                RenderTransform = new RotateTransform(RotationDegree, lengthPixel * 0.5, widthPixel * 0.5),
                 IsHitTestVisible = false,
           };

            Canvas.SetLeft(shape, StartPoint.X - lengthPixel * 0.5);
            Canvas.SetTop(shape, StartPoint.Y - widthPixel * 0.5);
            canvas.Children.Add(shape);

            shape = new Rectangle {
                Uid = Uid + "_Selected2",
                Width = lengthPixel,
                Height = widthPixel,
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 2 / settings.ZoomScale,
                StrokeDashArray = DoubleCollection.Parse("3, 3"),
                Opacity = 1.0,
                RenderTransform = new RotateTransform(RotationDegree, lengthPixel * 0.5, widthPixel * 0.5),
                IsHitTestVisible = false,
            };
            Canvas.SetLeft(shape, StartPoint.X - lengthPixel * 0.5);
            Canvas.SetTop(shape, StartPoint.Y - widthPixel * 0.5);
            canvas.Children.Add(shape);
        }


        public override bool OpenDialogProp(System.Windows.Window owner, Point dialogPos, SymbolsViewModel symbolsVM) {
            var dlg = new DialogRectProp {
                Owner = owner,
                Symbol = this,
                SymbolsVM = symbolsVM,
                DialogPos = dialogPos
            };

            var result = dlg.ShowDialog();
            return result != null && result.Value;
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