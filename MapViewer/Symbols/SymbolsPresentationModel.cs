using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace MapViewer.Symbols {


    public class SymbolEventArgs : EventArgs {
        public SymbolsPresentationModel SymbolsPM;

        public SymbolEventArgs(SymbolsPresentationModel symbolsPm)
        {
            SymbolsPM = symbolsPm;
        }
    }

    public class SymbolsPresentationModel {
        public Dictionary<string, ISymbol> Symbols { get; set; }

        public event EventHandler SymbolsChanged;
        
        public SymbolsPresentationModel()
        {
            Symbols = new Dictionary<string, ISymbol>();

        }
        
        public void AddSymbol(ISymbol symbol)
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
        
        public string GetTimestamp()
        {
            return "Symbol_" + (int)(DateTime.UtcNow.Subtract(new DateTime(2022, 1, 1)).TotalSeconds);
        }

        public void CreateOverlayCreature(Point pos, Color color, double sizeMeter, string text)
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

        public void SetSymbolColor(string uid, Color color)
        {
            if (!Symbols.ContainsKey(uid)) {
                return;
            }
            Symbols[uid].Color = color;
            RaiseSymbolsChanged();
        }

        public void DeleteSymbol(string uid)
        {
            if (!Symbols.ContainsKey(uid)) {
                return;
            }
            Symbols.Remove(uid);
            RaiseSymbolsChanged();
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
        
        public void UpdateElements(Canvas canvas, double Scale, double ImageScaleMperPix)
        {
            canvas.RemoveAllSymbolsFromOverlay();

            var symbolsInZorder = Symbols.Values.OrderByDescending(s => s.Zorder);

            foreach (var symbol in symbolsInZorder) {
                symbol.CreateElements(canvas, Scale, ImageScaleMperPix);
            }

            Debug.WriteLine("MaskedMap_Symbols_Updated!!");
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

    }
}
