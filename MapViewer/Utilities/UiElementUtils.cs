using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace MapViewer.Utilities {
    internal static class UiElementUtils {
        public static BitmapCacheBrush CreateTextBrush(string text, double size, Brush brush) {
            var font = new FontFamily("Arial");
            var textBlock = new TextBlock { FontFamily = font, Text = text, Foreground = Brushes.Black, FontSize = 0.9 * size, Background = brush };
            //The next line create a special brush that contains a bitmap rendering of the UI element
            return new BitmapCacheBrush(textBlock);
        }

        public static void SetColor(this UIElement elem, Brush brush) {
            switch (elem) {
                case Ellipse ellipse when ellipse.Uid.StartsWith("Player") && ellipse.Uid.Length > 7:
                    ellipse.Fill = CreateTextBrush(ellipse.Uid.Substring(7), ellipse.Width, brush);
                    break;
                case Ellipse ellipse:
                    ellipse.Fill = brush;
                    break;
                case Rectangle rectangle:
                    rectangle.Fill = brush;
                    break;
                case Polygon polygon:
                    polygon.Fill = brush;
                    break;
            }
        }
    }
}
