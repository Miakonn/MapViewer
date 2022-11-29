using MapViewer.Maps;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Serialization;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;

namespace MapViewer.Symbols {

    public enum RotationDirection {
        Clockwise, CounterClockwise
    }

    [Serializable]
    [XmlInclude(typeof(SymbolCreature)), XmlInclude(typeof(SymbolPolygon)), XmlInclude(typeof(SymbolIcon)),
     XmlInclude(typeof(SymbolCone)),
     XmlInclude(typeof(SymbolCircle)), XmlInclude(typeof(SymbolText)), XmlInclude(typeof(SymbolRectangle))]
    public abstract class Symbol {
        public const double RotationStep = 22.5;

        [XmlIgnore]
        public static Cursor SymbolCursor = Cursors.Hand;

        public string Uid { get; set; }
        public int OrderZ { get; set; }   
        public Point StartPoint { get; set; }
        public Color FillColor { get; set; }
        public double SizeMeter { get; set; }
        public string Caption { get; set; }

        [XmlIgnore]
        public bool IsSelected{ get; set; }

        public virtual void Draw(Canvas canvas, MapDrawingSettings settings) {
            if (!(this is SymbolText)) {
                DrawText(Caption, canvas, settings);
            }

            if (IsSelected) {
                DrawSelected(canvas, settings);
            }
        }

        public virtual bool OpenDialogProp(Point dialogPos, SymbolsViewModel symbolsVM) {
            return true;
        }

        public virtual void Rotate(RotationDirection direction) {}

        public abstract Symbol Copy();

        public void CopyBase(Symbol symbolSource) {
            Uid = SymbolsViewModel.GetTimestamp();
            StartPoint = symbolSource.StartPoint + new Vector(50, 50);
            FillColor = symbolSource.FillColor;
            OrderZ = symbolSource.OrderZ - 1;
            SizeMeter = symbolSource.SizeMeter;
            Caption = SymbolsViewModel.CountUpCaption(symbolSource.Caption);
        }

        public void DrawText(string caption, Canvas canvas, MapDrawingSettings drawingSettings) {
            if (string.IsNullOrWhiteSpace(caption)) {
                return;
            }
            var fontSize = 20 / drawingSettings.ZoomScale;
            var fontColor = Colors.Black;

            var textBlock = new TextBlock {
                Uid = Uid + "_Text",
                Text = caption,
                FontSize = fontSize,
                Foreground = new SolidColorBrush(fontColor),
                FontWeight = FontWeights.Normal,
                IsHitTestVisible = false
            };

            var textSize = canvas.GetTextSize(textBlock);
            Canvas.SetLeft(textBlock, StartPoint.X - textSize.Width / 2.0);
            Canvas.SetTop(textBlock, StartPoint.Y - textSize.Height / 2.0);
            canvas.Children.Add(textBlock);
        }


        public void DrawSelected(Canvas canvas, MapDrawingSettings drawingSettings) {
            if (!IsSelected) {
                return;
            }
            var sizePixel = drawingSettings.GetMinSizePixelFromMeter(SizeMeter);

            var shape = new Ellipse {
                Uid = Uid + "_Selected1",
                Width = sizePixel,
                Height = sizePixel,
                Stroke = new SolidColorBrush(Colors.Yellow),
                StrokeThickness = 2 / drawingSettings.ZoomScale,
                StrokeDashArray = DoubleCollection.Parse("3, 3"),
                StrokeDashOffset = 0,
                Opacity = 1.0,
                IsHitTestVisible = false
            };

            Canvas.SetLeft(shape, StartPoint.X - shape.Width / 2);
            Canvas.SetTop(shape, StartPoint.Y - shape.Height / 2);

            canvas.Children.Add(shape);
            
            shape = new Ellipse {
                Uid = Uid + "_Selected2",
                Width = sizePixel,
                Height = sizePixel,
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 2 / drawingSettings.ZoomScale,
                StrokeDashArray = DoubleCollection.Parse("3, 3"),
                StrokeDashOffset = 3,
                Opacity = 1.0,
                IsHitTestVisible = false
            };
           

            Canvas.SetLeft(shape, StartPoint.X - shape.Width / 2);
            Canvas.SetTop(shape, StartPoint.Y - shape.Height / 2);

            canvas.Children.Add(shape);
        }
    }
}

