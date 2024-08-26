using MapViewer.Maps;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Serialization;
using MapViewer.Utilities;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;

namespace MapViewer.Symbols {

    public enum RotationDirection {
        Clockwise, CounterClockwise
    }

    [Serializable]
    [XmlInclude(typeof(SymbolCreature)), XmlInclude(typeof(SymbolIcon)), XmlInclude(typeof(SymbolCone)),
     XmlInclude(typeof(SymbolCircle)), XmlInclude(typeof(SymbolText)), XmlInclude(typeof(SymbolRectangle))]
    public abstract class Symbol : ICloneable {
        public const double RotationStep = 22.5;

        protected static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
        
        [XmlIgnore]
        public static Cursor SymbolCursor = Cursors.Hand;

        private double _sizeMeter;

        public string Uid { get; set; }
        public int OrderZ { get; set; }   
        public Point StartPoint { get; set; }
        public Color FillColor { get; set; }
        public double Opacity { get; set; } = 1.0;
        public bool Hidden { get; set; }

        public bool LockedPosition { get; set; }

        [XmlIgnore]
        public string OpacityPercent {
            get => $"{Math.Round(Opacity * 100)}%";
            set {
                if (int.TryParse(value.TrimEnd('%'), out int opResult)) {
                    Opacity = opResult / 100.0;
                }
            }
        }

        public double SizeMeter {
            get => _sizeMeter;
            set => _sizeMeter = Math.Max(value, 0.1);
        }

        public string Caption { get; set; }
        public string Comment { get; set; }

        public string DisplayName {
            get {
                if (!string.IsNullOrWhiteSpace(Caption)) {
                    return Caption;
                }

                if (string.IsNullOrWhiteSpace(ImageFileName)) {
                    return string.Empty;
                }
                var filename = System.IO.Path.GetFileNameWithoutExtension(ImageFileName);
                var parts = filename.Split('[');
                return parts[0].Trim();
            }
        }

        public string ImageFileName { get; set; }

        [XmlIgnore]
        public bool IsSelected{ get; set; }

        public string Status { get; set; }

        public virtual void Draw(CanvasOverlay canvas, MapDrawSettings settings) {
            if (Hidden) {
                return;
            }

            if (!(this is SymbolText)) {
                DrawText(Caption, canvas, settings);
            }

            if (Status != null) {
                if (Status.EndsWith("Cross")) {
                    DrawStatusCross(canvas, settings, GetStatusColor(Status));
                }
                else if (Status.EndsWith("Circle")) {
                    DrawStatusCircle(canvas, settings, GetStatusColor(Status));
                }
            }

            if (IsSelected) {
                DrawSelected(canvas, settings);
            }
        }

        public virtual bool OpenDialogProp(Window owner, Point dialogPos, SymbolsViewModel symbolsVM) {
            return true;
        }

        public virtual void Rotate(RotationDirection direction) {}

        public abstract Symbol Copy(Vector offset);

        public void CopyBase(Symbol symbolSource, Vector offset) {
            Uid = SymbolsViewModel.GetTimestamp();
            StartPoint = symbolSource.StartPoint + offset;
            FillColor = symbolSource.FillColor;
            OrderZ = symbolSource.OrderZ;
            SizeMeter = symbolSource.SizeMeter;
            Caption = symbolSource.Caption;
            Opacity = symbolSource.Opacity;
        }

        public void DrawText(string caption, CanvasOverlay canvas, MapDrawSettings drawSettings) {
            if (string.IsNullOrWhiteSpace(caption)) {
                return;
            }
 
            var textBlock = new OutlineTextControl {
                Caption = caption,
                FontSize = 20 / drawSettings.ZoomScale,
                StrokeThickness = 1.2 / drawSettings.ZoomScale,
                Uid = Uid
            };

            var rect = textBlock.CreateText();

            var nudge = 2 / drawSettings.ZoomScale;

            Canvas.SetLeft(textBlock, StartPoint.X - (rect.Width) / 2.0 - nudge);
            Canvas.SetTop(textBlock, StartPoint.Y - (rect.Height)  / 2.0 - nudge);
            canvas.Children.Add(textBlock);
        }

