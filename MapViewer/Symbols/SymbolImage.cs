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
    public class SymbolImage : Symbol {

        public string Caption { get; set; }
        public string ImageFileName { get; set; }
        public double RotationAngle { get; set; }


        public override void CreateElements(Canvas canvas, MapDrawingSettings drawingSettings) {

            if (string.IsNullOrWhiteSpace(ImageFileName) || !File.Exists(ImageFileName)) {
                return;
            }

            var image = new BitmapImage(new Uri(ImageFileName));

            double scale = (SizeMeter / drawingSettings.ImageScaleMperPix) / Math.Max(image.Width, image.Height);

            var trfScale = new ScaleTransform(scale, scale);
            var trfRot = new RotateTransform(RotationAngle, scale * image.Width * 0.5, scale * image.Height * 0.5);

            var finalTransform = new TransformGroup();
            finalTransform.Children.Add(trfScale);
            finalTransform.Children.Add(trfRot);

            var shape = new Image {
                Uid= Uid,
                RenderTransform = finalTransform,
                Opacity = 1.0,
                Source = image,
            };

            Canvas.SetLeft(shape, StartPoint.X - image.PixelWidth * scale / 2 ); 
            Canvas.SetTop(shape, StartPoint.Y - image.PixelHeight * scale / 2);
            canvas.Children.Add(shape);

            CreateText(Caption, canvas, drawingSettings);
        }

        public override bool OpenEditor(Point mouseDownPoint, SymbolsViewModel symbolsVM) {
            var dlg = new DialogImageProp {
                Symbol = this,
                SymbolsVM = symbolsVM,
                StartPosition = mouseDownPoint
            };

            var result = dlg.ShowDialog();
            return result != null && result.Value;
        }

        public override Symbol Copy() {
            var newSymbol = new SymbolImage();
            newSymbol.CopyBase(this);
            newSymbol.Caption = newSymbol.Caption = SymbolsViewModel.CountUpCaption(Caption);
            newSymbol.RotationAngle = RotationAngle;
            newSymbol.ImageFileName = ImageFileName;

            return newSymbol;
        }
    }
}