using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using MapViewer.Maps;

namespace MapViewer.Symbols {
    internal class Ruler {

        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        private readonly Canvas _canvas;

        private static double CalcStep(double length, out int count) {
            Log.Debug("CalcStep length=" + length);

            var factor = Math.Pow(10, Math.Floor(Math.Log10(length)) - 1);
            length /= factor;

            if (length <= 10) {
                count = (int)length;
                return factor;
            }
            if (length <= 20) {
                count = 10;
                return factor;
            }
            if (length <= 50) {
                count = 20;
                return factor;
            }
            count = 10;
            var result = 5 * factor;
            return result;
        }

        private const double RulerMarginX = 20;

        private void WriteRulerText(string unit, double length, double yPos, bool topJustify) {

            var text = new TextBlock {
                RenderTransform = new RotateTransform(-90),
                Text = $"{length} {unit}",
                FontSize = 25,
                Foreground = Brushes.Red,
                FontWeight = FontWeights.UltraBold
            };

            if (topJustify) {
                yPos += 10 * text.Text.Length;
            }

            Canvas.SetLeft(text, RulerMarginX - 20);
            Canvas.SetTop(text, yPos);
            _canvas.Children.Add(text);
        }

        private void DrawRuler(int count, double step, double y0) {
            for (var i = 0; i < count; i++) {
                var shape = new Line {
                    X1 = RulerMarginX,
                    Y1 = y0 + step * i,
                    X2 = RulerMarginX,
                    Y2 = y0 + step * i + step,
                    StrokeThickness = 14,
                    Stroke = new SolidColorBrush((i % 2 == 0) ? Colors.WhiteSmoke : Colors.Red),
                    Opacity = 1.0,
                    Uid = "Ruler"

                };
                _canvas.Children.Add(shape);
            }
        }

        public void Draw(double windowHeight,  MapDrawSettings settings, string unit) {
            if (settings.ImageScaleMperPix == 0 || settings.ZoomScale == 0) {
                return;
            }
            
            var screenScaleMperPix = settings.ImageScaleMperPix / settings.ZoomScale;

            _canvas.Children.Clear();

            if (screenScaleMperPix < 0.0001 || screenScaleMperPix > 1E6) {
                Log.Debug($"SetRuler ImageScaleMperPix={settings.ImageScaleMperPix} Scale={settings.ZoomScale}");
                Log.Error("Screen scale out of bounds!");
                return;
            }
            var y0 = windowHeight / 10;
            var height = windowHeight - 2 * y0;
            var lengthM = height * screenScaleMperPix;
            var stepM = CalcStep(lengthM, out var count);
            var stepPix = stepM / screenScaleMperPix;

            DrawRuler(count, stepPix, y0);
            WriteRulerText(unit, count * stepM, y0 - 5, false);
            WriteRulerText(unit, stepM, y0 + count * stepPix + 25, true);
        }

        public Ruler(Canvas canvas) {
            _canvas = canvas;
        }


    }
}
