﻿using MapViewer.Maps;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace MapViewer.Symbols {


    [Serializable]
    [XmlInclude(typeof(SymbolCreature)), XmlInclude(typeof(SymbolPolygon)), XmlInclude(typeof(SymbolPlayer)),
     XmlInclude(typeof(SymbolCircle)), XmlInclude(typeof(SymbolText)), XmlInclude(typeof(SymbolLine))]
    public abstract class Symbol {
        public Point StartPoint { get; set; }
        public string Uid { get; set; }
        public Color FillColor { get; set; }
        public int Z_Order { get; set; }
        public double SizeMeter { get; set; }

        public abstract void CreateElements(Canvas canvas, MapDrawingSettings drawingSettings);
    }
    

    public class SymbolCreature : Symbol
    {
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

            var textBlock = new TextBlock {
                Uid = Uid + "_1",
                Text = Caption,
                FontSize = fontSize,
                Foreground = new SolidColorBrush(Colors.Black),
                FontWeight = FontWeights.Normal,
                IsHitTestVisible = false
            };

            var textSize = canvas.GetTextSize(textBlock);
            Canvas.SetLeft(textBlock, StartPoint.X - textSize.Width / 2.0);
            Canvas.SetTop(textBlock, StartPoint.Y - textSize.Height / 2.0);
            canvas.Children.Add(textBlock);
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
    }
    

}

