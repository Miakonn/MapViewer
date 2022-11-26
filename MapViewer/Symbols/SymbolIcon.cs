using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using MapViewer.Dialogs;
using MapViewer.Maps;

namespace MapViewer.Symbols {
    [Serializable]
    [XmlInclude(typeof(Symbol))]
    public class SymbolIcon : Symbol {

        public string ImageFileName { get; set; }
        public double RotationDegree { get; set; }
        
        public override void DrawElements(Canvas canvas, MapDrawingSettings drawingSettings) {
            BitmapSource iconSource;
            if (string.IsNullOrWhiteSpace(ImageFileName) || !File.Exists(ImageFileName)) {
                iconSource = new BitmapImage(new Uri("pack://application:,,,/Images/Question_mark.png"));
            }
            else {
                iconSource = new BitmapImage(new Uri(ImageFileName));
            }

            double iconSize = Math.Max(iconSource.Width, iconSource.Height);

            var scale = (SizeMeter / drawingSettings.ImageScaleMperPix) / iconSize;

            double minSizeScaled = drawingSettings.MinCreatureSizePixel / drawingSettings.ZoomScale;
            if (scale * iconSize < minSizeScaled) {
                scale = minSizeScaled / iconSize;
            }

            var center = new Vector(scale * iconSource.Width * 0.5, scale * iconSource.Height * 0.5);

            var trfScale = new ScaleTransform(scale, scale);
            var trfRot = new RotateTransform(RotationDegree, center.X, center.Y);

            var finalTransform = new TransformGroup();
            finalTransform.Children.Add(trfScale);
            finalTransform.Children.Add(trfRot);

            var shape = new Image {
                Uid= Uid,
                RenderTransform = finalTransform,
                Opacity = 1.0,
                Source = iconSource,
            };

            Canvas.SetLeft(shape, StartPoint.X -center.X); 
            Canvas.SetTop(shape, StartPoint.Y - center.Y);
            canvas.Children.Add(shape);

            DrawTextElement(Caption, canvas, drawingSettings);
        }

        public override bool OpenDialogProp(Point dialogPos, SymbolsViewModel symbolsVM) {
            var dlg = new DialogIconProp {
                Symbol = this,
                SymbolsVM = symbolsVM,
                DialogPos = dialogPos
            };

            var result = dlg.ShowDialog();
            return result != null && result.Value;
        }

        public override Symbol Copy() {
            var newSymbol = new SymbolIcon();
            newSymbol.CopyBase(this);
            newSymbol.RotationDegree = RotationDegree;
            newSymbol.ImageFileName = ImageFileName;

            return newSymbol;
        }
    }
}