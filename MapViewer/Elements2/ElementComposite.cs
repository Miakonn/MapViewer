using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MapViewer.Elements2 {
    public class ElementComposite : UIElement {
        private readonly UIElementCollection children;

        public ElementComposite() {
            children = new UIElementCollection(this, null);
        }

        public void AddChild(UIElement element) {
            children.Add(element);
        }

        public void RemoveChild(UIElement element) {
            children.Remove(element);
        }

        protected override int VisualChildrenCount => children.Count;

        protected override Visual GetVisualChild(int index) {
            return children[index];
        }

        protected override Size MeasureCore(Size availableSize) {
            foreach (UIElement element in children) {
                element.Measure(availableSize);
            }

            return new Size();
        }

        protected override void ArrangeCore(Rect finalRect) {
            foreach (UIElement element in children) {
                element.Arrange(finalRect);
            }
        }

    }

}
