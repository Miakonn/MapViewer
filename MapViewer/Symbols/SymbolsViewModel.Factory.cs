using System.Windows;
using System.Windows.Media;

namespace MapViewer.Symbols {
    public partial class SymbolsViewModel {
        #region Factory

        public Symbol CreateSymbolCreature(Point pos, Color color, double sizeMeter, string caption) {
            var symbol = new SymbolCreature {
                Uid = GetTimestamp(),
                FillColor = color,
                Opacity = 1,
                Caption = caption,
                OrderZ = GetMinOrderZ() - 1,
                StartPoint = pos,
                SizeMeter = sizeMeter
            };

            AddSymbol(symbol);
            return symbol;
        }

        public Symbol CreateSymbolCircle(Point pos, Color color, double radiusMeter) {
            var symbol = new SymbolCircle {
                Uid = GetTimestamp(),
                FillColor = color,
                Opacity = 0.8,
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
                Opacity = 0.8,
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
                Opacity = 0.8,
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
                Opacity = 0.8,
                WidthDegrees = widthDegrees,
                OrderZ = GetMinOrderZ() - 1,
                StartPoint = pos,
                RotationDegree = angleDegree
            };

            AddSymbol(symbol);
            return symbol;
        }

        public Symbol CreateSymbolIcon(Point pos) {
            var symbol = new SymbolIcon {
                Uid = GetTimestamp(),
                OrderZ = GetMinOrderZ() - 1,
                Opacity = 1.0,
                SizeMeter = 1,
                StartPoint = pos,
                FillColor = Colors.Black,
            };

            AddSymbol(symbol);
            return symbol;
        }

        public Symbol CreateSymbolFrame(Point startPoint, double sizeM, double widthM, double angleDeg, Color color) {
            var symbol = new SymbolFrame {
                Uid = GetTimestamp(),
                FillColor = color,
                Opacity = 0.8,
                OrderZ = int.MinValue,
                StartPoint = startPoint,
                SizeMeter = sizeM,
                WidthMeter = widthM,
                RotationDegree = angleDeg
            };

            AddSymbol(symbol);
            return symbol;
        }

        #endregion
    }
}