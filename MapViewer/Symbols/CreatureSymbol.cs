using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MapViewer {

    public interface ISymbol {
        Point Position { get; set; }
        string Uid { get; set; }
        string Caption { get; set; }
        Color Color { get; set; }
        int Layer { get; set; }
        double SizeMeter { get; set; }

        void CreateElements(Canvas canvas, double Scale, double ImageScaleMperPix);
        void Move(Vector move);
    }

    public abstract class Symbol : ISymbol {
        public Point Position { get; set; }
        public string Uid { get; set; }
        public string Caption { get; set; }
        public Color Color { get; set; }
        public int Layer { get; set; }
        public double SizeMeter { get; set; }

        public abstract void CreateElements(Canvas canvas, double Scale, double ImageScaleMperPix);

        public void Move(Vector move) {
            Position -= move;
        }
    }



    public class CreatureSymbol : Symbol {


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

            Debug.WriteLine($"CreateElements {shape.Uid}  {shape.Width} ImageScaleMperPix={ImageScaleMperPix}  Scale={Scale} Size={SizeMeter}");
            shape.Uid = Uid;
            canvas.Children.Add(shape);

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
}
