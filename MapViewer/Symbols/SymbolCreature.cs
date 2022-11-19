using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace MapViewer.Symbols {


    [Serializable]
    [XmlInclude(typeof(SymbolCreature))]
    public abstract class Symbol {
        public Point Position { get; set; }
        public string Uid { get; set; }
        public string Caption { get; set; }
        public Color Color { get; set; }
        public int Zorder { get; set; }
        public double SizeMeter { get; set; }

        public abstract void CreateElements(Canvas canvas, double Scale, double ImageScaleMperPix);

        public void Move(Vector move) {
            Position -= move;
        }
    }


    [Serializable]
    [XmlInclude(typeof(Symbol))]
    public class SymbolCreature : Symbol {

        public override void CreateElements(Canvas canvas, double Scale, double ImageScaleMperPix)
        {
            var brush = new SolidColorBrush(Color);
 
            var shape = new Ellipse {
                Uid = Uid,
                Width = SizeMeter / ImageScaleMperPix,
                Height = SizeMeter / ImageScaleMperPix,
                Fill = brush,
                Opacity = 1.0
            };

            Canvas.SetLeft(shape, Position.X - shape.Width / 2);
            Canvas.SetTop(shape, Position.Y - shape.Height / 2);

            canvas.Children.Add(shape);

            if (string.IsNullOrWhiteSpace(Caption)) {
                return;
            }

            var fontSize = 20 / Scale;

            var textBlock = new TextBlock {
                Uid = Uid + "_1",
                Text = Caption,
                FontSize = fontSize,
                Foreground = new SolidColorBrush(Colors.Black),
                FontWeight = FontWeights.Normal,
                IsHitTestVisible = false
            };

            var textSize = canvas.GetTextSize(textBlock);
            Canvas.SetLeft(textBlock, Position.X - textSize.Width / 2.0);
            Canvas.SetTop(textBlock, Position.Y - textSize.Height / 2.0);
            canvas.Children.Add(textBlock);
        }
    }

    [Serializable]
    [XmlInclude(typeof(Symbol))]
    public class SymbolCircle : Symbol {

        public override void CreateElements(Canvas canvas, double Scale, double ImageScaleMperPix)
        {
            var brush = new SolidColorBrush(Color);

            var shape = new Ellipse {
                Uid = Uid,
                Width = SizeMeter / ImageScaleMperPix,
                Height = SizeMeter / ImageScaleMperPix,
                Fill = brush,
                Opacity = 1.0
            };

            Canvas.SetLeft(shape, Position.X - shape.Width / 2);
            Canvas.SetTop(shape, Position.Y - shape.Height / 2);

            canvas.Children.Add(shape);

            if (string.IsNullOrWhiteSpace(Caption)) {
                return;
            }
        }
    }
}
