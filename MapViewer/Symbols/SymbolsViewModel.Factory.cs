using System.Windows;
using System.Windows.Media;

namespace MapViewer.Symbols {
    public partial class SymbolsViewModel {
        #region Factory

        public Symbol CreateSymbolCreature(Point pos, Color color, double sizeMeter, string caption) {
            var symbol = new SymbolCreature {
                Uid = GetTimestamp(),
                FillColor = color,
                Caption = caption,
                OrderZ = GetMinOrderZ() - 1,
                StartPoint = pos,
                SizeMeter = sizeMeter
            };

            AddSymbol(symbol);
            return symbol;
        }

        public Symbol CreateSymbolCircle(Point pos, Color color, double radiusMeter) {
            var symbol = new SymbolCreature {
                Uid = GetTimestamp(),
                FillColor = color,
                OrderZ = GetMinOrderZ() - 1,
                StartPoint = pos,
                SizeMeter = radiusMeter * 2
            };

            AddSymbol(symbol);
            return symbol;
        }

        public Symbol CreateSymbolRect(Point startPoint, double sizeM, double widthM, double angleDeg, Color color) {
            var symbol = new SymbolRectangle {
                Uid = GetTimestamp(),
                FillColor = color,
                OrderZ = GetMinOrderZ() - 1,
                StartPoint = startPoint,
                SizeMeter = sizeM,
                WidthMeter = widthM,
                RotationDegree = angleDeg
            };

            AddSymbol(symbol);
            return symbol;
        }

        public Symbol CreateSymbolText(Point pos, double angle, double sizeMeter, Color color, string caption) {
            var symbol = new SymbolText {
                Uid = GetTimestamp(),
                Caption = caption,
                FillColor = color,
                SizeMeter = sizeMeter,
                OrderZ = GetMinOrderZ() - 1,
                StartPoint = pos,
                RotationDegree = angle
            };

            AddSymbol(symbol);
            return symbol;
        }


        public Symbol CreateSymbolCone(Point pos, double angleDegree, double lengthMeter, double widthDegrees, Color color) {
            var symbol = new SymbolCone() {
                Uid = GetTimestamp(),
                FillColor = color,
                SizeMeter = lengthMeter,
                WidthDegrees = widthDegrees,
                OrderZ = GetMinOrderZ() - 1,
                StartPoint = pos,
                RotationDegree = angleDegree
            };

            AddSymbol(symbol);
            return symbol;
        }


        public Symbol CreateSymbolPolygon(PointCollection corners, Color color) {
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
                OrderZ = GetMinOrderZ() - 1,
                StartPoint = posCenter,
                Corners = cornersMoved
            };

            AddSymbol(symbol);
            return symbol;
        }

        public Symbol CreateSymbolImage(Point pos) {
            var symbol = new SymbolImage {
                Uid = GetTimestamp(),
                OrderZ = GetMinOrderZ() - 1,
                SizeMeter = 4,
                StartPoint = pos
            };

            AddSymbol(symbol);
            return symbol;
        }

        #endregion
    }
}