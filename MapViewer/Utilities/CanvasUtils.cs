using System.Linq;
using System.Windows;
using System.Windows.Controls;

// ReSharper disable once CheckNamespace
namespace MapViewer {
    public static class CanvasUtils {
      
        public static void RemoveMySymbolsFromOverlay(this Canvas canvas, string prefix) {
            var symbols = canvas.Children.Cast<UIElement>().Where(elem => elem.Uid.StartsWith(prefix)).ToList();
            foreach (var elem in symbols) {
                 canvas.Children.Remove(elem);
            }
        }

        public static UIElement FindElementHit(this Canvas canvas) {
            return canvas.Children.Cast<UIElement>().FirstOrDefault(child => child.IsMouseOver);
        }

        public static UIElement FindElementByUid(this Canvas canvas, string uid) {
            return canvas.Children.Cast<UIElement>().FirstOrDefault(child => child.Uid == uid);
        }
        
        public static Size GetTextSize(this Canvas canvas, TextBlock textBlock) {
            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            return textBlock.DesiredSize;
        }

    }
}