        protected double CalcLuma(Color color) {
            return (0.2126 * color.ScR + 0.7152 * color.ScG + 0.0722 * color.ScB);
        }

        protected Color CalculatingContrastingColor() {
            if (FillColor.ScA == 0.0) {
                return Colors.Black;
            }
            var blackLuma = 0.05;
            var orangeLuma = CalcLuma(Colors.Orange) + 0.05;

            var fillLuma = CalcLuma(FillColor) + 0.05;
            var blackRatio = (fillLuma / blackLuma);
            var orangeRatio = (orangeLuma / fillLuma);
            return blackRatio > orangeRatio ? Colors.Black : Colors.Orange;
        }

        public virtual void DrawSelected(Canvas canvas, MapDrawSettings drawSettings) {
            if (!IsSelected) {
                return;
            }
            var sizePixel = drawSettings.GetMinSizePixelFromMeter(SizeMeter);

            var shape = new Ellipse {
                Uid = Uid + "_Selected1",
                Width = sizePixel,
                Height = sizePixel,
                Stroke = new SolidColorBrush(Colors.Yellow),
                StrokeThickness = 2 / drawSettings.ZoomScale,
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
                StrokeThickness = 2 / drawSettings.ZoomScale,
                StrokeDashArray = DoubleCollection.Parse("3, 3"),
                Opacity = 1.0,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(shape, StartPoint.X - shape.Width / 2);
            Canvas.SetTop(shape, StartPoint.Y - shape.Height / 2);
            canvas.Children.Add(shape);
        }

        public object Clone() {
            return (Symbol)MemberwiseClone();
        }

        protected void DrawStatusCross(CanvasOverlay canvas, MapDrawSettings drawSettings, Color color) {

            var sizePixel = drawSettings.GetMinSizePixelFromMeter(SizeMeter);

            var thickness = Math.Max(sizePixel * 0.1, 1.5 / drawSettings.ZoomScale);
            var circleCorner = sizePixel * 0.146; // (2 - sqr_root(2)) / 4;

            var brush = new SolidColorBrush(color);

            var line1 = new Line {
                Uid = Uid,
                X1 = circleCorner,
                Y1 = circleCorner,
                X2 = sizePixel - circleCorner,
                Y2 = sizePixel - circleCorner,
                Stroke = brush,
                StrokeThickness = thickness,
                Opacity = 1.0,
                IsHitTestVisible = false
            };

            Canvas.SetLeft(line1, StartPoint.X - sizePixel * 0.5);
            Canvas.SetTop(line1, StartPoint.Y - sizePixel * 0.5);
            canvas.Children.Add(line1);

            var line2 = new Line {
                Uid = Uid,
                X1 = circleCorner,
                Y1 = sizePixel - circleCorner,
                X2 = sizePixel - circleCorner,
                Y2 = circleCorner,
                Stroke = brush,
                StrokeThickness = thickness,
                Opacity = 1.0,
                IsHitTestVisible = false
            };

            Canvas.SetLeft(line2, StartPoint.X - sizePixel * 0.5);
            Canvas.SetTop(line2, StartPoint.Y - sizePixel * 0.5);
            canvas.Children.Add(line2);
        }

        private void DrawStatusCircle(Canvas canvas, MapDrawSettings drawSettings, Color color) {
            var sizePixel = drawSettings.GetMinSizePixelFromMeter(SizeMeter);
            var thickness = Math.Max(sizePixel * 0.1, 1.5 / drawSettings.ZoomScale);
            var shape = new Ellipse {
                Uid = Uid + "_Status",
                Width = sizePixel,
                Height = sizePixel,
                Stroke = new SolidColorBrush(color),
                StrokeThickness = thickness,
                Opacity = 1.0,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(shape, StartPoint.X - shape.Width / 2);
            Canvas.SetTop(shape, StartPoint.Y - shape.Height / 2);
            canvas.Children.Add(shape);
        }

        private Color GetStatusColor(string status) {
            var parts = status.Split(' ');

            if (parts.Length == 1) {
                return Colors.Red;
            }

            // ReSharper disable once PossibleNullReferenceException
            return (Color)ColorConverter.ConvertFromString(parts[0]);
        }

    }
}

