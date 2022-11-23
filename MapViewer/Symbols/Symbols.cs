using MapViewer.Maps;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Serialization;
using MapViewer.Dialogs;

namespace MapViewer.Symbols {


    [Serializable]
    [XmlInclude(typeof(SymbolCreature)), XmlInclude(typeof(SymbolPolygon)), XmlInclude(typeof(SymbolPlayer)), XmlInclude(typeof(SymbolImage)),
     XmlInclude(typeof(SymbolCircle)), XmlInclude(typeof(SymbolText)), XmlInclude(typeof(SymbolLine))]
    public abstract class Symbol {
        public Point StartPoint { get; set; }
        public string Uid { get; set; }
        public Color FillColor { get; set; }
        public int Z_Order { get; set; }
        public double SizeMeter { get; set; }

        public abstract void CreateElements(Canvas canvas, MapDrawingSettings drawingSettings);

        public virtual bool OpenEditor(Point mouseDownPoint, SymbolsViewModel symbolsVM) {
            return true;
        }

        public abstract Symbol Copy();

        public void CopyBase(Symbol symbolSource) {
            Uid = SymbolsViewModel.GetTimestamp();
            StartPoint = symbolSource.StartPoint;
            FillColor = symbolSource.FillColor;
            Z_Order = symbolSource.Z_Order + 1;
            SizeMeter = symbolSource.SizeMeter;
        }
    }


    public class SymbolCreature : Symbol {
        public string Caption { get; set; }

        public override void CreateElements(Canvas canvas, MapDrawingSettings drawingSettings) {
            var brush = new SolidColorBrush(FillColor);

            double sizePixel = SizeMeter / drawingSettings.ImageScaleMperPix;
            double minSizeScaled = drawingSettings.MinCreatureSizePixel / drawingSettings.ZoomScale;
            sizePixel = Math.Max(sizePixel, minSizeScaled);

            var shape = new Ellipse {
                Uid = Uid,
                Width = sizePixel,
                Height = sizePixel,
                Fill = brush,
                Opacity = 1.0
            };

            Canvas.SetLeft(shape, StartPoint.X - shape.Width / 2);
            Canvas.SetTop(shape, StartPoint.Y - shape.Height / 2);

            canvas.Children.Add(shape);

            if (string.IsNullOrWhiteSpace(Caption)) {
                return;
            }

            var fontSize = 20 / drawingSettings.ZoomScale;
            var fontColor = Colors.Black;

            if (FillColor.Equals(Colors.Black) || FillColor.Equals(Colors.Purple) || FillColor.Equals(Colors.Blue)) {
                fontColor = Colors.Orange;
            }

            var textBlock = new TextBlock {
                Uid = Uid + "_1",
                Text = Caption,
                FontSize = fontSize,
                Foreground = new SolidColorBrush(fontColor),
                FontWeight = FontWeights.Normal,
                IsHitTestVisible = false
            };

            var textSize = canvas.GetTextSize(textBlock);
            Canvas.SetLeft(textBlock, StartPoint.X - textSize.Width / 2.0);
            Canvas.SetTop(textBlock, StartPoint.Y - textSize.Height / 2.0);
            canvas.Children.Add(textBlock);
        }


        public override bool OpenEditor(Point mouseDownPoint, SymbolsViewModel symbolsVM) {
            var dlg = new DialogCreatureProp {
                Caption = Caption,
                SizeMeter = SizeMeter,
                StartPosition = mouseDownPoint
            };

            var result = dlg.ShowDialog();
            if (result == null || !result.Value) {
                return false;
            }

            Caption = dlg.Caption;
            SizeMeter = dlg.SizeMeter;
            return true;
        }

        public override Symbol Copy() {
            var newSymbol = new SymbolCreature();
            newSymbol.CopyBase(this);
            return newSymbol;
        }
    }

    public class SymbolPlayer : SymbolCreature { }  

    [Serializable]
    [XmlInclude(typeof(Symbol))]
    public class SymbolCircle : Symbol {

