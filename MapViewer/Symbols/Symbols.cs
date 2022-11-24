using MapViewer.Maps;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Serialization;

namespace MapViewer.Symbols {


    [Serializable]
    [XmlInclude(typeof(SymbolCreature)), XmlInclude(typeof(SymbolPolygon)), XmlInclude(typeof(SymbolPlayer)), XmlInclude(typeof(SymbolImage)),
     XmlInclude(typeof(SymbolCone)),
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
}

