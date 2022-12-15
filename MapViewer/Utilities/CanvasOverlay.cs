using System.Linq;
using System.Windows;
using System.Windows.Controls;

// ReSharper disable once CheckNamespace
namespace MapViewer {

    public class CanvasOverlay : Canvas {
        public void RemoveMySymbolsFromOverlay(string prefix) {
            var symbols = Children.Cast<UIElement>().Where(elem => elem.Uid.StartsWith(prefix)).ToList();
            foreach (var elem in symbols) {
                Children.Remove(elem);
            }
        }

        public UIElement FindElementHit() {
            return Children.Cast<UIElement>().FirstOrDefault(child => child.IsMouseOver);
        }

        public UIElement FindElementByUid(string uid) {
            return Children.Cast<UIElement>().FirstOrDefault(child => child.Uid == uid);
        }

        public Size GetTextSize( TextBlock textBlock) {
            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            return textBlock.DesiredSize;
        }

        public void RemoveElement(UIElement elem) {
            if (elem == null) {
                return;
            }
            Children.Remove(elem);
        }

        public void RemoveElement(string uid) {
            RemoveElement(FindElementByUid(uid));
        }

        public void Clear() {
            Children.Clear();
        }
    }
}