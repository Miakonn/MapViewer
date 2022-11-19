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
    [XmlInclude(typeof(Symbol)), XmlInclude(typeof(CreatureSymbol))]
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

        public void CreateSymbolCreature(Point pos, Color color, double sizeMeter, string text)
        {
            var symbol = new CreatureSymbol {
                Uid = GetTimestamp(),
                Color = color,
                Caption = text,
                Zorder = GetMinZorder() - 1,
                Position = pos,
                SizeMeter = sizeMeter
            };

            AddSymbol(symbol);
        }

        public int GetMaxZorder()
        {
            if (Symbols.Count == 0) {
                return 1000;
            }
            return Symbols.Values.Select(symbol => symbol.Zorder).Max();
        }

        public int GetMinZorder()
        {
            if (Symbols.Count == 0) {
                return 1000;
            }
            return Symbols.Values.Select(symbol => symbol.Zorder).Min();
        }
        
        public void UpdateElements(Canvas canvas, double Scale, double ImageScaleMperPix)
        {
            canvas.RemoveAllSymbolsFromOverlay();

            var symbolsInZorder = Symbols.Values.OrderByDescending(s => s.Zorder);

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
            Symbols[uid].Color = color;
            RaiseSymbolsChanged();
        }

        public void MoveSymbolToBack(string uid)
        {
            if (!Symbols.ContainsKey(uid)) {
                return;
            }

            Symbols[uid].Zorder = GetMaxZorder() + 1;
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
                        AddSymbol(symbol);
                    }
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

    }
}
