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
    public partial class SymbolsPresentationModel {
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

        public void DeleteAllSymbols()
        {
            Symbols.Clear();
            RaiseSymbolsChanged();
        }

        public void RaiseSymbolsChanged()
        {
            SymbolsChanged?.Invoke(this, null);
        }
        
        private string GetTimestamp()
        {
            return "Symbol_" + (int)(DateTime.UtcNow.Subtract(new DateTime(2022, 1, 1)).TotalSeconds);
        }

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
        
        public void UpdateElements(Canvas canvas, MapDrawingSettings drawSettings)
        {
            canvas.RemoveAllSymbolsFromOverlay();

            var symbolsInZorder = Symbols.Values.OrderByDescending(s => s.Z_Order);
            foreach (var symbol in symbolsInZorder) {
                symbol.CreateElements(canvas, drawSettings);
            }
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

        public void MoveSymbolToFront(string uid) {
            if (!Symbols.ContainsKey(uid)) {
                return;
            }

            Symbols[uid].Z_Order = GetMinZorder() - 1;
            RaiseSymbolsChanged();
        }

        public void MoveSymbolToBack(string uid) {
            if (!Symbols.ContainsKey(uid)) {
                return;
            }

            Symbols[uid].Z_Order = GetMaxZorder() + 1;
            RaiseSymbolsChanged();
        }

        public void MoveSymbolPosition(string uid, Vector move) {
            if (!Symbols.ContainsKey(uid)) {
                return;
            }

            Symbols[uid].StartPoint -= move;
            RaiseSymbolsChanged();
        }

        public void MoveSymbolUpDown(string uid, SymbolsPresentationModel symbolsPmNew)
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

        public void Deserialize(string filename) {
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
