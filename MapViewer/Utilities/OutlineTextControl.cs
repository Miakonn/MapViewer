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

        #region Private Methods

        /// <summary>
        /// Invoked when a dependency property has changed. Generate a new FormattedText object to display.
        /// </summary>
        /// <param name="d">OutlineText object whose property was updated.</param>
        /// <param name="e">Event arguments for the dependency property.</param>
        private static void OnOutlineTextInvalidated(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((OutlineTextControl)d).CreateText();
        }

        #endregion

        #region FrameworkElement Overrides
        /// <summary>
        /// OnRender override draws the geometry of the text and optional highlight.
        /// </summary>
        /// <param name="drawingContext">Drawing context of the OutlineText control.</param>
        protected override void OnRender(DrawingContext drawingContext) {
            // Draw the outline based on the properties that are set.
            drawingContext.DrawGeometry(Fill, new Pen(Stroke, StrokeThickness), _textGeometry);
        }


         /// <summary>
        /// Create the outline geometry based on the formatted text.
        /// </summary>
        public void CreateText() {

            // Create the formatted text based on the properties set.
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
         }
        #endregion


        public Rect TextBounds => _textGeometry?.Bounds ?? new Rect();


        #region DependencyProperties

        /// <summary>
        /// Specifies whether the font should display Bold font weight.
        /// </summary>
        public FontWeight FontWeight {
            get => (FontWeight)GetValue(FontWeightProperty);

            set => SetValue(FontWeightProperty, value);
        }

        /// <summary>
        /// Identifies the FontWeight dependency property.
        /// </summary>
        public static readonly DependencyProperty FontWeightProperty = DependencyProperty.Register(
            nameof(FontWeight),
            typeof(FontWeight),
            typeof(OutlineTextControl),
            new FrameworkPropertyMetadata(
                FontWeights.Normal,
                FrameworkPropertyMetadataOptions.AffectsRender,
                OnOutlineTextInvalidated,
                null
                )
            );

        /// <summary>
        /// Specifies the brush to use for the fill of the formatted text.
        /// </summary>
        public Brush Fill {
            get => (Brush)GetValue(FillProperty);

            set => SetValue(FillProperty, value);
        }

        /// <summary>
        /// Identifies the Fill dependency property.
        /// </summary>
        public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
            nameof(Fill),
            typeof(Brush),
            typeof(OutlineTextControl),
            new FrameworkPropertyMetadata(
                new SolidColorBrush(Colors.White),
                FrameworkPropertyMetadataOptions.AffectsRender,
                OnOutlineTextInvalidated,
                null
                )
            );

        /// <summary>
        /// The font to use for the displayed formatted text.
        /// </summary>
        public FontFamily Font {
            get => (FontFamily)GetValue(FontProperty);

            set => SetValue(FontProperty, value);
        }

        /// <summary>
        /// Identifies the Font dependency property.
        /// </summary>
        public static readonly DependencyProperty FontProperty = DependencyProperty.Register(
            nameof(Font),
            typeof(FontFamily),
            typeof(OutlineTextControl),
            new FrameworkPropertyMetadata(
                new FontFamily("Arial"),
                FrameworkPropertyMetadataOptions.AffectsRender,
                OnOutlineTextInvalidated,
                null
                )
            );

        /// <summary>
        /// The current font size.
        /// </summary>
        public double FontSize {
            get => (double)GetValue(FontSizeProperty);

            set => SetValue(FontSizeProperty, value);
        }

        /// <summary>
        /// Identifies the FontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(
            nameof(FontSize),
            typeof(double),
            typeof(OutlineTextControl),
            new FrameworkPropertyMetadata(
                 24.0,
                 FrameworkPropertyMetadataOptions.AffectsRender,
                 OnOutlineTextInvalidated,
                 null
                 )
            );

        /// <summary>
        /// Specifies whether the font should display Italic font style.
        /// </summary>
        public FontStyle FontStyle {
            get => (FontStyle)GetValue(FontStyleProperty);

            set => SetValue(FontStyleProperty, value);
        }

        /// <summary>
        /// Identifies the Italic dependency property.
        /// </summary>
        public static readonly DependencyProperty FontStyleProperty = DependencyProperty.Register(
            nameof(FontStyle),
            typeof(FontStyle),
            typeof(OutlineTextControl),
            new FrameworkPropertyMetadata(
                FontStyles.Normal,
                 FrameworkPropertyMetadataOptions.AffectsRender,
                 OnOutlineTextInvalidated,
                 null
                 )
            );

        /// <summary>
        /// Specifies the brush to use for the stroke and optional highlight of the formatted text.
        /// </summary>
        public Brush Stroke {
            get => (Brush)GetValue(StrokeProperty);

            set => SetValue(StrokeProperty, value);
        }

        /// <summary>
        /// Identifies the Stroke dependency property.
        /// </summary>
        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
            nameof(Stroke),
            typeof(Brush),
            typeof(OutlineTextControl),
            new FrameworkPropertyMetadata(
                 new SolidColorBrush(Colors.Teal),
                 FrameworkPropertyMetadataOptions.AffectsRender,
                 OnOutlineTextInvalidated,
                 null
                 )
            );

        /// <summary>
        ///     The stroke thickness of the font.
        /// </summary>
        public double StrokeThickness {
            get => (double)GetValue(StrokeThicknessProperty);

            set => SetValue(StrokeThicknessProperty, value);
        }

        /// <summary>
        /// Identifies the StrokeThickness dependency property.
        /// </summary>
        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
            nameof(StrokeThickness),
            typeof(double),
            typeof(OutlineTextControl),
            new FrameworkPropertyMetadata(
                 (double)0,
                 FrameworkPropertyMetadataOptions.AffectsRender,
                 OnOutlineTextInvalidated,
                 null
                 )
            );

        /// <summary>
        /// Specifies the text string to display.
        /// </summary>
        public string Caption {
            get => (string)GetValue(CaptionProperty);

            set => SetValue(CaptionProperty, value);
        }

        /// <summary>
        /// Identifies the Text dependency property.
        /// </summary>
        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register(
            nameof(Caption),
            typeof(string),
            typeof(OutlineTextControl),
            new FrameworkPropertyMetadata(
                 "",
                 FrameworkPropertyMetadataOptions.AffectsRender,
                 OnOutlineTextInvalidated,
                 null
                 )
            );

        #endregion
    }
}