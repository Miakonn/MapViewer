using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace MapViewer.Utilities {
    internal static class UiElementUtils {
        public static void SetColor(this UIElement elem, Brush brush) {
            switch (elem) {
                case Ellipse ellipse:
                    ellipse.Fill = brush;
                    break;
                case Rectangle rectangle:
                    rectangle.Fill = brush;
                    break;
                case Polygon polygon:
                    polygon.Fill = brush;
                    break;
                case Line line:
                    line.Stroke = brush;
                    break;
                case TextBlock text:
                    text.Foreground = brush;
                    break;
            }
        }
    }
}
