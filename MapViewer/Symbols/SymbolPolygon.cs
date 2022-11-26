﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Serialization;
using MapViewer.Dialogs;
using MapViewer.Maps;

namespace MapViewer.Symbols {
    [Serializable]
    [XmlInclude(typeof(Symbol))]
    public class SymbolPolygon : Symbol {
        public PointCollection Corners { get; set; }
        
        public override void DrawElements(Canvas canvas, MapDrawingSettings drawingSettings) {
            var shape = new Polygon {
                Uid = Uid,
                Points = Corners,
                Fill = new SolidColorBrush(FillColor),
                Opacity = 0.4
            };

            Canvas.SetLeft(shape, StartPoint.X);
            Canvas.SetTop(shape, StartPoint.Y);
            canvas.Children.Add(shape);

            DrawTextElement(Caption, canvas, drawingSettings);
        }
        public override bool OpenEditor(Point dialogPos, SymbolsViewModel symbolsVM) {
            var dlg = new DialogBaseSymbolProp() {
                Symbol = this,
                SymbolsVM = symbolsVM,
                DialogPos = dialogPos
            };

            var result = dlg.ShowDialog();
            return result != null && result.Value;
        }

        public override Symbol Copy() {
            var newSymbol = new SymbolPolygon();
            newSymbol.CopyBase(this);
            newSymbol.Corners = Corners.Clone();
            return newSymbol;
        }
    }
}