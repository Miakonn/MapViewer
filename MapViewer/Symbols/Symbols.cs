using MapViewer.Maps;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Serialization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace MapViewer.Symbols {


    [Serializable]
    [XmlInclude(typeof(SymbolCreature)), XmlInclude(typeof(SymbolPolygon)), XmlInclude(typeof(SymbolPlayer)), XmlInclude(typeof(SymbolImage)),
     XmlInclude(typeof(SymbolCone)),
     XmlInclude(typeof(SymbolCircle)), XmlInclude(typeof(SymbolText)), XmlInclude(typeof(SymbolRectangle))]
    public abstract class Symbol {
        public Point StartPoint { get; set; }
        public string Uid { get; set; }
        public Color FillColor { get; set; }
        public int Z_Order { get; set; }
        public double SizeMeter { get; set; }

        public abstract void CreateElements(Canvas canvas, MapDrawingSettings drawingSettings);

        public virtual bool OpenEditor(Point mouseDownPoint, SymbolsViewModel symbolsVM) {
            return true;
        }

        public abstract Symbol Copy();

        public void CopyBase(Symbol symbolSource) {
            Uid = SymbolsViewModel.GetTimestamp();
            StartPoint = symbolSource.StartPoint + new Vector(50, 50);
            FillColor = symbolSource.FillColor;
            Z_Order = symbolSource.Z_Order - 1;
            SizeMeter = symbolSource.SizeMeter;
        }


        public void CreateText(string caption, Canvas canvas, MapDrawingSettings drawingSettings) {
            if (string.IsNullOrWhiteSpace(caption)) {
                return;
            }
            var fontSize = 20 / drawingSettings.ZoomScale;
            var fontColor = Colors.Black;

            var textBlock = new TextBlock {
                Uid = Uid + "_1",
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
    }
}

