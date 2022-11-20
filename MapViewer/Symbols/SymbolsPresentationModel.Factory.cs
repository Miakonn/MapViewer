using System.Windows;
using System.Windows.Media;

namespace MapViewer.Symbols {
    public partial class SymbolsPresentationModel {
        #region Factory

        public void CreateSymbolCreature(Point pos, Color color, double sizeMeter, string caption) {
            var symbol = new SymbolCreature {
                Uid = GetTimestamp(),
                FillColor = color,
                Caption = caption,
                Z_Order = GetMinZorder() - 1,
                StartPoint = pos,
                SizeMeter = sizeMeter
            };

            AddSymbol(symbol);
        }

        public void CreateSymbolCircle(Point pos, Color color, double radiusMeter) {
            var symbol = new SymbolCreature {
                Uid = GetTimestamp(),
                FillColor = color,
                Z_Order = GetMinZorder() - 1,
                StartPoint = pos,
                SizeMeter = radiusMeter * 2
            };

            AddSymbol(symbol);
        }

        public void CreateSymbolLine(Point startPoint, Point endPoint, double width, Color color) {
            var symbol = new SymbolLine {
                Uid = GetTimestamp(),
                FillColor = color,
                Z_Order = GetMinZorder() - 1,
                StartPoint = startPoint,
                EndPoint = endPoint,
                Width = width
            };

            AddSymbol(symbol);
        }

        public void CreateSymbolText(Point pos, double angle, double lengthMeter, Color color, string caption) {
            var symbol = new SymbolText {
                Uid = GetTimestamp(),
                Caption = caption,
                FillColor = color,
                LengthMeter = lengthMeter,
                Z_Order = GetMinZorder() - 1,
                StartPoint = pos,
                RotationAngle = angle
            };

            AddSymbol(symbol);
        }

        public void CreateSymbolPolygon(PointCollection corners, Color color) {
            var posCenter = new Point();

            foreach (var corner in corners) {
                posCenter.X += corner.X;
                posCenter.Y += corner.Y;
            }

            posCenter.X /= corners.Count;
            posCenter.Y /= corners.Count;

            var cornersMoved = new PointCollection(corners.Count);
            foreach (var corner in corners) {
                var point = corner;
                point.X -= posCenter.X;
                point.Y -= posCenter.Y;
                cornersMoved.Add(point);
            }

            var symbol = new SymbolPolygon {
                Uid = GetTimestamp(),
                FillColor = color,
                Z_Order = GetMinZorder() - 1,
                StartPoint = posCenter,
                Corners = cornersMoved
            };

            AddSymbol(symbol);
        }


        public void CreateSymbolImage(Point pos, double angle, double sizeMeter, string fileName) {
            fileName = System.IO.Path.GetFullPath(fileName);
            var symbol = new SymbolImage {
                Uid = GetTimestamp(),
                Z_Order = GetMinZorder() - 1,
                SizeMeter = sizeMeter,
                StartPoint = pos,
                RotationAngle = angle,
                ImageFileName = fileName
            };

            AddSymbol(symbol);
        }

        #endregion
    }
}