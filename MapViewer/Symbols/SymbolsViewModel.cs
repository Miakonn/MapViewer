using MapViewer.Maps;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;
using Point = System.Windows.Point;

namespace MapViewer.Symbols {
    
    [Serializable]
    [XmlInclude(typeof(Symbol)), XmlInclude(typeof(SymbolCreature))]
    public partial class SymbolsViewModel {
        [XmlIgnore]
        private Dictionary<string, Symbol> Symbols { get; set; }

        public Collection<Symbol> SymbolsOnly { get; set; }

        public const string UidPrefix = "S";
        
        public event EventHandler SymbolsChanged;
        
        public SymbolsViewModel() {
            Symbols = new Dictionary<string, Symbol>();
        }
        
        public void AddSymbol(Symbol symbol) {
            Symbols[symbol.Uid] = symbol;
            RaiseSymbolsChanged();
        }

        public void AddSymbolWithoutRaise(Symbol symbol) {
            Symbols[symbol.Uid] = symbol;
        }

        public Symbol FindSymbolFromUid(string uid) {
            if (!Symbols.ContainsKey(uid)) {
                return null;
            }
            return Symbols[uid];
        }

        public void DeleteAllSymbols() {
            Symbols.Clear();
            RaiseSymbolsChanged();
        }

        public void RaiseSymbolsChanged() {
            SymbolsChanged?.Invoke(this, null);
        }
        
        public static string GetTimestamp() {
            return UidPrefix + (DateTime.UtcNow.Subtract(new DateTime(2022, 1, 1)).TotalMilliseconds);
        }

        public int GetMaxOrderZ() {
            if (Symbols.Count == 0) {
                return 1000;
            }
            return Symbols.Values.Select(symbol => symbol.OrderZ).Max();
        }

        public int GetMinOrderZ() {
            if (Symbols.Count == 0) {
                return 1000;
            }
            return Symbols.Values.Select(symbol => symbol.OrderZ).Min();
        }
        
        public void DrawSymbols(Canvas canvas, MapDrawingSettings drawSettings) {
            canvas.RemoveAllSymbolsFromOverlay();

            var symbolsInZorder = Symbols.Values.OrderByDescending(s => s.OrderZ);
            foreach (var symbol in symbolsInZorder) {
                symbol.Draw(canvas, drawSettings);
            }
        }

        #region Symbol properties

        public void DeleteSymbol(Symbol symbolActive) {
            if (symbolActive == null) {
                return;
            }
            Symbols.Remove(symbolActive.Uid);
            RaiseSymbolsChanged();
        }

        public void DuplicateSymbol(Symbol symbolActive) {
            if (symbolActive == null) {
                return;
            }

            var symbolCopy = symbolActive.Copy();
            AddSymbol(symbolCopy);
            RaiseSymbolsChanged();
        }


        public void SetSymbolColor(Symbol symbolActive, Color color) {
            if (symbolActive == null) {
                return;
            }
            symbolActive.FillColor = color;
            RaiseSymbolsChanged();
        }

        public void MoveSymbolToFront(Symbol symbolActive) {
            if (symbolActive == null) {
                return;
            }

            symbolActive.OrderZ = GetMinOrderZ() - 1;
            RaiseSymbolsChanged();
        }

        public void MoveSymbolToBack(Symbol symbolActive) {
            if (symbolActive == null) {
                return;
            }

            symbolActive.OrderZ = GetMaxOrderZ() + 1;
            RaiseSymbolsChanged();
        }

        public void RotateSymbol(Symbol symbolActive, RotationDirection direction) {
            if (symbolActive == null) {
                return;
            }
     
            if (symbolActive.IsSelected) {
                foreach (var symbol in Symbols.Values) {
                    if (symbol.IsSelected) {
                        symbol.Rotate(direction);
                    }
                }
            }
            else {
                symbolActive.Rotate(direction);
                ClearSymbolSelection();
            }
            RaiseSymbolsChanged();
        }
        
