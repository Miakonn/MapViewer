﻿using MapViewer.Maps;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Serialization;
using MapViewer.Dialogs;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace MapViewer.Symbols {
    [Serializable]
    [XmlInclude(typeof(Symbol))]
    public class SymbolCone : Symbol {
        public double WidthDegrees { get; set; }

        public double RotationAngle { get; set; }

        public override void CreateElements(Canvas canvas, MapDrawingSettings drawingSettings) {

            double sizePixel = SizeMeter / drawingSettings.ImageScaleMperPix;

            var corners = new PointCollection { new Point() };
            for (var a = -WidthDegrees / 2; a < WidthDegrees / 2; a+= 5.0) {
                var aRadian = (a + RotationAngle) * (Math.PI / 180.0);
                var pnt = new Point( sizePixel * Math.Cos(aRadian),  sizePixel * Math.Sin(aRadian) );
                corners.Add(pnt);
            }

            var aRadianLast = (WidthDegrees / 2.0 + RotationAngle) * (Math.PI / 180) ;
            var pntLast = new Point(sizePixel * Math.Cos(aRadianLast), sizePixel * Math.Sin(aRadianLast));
            corners.Add(pntLast);

            var shape = new Polygon {
                Uid = Uid,
                Points = corners,
                Fill = new SolidColorBrush(FillColor),
                Opacity = 0.4
            };
            Canvas.SetLeft(shape, StartPoint.X);
            Canvas.SetTop(shape, StartPoint.Y);
            canvas.Children.Add(shape);
        }

        public override bool OpenEditor(Point mouseDownPoint, SymbolsViewModel symbolsVM) {
            var dlg = new DialogConeProp {
                Symbol = this,
                SymbolsVM = symbolsVM,
                StartPosition = mouseDownPoint
            };

            var result = dlg.ShowDialog();
            return result != null && result.Value;
        }

        public override Symbol Copy() {
            var newSymbol = new SymbolCone();
            newSymbol.CopyBase(this);
            newSymbol.WidthDegrees = WidthDegrees;
            newSymbol.RotationAngle = RotationAngle;
            return newSymbol;
        }
    }
}