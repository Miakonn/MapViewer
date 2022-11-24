﻿using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Serialization;
using MapViewer.Maps;

namespace MapViewer.Symbols {
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
}