        public override void CreateElements(Canvas canvas, MapDrawingSettings drawingSettings)
        {
            var brush = new SolidColorBrush(FillColor);

            var shape = new Ellipse {
                Uid = Uid,
                Width = SizeMeter / drawingSettings.ImageScaleMperPix,
                Height = SizeMeter / drawingSettings.ImageScaleMperPix,
                Fill = brush,
                Opacity = 1.0
            };

            Canvas.SetLeft(shape, StartPoint.X - shape.Width / 2);
            Canvas.SetTop(shape, StartPoint.Y - shape.Height / 2);

            canvas.Children.Add(shape);
        }

        public override Symbol Copy() {
            throw new NotImplementedException();
        }
    }
    
    [Serializable]
    [XmlInclude(typeof(Symbol))]
    public class SymbolPolygon : Symbol {
        public PointCollection Corners { get; set; }
        
        public override void CreateElements(Canvas canvas, MapDrawingSettings drawingSettings) {
            var shape = new Polygon {
                Uid = Uid,
                Points = Corners,
                Fill = new SolidColorBrush(FillColor),
                Opacity = 0.4
            };

            Canvas.SetLeft(shape, StartPoint.X);
            Canvas.SetTop(shape, StartPoint.Y);
            canvas.Children.Add(shape);
        }

        public override Symbol Copy() {
            throw new NotImplementedException();
        }
    }
    
    [Serializable]
    [XmlInclude(typeof(Symbol))]
    public class SymbolText : Symbol {
        public string Caption { get; set; }
        public double RotationAngle { get; set; }
        public double LengthMeter { get; set; }

        public override void CreateElements(Canvas canvas, MapDrawingSettings drawingSettings)
        {
            var fontSize = 25 / drawingSettings.ZoomScale;
            var textBlock = new TextBlock {
                Uid = Uid,
                //RenderTransform = new RotateTransform(RotationAngle),
                Text = Caption,
                FontSize = fontSize,
                Foreground = new SolidColorBrush(FillColor),
                FontWeight = FontWeights.UltraBold,
            };

            var textSize = canvas.GetTextSize(textBlock);
            double scaleLength = LengthMeter / drawingSettings.ImageScaleMperPix / textSize.Width;

            // Reset size and angle
            textBlock.RenderTransform = new RotateTransform(RotationAngle);
            textBlock.FontSize = fontSize * scaleLength;

            textSize = canvas.GetTextSize(textBlock);

            Canvas.SetLeft(textBlock, StartPoint.X - textSize.Width / 2);
            Canvas.SetTop(textBlock, StartPoint.Y - textSize.Height / 2);
            canvas.Children.Add(textBlock);
        }

        public override Symbol Copy() {
            throw new NotImplementedException();
        }
    }
    

    [Serializable]
    [XmlInclude(typeof(Symbol))]
    public class SymbolLine : Symbol {
        public double Width { get; set; }
        public Point EndPoint { get; set; }

        public override void CreateElements(Canvas canvas, MapDrawingSettings drawingSettings)
        {
            var shape = new Line {
                Uid = Uid,
                X1 = 0,
                Y1 = 0,
                X2 = EndPoint.X,
                Y2 = EndPoint.Y,
                StrokeThickness = Width / drawingSettings.ImageScaleMperPix,
                Stroke = new SolidColorBrush(FillColor),
                Opacity = 0.4

            };
            Canvas.SetLeft(shape, StartPoint.X);
            Canvas.SetTop(shape, StartPoint.Y);
            canvas.Children.Add(shape);
        }

        public override Symbol Copy() {
            throw new NotImplementedException();
        }
    }


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


            if (string.IsNullOrWhiteSpace(Caption)) {
                return;
            }
            var fontSize = 20 / drawingSettings.ZoomScale;
            var fontColor = Colors.Black;

            var textBlock = new TextBlock {
                Uid = Uid + "_1",
                Text = Caption,
                FontSize = fontSize,
                Foreground = new SolidColorBrush(fontColor),
                FontWeight = FontWeights.Normal,
                IsHitTestVisible = false
            };

            var textSize = canvas.GetTextSize(textBlock);
            Canvas.SetLeft(textBlock, StartPoint.X - textSize.Width / 2.0);
            Canvas.SetTop(textBlock, StartPoint.Y - textSize.Height / 2.0);
            canvas.Children.Add(textBlock);
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
            newSymbol.Caption = Caption + "1";
            newSymbol.RotationAngle = RotationAngle;
            newSymbol.ImageFileName = ImageFileName;

            return newSymbol;
        }
    }
}