        public void MoveSymbolPosition(Symbol symbolActive, Vector move) {
            if (symbolActive == null) {
                return;
            }


            if (symbolActive.IsSelected) {
                foreach (var symbol in Symbols.Values) {
                    if (symbol.IsSelected) {
                        symbol.StartPoint -= move;
                    }
                }
            }
            else {
                symbolActive.StartPoint -= move;
                ClearSymbolSelection();
            }

            RaiseSymbolsChanged();
        }

        public void MoveSymbolUpDown(Symbol symbolActive, SymbolsViewModel symbolsPmNew) {
            if (symbolActive == null) {
                return;
            }

            Symbols.Remove(symbolActive.Uid);
            symbolsPmNew.AddSymbol(symbolActive);
            RaiseSymbolsChanged();
        }

        public void ChangeSymbolSelection(Symbol symbolActive) {
            if (symbolActive == null) {
                return;
            }

            symbolActive.IsSelected = !symbolActive.IsSelected;
            RaiseSymbolsChanged();
        }

        public void ClearSymbolSelection() {
            foreach (var symbol in Symbols.Values) {
                symbol.IsSelected = false;
            }

            RaiseSymbolsChanged();
        }

        public void ChangePlayerSizes(double sizeMeterNew) {
            foreach (var symbol in Symbols.Values) {
                if (symbol is SymbolCreature creature) {
                    if (creature.SizeMeter >= 0.7 && creature.SizeMeter <= 1.0) {
                        creature.SizeMeter = sizeMeterNew;
                    }
                }
            }

            RaiseSymbolsChanged();
        }

        public static string CountUpCaption(string caption) {
            if (string.IsNullOrWhiteSpace(caption)) {
                return string.Empty;
            }

            int i = caption.Length - 1;
            while (char.IsDigit(caption[i]) && i >= 0) {
                i--;
            }

            i++;
            if (i <= caption.Length - 1) {
                int num = int.Parse(caption.Substring(i)) + 1;
                return caption.Substring(0, i) + num.ToString();
            }
            else {
                return caption + "1";
            }
        }

        public static double ToRadians(double degrees) {
            return degrees * (Math.PI / 180.0);
        }

        public static double ToDegrees(double radians) {
            return radians * (180.0 / Math.PI);
        }

        public void OpenEditor(Symbol symbolActive, Point dialogScreenPos) {
            if (symbolActive == null) {
                return;
            }

            symbolActive.OpenDialogProp(dialogScreenPos, this);
            RaiseSymbolsChanged();
        }

        #endregion

        public void Serialize(string filename) {
            try {
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

        // Check and calulate if symbol positions needs to scaled to fit.
        private double GetNeededScaleToFitThisImage(Collection<Symbol> symbols, Size imSize) {
            double maxX = 0;
            double maxY = 0;
            foreach (var symbol in symbols) {
                maxX = Math.Max(maxX, symbol.StartPoint.X);
                maxY = Math.Max(maxY, symbol.StartPoint.Y);
            }

            var scaleX = (maxX > imSize.Width) ? (imSize.Width - 10) / maxX : 1;
            var scaleY = (maxY > imSize.Height) ? (imSize.Height - 10) / maxY : 1;

            return Math.Min(scaleX, scaleY);
        }

        public void Deserialize(string filename, Size imSize) {
            if (!File.Exists(filename)) {
                return;
            }

            try {
                var serializer = new XmlSerializer(GetType());
                using (var reader = XmlReader.Create(filename)) {
                    var symbolVM = (SymbolsViewModel)serializer.Deserialize(reader);
                    var scale = GetNeededScaleToFitThisImage(symbolVM.SymbolsOnly, imSize);

                    foreach (var symbol in symbolVM.SymbolsOnly) {
                        symbol.StartPoint = new Point(scale * symbol.StartPoint.X, scale * symbol.StartPoint.Y);
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
