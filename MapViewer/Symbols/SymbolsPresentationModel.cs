using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;
using Point = System.Windows.Point;

namespace MapViewer.Symbols {


    public class SymbolEventArgs : EventArgs {
        public SymbolsPresentationModel SymbolsPM;

        public SymbolEventArgs(SymbolsPresentationModel symbolsPm)
        {
            SymbolsPM = symbolsPm;
        }
    }

    [Serializable]
    [XmlInclude(typeof(Symbol)), XmlInclude(typeof(SymbolCreature))]
    public class SymbolsPresentationModel {
        [XmlIgnore]
        private Dictionary<string, Symbol> Symbols { get; set; }

        public Collection<Symbol> SymbolsOnly { get; set; }


        public event EventHandler SymbolsChanged;
        
        public SymbolsPresentationModel()
        {
            Symbols = new Dictionary<string, Symbol>();
        }
        
        public void AddSymbol(Symbol symbol)
        {
            Symbols[symbol.Uid] = symbol;
            RaiseSymbolsChanged();
        }

        public void AddSymbolWithoutRaise(Symbol symbol)
        {
            Symbols[symbol.Uid] = symbol;
        }

        public void RemoveAllSymbols()
        {
            Symbols.Clear();
            RaiseSymbolsChanged();
        }

        public void RaiseSymbolsChanged()
        {
            SymbolsChanged?.Invoke(this, new SymbolEventArgs(this));

        }
        
        private string GetTimestamp()
        {
            return "Symbol_" + (int)(DateTime.UtcNow.Subtract(new DateTime(2022, 1, 1)).TotalSeconds);
        }

        #region Factory

        public void CreateSymbolCreature(Point pos, Color color, double sizeMeter, string caption)
        {
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

        public void CreateSymbolCircle(Point pos, Color color, double radiusMeter)
        {
            var symbol = new SymbolCreature {
                Uid = GetTimestamp(),
                FillColor = color,
                Z_Order = GetMinZorder() - 1,
                StartPoint = pos,
                SizeMeter = radiusMeter * 2
            };

            AddSymbol(symbol);
        }

        public void CreateSymbolLine(Point startPoint, Point endPoint, double width, Color color)
        {
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

        public void CreateSymbolText(Point pos, double angle, Color color, string caption)
        {
            var symbol = new SymbolText {
                Uid = GetTimestamp(),
                FillColor = color,
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

        #endregion


        public int GetMaxZorder()
        {
            if (Symbols.Count == 0) {
                return 1000;
            }
            return Symbols.Values.Select(symbol => symbol.Z_Order).Max();
        }

        public int GetMinZorder()
        {
            if (Symbols.Count == 0) {
                return 1000;
            }
            return Symbols.Values.Select(symbol => symbol.Z_Order).Min();
        }
        
        public void UpdateElements(Canvas canvas, double Scale, double ImageScaleMperPix)
        {
            canvas.RemoveAllSymbolsFromOverlay();

            var symbolsInZorder = Symbols.Values.OrderByDescending(s => s.Z_Order);

            foreach (var symbol in symbolsInZorder) {
                symbol.CreateElements(canvas, Scale, ImageScaleMperPix);
            }

            Debug.WriteLine("MaskedMap_Symbols_Updated!!");
        }

        #region Symbol properties
 
        public void DeleteSymbol(string uid)
        {
            if (!Symbols.ContainsKey(uid)) {
                return;
            }
            Symbols.Remove(uid);
            RaiseSymbolsChanged();
        }
       
        public void SetSymbolColor(string uid, Color color)
        {
            if (!Symbols.ContainsKey(uid)) {
                return;
            }
            Symbols[uid].FillColor = color;
            RaiseSymbolsChanged();
        }

        public void MoveSymbolToBack(string uid)
        {
            if (!Symbols.ContainsKey(uid)) {
                return;
            }

            Symbols[uid].Z_Order = GetMaxZorder() + 1;
            RaiseSymbolsChanged();
        }

        public void MoveSymbol(string uid, Vector move)
        {
            if (!Symbols.ContainsKey(uid)) {
                return;
            }

            Symbols[uid].Move(move);
            RaiseSymbolsChanged();
        }

        public void MoveElementUpDown(string uid, SymbolsPresentationModel symbolsPmNew)
        {
            if (!Symbols.ContainsKey(uid)) {
                return;
            }

            var symbolToMove = Symbols[uid];
            Symbols.Remove(uid);
            symbolsPmNew.AddSymbol(symbolToMove);
            RaiseSymbolsChanged();
        }

        #endregion



        public void Serialize(string filename)
        {
            try {
                if (Symbols.Count == 0) {
                    return;
                }

                SymbolsOnly = new Collection<Symbol>();
                foreach (var symbol in Symbols.Values) {
                    SymbolsOnly.Add(symbol);
                }

                var serializer = new XmlSerializer(GetType());
                using (var writer = XmlWriter.Create(filename)) {
                    serializer.Serialize(writer, this);
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        public void Deserialize(string filename)
        {
            if (!File.Exists(filename)) {
                return;
            }

            try {
                var serializer = new XmlSerializer(GetType());
                using (var reader = XmlReader.Create(filename)) {
                    var spm = (SymbolsPresentationModel )serializer.Deserialize(reader);

                    foreach (var symbol in spm.SymbolsOnly) {
                        AddSymbolWithoutRaise(symbol);
                    }
                }
                RaiseSymbolsChanged();
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

    }
}
