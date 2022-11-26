using System;
using System.Collections.Generic;
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

        public string ImageFileName { get; set; }
        public double RotationDegree { get; set; }
        
        public override void CreateElements(Canvas canvas, MapDrawingSettings drawingSettings) {
            BitmapSource iconSource;
            if (string.IsNullOrWhiteSpace(ImageFileName) || !File.Exists(ImageFileName)) {
                iconSource = CreateBitmapSource(Colors.Orange);
            }
            else {
                iconSource = new BitmapImage(new Uri(ImageFileName));
            }

            var scale = (SizeMeter / drawingSettings.ImageScaleMperPix) / Math.Max(iconSource.Width, iconSource.Height);

            var trfScale = new ScaleTransform(scale, scale);
            var trfRot = new RotateTransform(RotationDegree, scale * iconSource.Width * 0.5, scale * iconSource.Height * 0.5);

            var finalTransform = new TransformGroup();
            finalTransform.Children.Add(trfScale);
            finalTransform.Children.Add(trfRot);

            var shape = new Image {
                Uid= Uid,
                RenderTransform = finalTransform,
                Opacity = 1.0,
                Source = iconSource,
            };

            Canvas.SetLeft(shape, StartPoint.X - iconSource.PixelWidth * scale / 2 ); 
            Canvas.SetTop(shape, StartPoint.Y - iconSource.PixelHeight * scale / 2);
            canvas.Children.Add(shape);

            CreateTextElement(Caption, canvas, drawingSettings);
        }

        public override bool OpenEditor(Point dialogPos, SymbolsViewModel symbolsVM) {
            var dlg = new DialogImageProp {
                Symbol = this,
                SymbolsVM = symbolsVM,
                DialogPos = dialogPos
            };

            var result = dlg.ShowDialog();
            return result != null && result.Value;
        }

        public override Symbol Copy() {
            var newSymbol = new SymbolImage();
            newSymbol.CopyBase(this);
            newSymbol.Caption = newSymbol.Caption = SymbolsViewModel.CountUpCaption(Caption);
            newSymbol.RotationDegree = RotationDegree;
            newSymbol.ImageFileName = ImageFileName;

            return newSymbol;
        }


        private BitmapSource CreateBitmapSource(Color color) {
            var width = 128;
            var height = width;
            var stride = width / 8;
            var pixels = new byte[height * stride];

            var colors = new List<Color> { color };
            var myPalette = new BitmapPalette(colors);

            var image = BitmapSource.Create(
                width,
                height,
                96,
                96,
                PixelFormats.Indexed1,
                myPalette,
                pixels,
                stride);

            return image;
        }

    }
}