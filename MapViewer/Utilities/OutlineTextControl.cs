using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace MapViewer.Utilities {
    /// <summary>
    /// OutlineText custom control class derives layout, event, data binding, and rendering from derived FrameworkElement class.
    /// </summary>
    public class OutlineTextControl : FrameworkElement {
        #region Private Fields

        private Geometry _textGeometry;

        #endregion


        #region FrameworkElement Overrides
        /// <summary>
        /// OnRender override draws the geometry of the text.
        /// </summary>
        /// <param name="drawingContext">Drawing context of the OutlineText control.</param>
        protected override void OnRender(DrawingContext drawingContext) {
            // Draw the outline based on the properties that are set.
            drawingContext.DrawGeometry(Fill, new Pen(Stroke, StrokeThickness), _textGeometry);
        }

        /// <summary>
        /// Create the outline geometry based on the formatted text.
        /// </summary>
        public Rect CreateText() {
            var formattedText = new FormattedText(
               Caption,
               CultureInfo.GetCultureInfo("sv-SE"),
               FlowDirection.LeftToRight,
               new Typeface(
                   Font,
                   FontStyle,
                   FontWeight,
                   FontStretches.Normal),
               FontSize,
               Brushes.Black, // This brush does not matter since we use the geometry of the text.
               VisualTreeHelper.GetDpi(this).PixelsPerDip);

            // Build the geometry object that represents the text.
            _textGeometry = formattedText.BuildGeometry(new Point(0, 0));

            IsHitTestVisible = false;
            return _textGeometry.Bounds;
        }
        #endregion

        #region DependencyProperties

        public string Caption { get; set; }

        public FontFamily Font { get; set; } = new FontFamily("Arial");

        public FontWeight FontWeight { get; set; } = FontWeights.Heavy;

        public FontStyle FontStyle { get; set; } = FontStyles.Normal;

        public double FontSize { get; set; } = 20.0;

        public Brush Fill { get; set; } = new SolidColorBrush(Colors.Yellow);

        public Brush Stroke { get; set; } = new SolidColorBrush(Colors.Black);

        public double StrokeThickness { get; set; } = 1.0;
        #endregion
    }
}