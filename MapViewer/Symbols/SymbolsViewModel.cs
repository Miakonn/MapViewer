using MapViewer.Maps;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace MapViewer.Symbols {
    
    [Serializable]
    [XmlInclude(typeof(Symbol)), XmlInclude(typeof(SymbolCreature))]
    public partial class SymbolsViewModel {

        [XmlIgnore]
        private Dictionary<string, Symbol> Symbols { get; set; }

        public Collection<Symbol> SymbolsOnly { get; set; }

        private readonly string UidPrefix;
        
        public event EventHandler SymbolsChanged;

        public SymbolsViewModel() {
            Symbols = new Dictionary<string, Symbol>();
            SymbolsOnly = new Collection<Symbol>();
        }

        public SymbolsViewModel(string prefix) {
            UidPrefix = prefix;
            Symbols = new Dictionary<string, Symbol>();
        }
        
        public void AddSymbol(Symbol symbol) {
            symbol.Uid = UidPrefix + symbol.Uid;
            Symbols[symbol.Uid] = symbol;
            RaiseSymbolsChanged();
        }

        public void AddSymbolWithoutRaise(Symbol symbol) {
            Symbols[symbol.Uid] = symbol;
        }

        public bool SymbolsExists => Symbols.Count > 0;

        public Symbol FindSymbolFromUid(string uid) {
            if (string.IsNullOrWhiteSpace(uid) || !Symbols.ContainsKey(uid)) {
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
            return (DateTime.UtcNow.Subtract(new DateTime(2022, 1, 1)).TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
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
            
        public void DrawSymbols(CanvasOverlay canvas, MapDrawSettings drawSettings) {
            canvas.RemoveMySymbolsFromOverlay(UidPrefix);

            var symbolsInZorder = Symbols.Values.OrderByDescending(s => s.OrderZ);
            foreach (var symbol in symbolsInZorder) {
                symbol.Draw(canvas, drawSettings);
            }
        }

        #region Symbol properties

        private List<Symbol> GetActiveList(Symbol symbolActive) {
            if (symbolActive == null) {
                return new List<Symbol>();
            }

            if (symbolActive.IsSelected) {
                return Symbols.Values.Where(symbol => symbol.IsSelected).ToList();
            }

            ClearSymbolSelection();
            return new List<Symbol> { symbolActive};
        }

        private List<SymbolIcon> GetActiveIconList(Symbol symbolActive) {
            if (symbolActive == null) {
                return new List<SymbolIcon>();
            }

            if (symbolActive.IsSelected) {
               return (from s in Symbols.Values where s.IsSelected && s is SymbolIcon select s as SymbolIcon).ToList();
                
            }

            ClearSymbolSelection();
            return new List<SymbolIcon> { (SymbolIcon)symbolActive };
        }


        public void DeleteSymbol(Symbol symbolActive) {
            foreach (var symbol in GetActiveList(symbolActive)) {
                Symbols.Remove(symbol.Uid);
            }

            RaiseSymbolsChanged();
        }

        public void DuplicateSymbol(Symbol symbolActive) {
            foreach (var symbol in GetActiveList(symbolActive)) {
                AddSymbol(symbol.Copy());
            }
            RaiseSymbolsChanged();
        }


        public void SetSymbolColor(Symbol symbolActive, Color color) {
            foreach (var symbol in GetActiveList(symbolActive)) {
                symbol.FillColor = color;
            }
            RaiseSymbolsChanged();
        }

        public void MoveSymbolToFront(Symbol symbolActive) {
            foreach (var symbol in GetActiveList(symbolActive)) {
                symbol.OrderZ = GetMinOrderZ() - 1;
            }
            RaiseSymbolsChanged();
        }

        public void MoveSymbolToBack(Symbol symbolActive) {
            foreach (var symbol in GetActiveList(symbolActive)) {
                symbol.OrderZ = GetMaxOrderZ() + 1;
            }
            RaiseSymbolsChanged();
        }

        public void RotateSymbol(Symbol symbolActive, RotationDirection direction) {
            foreach (var symbol in GetActiveList(symbolActive)) {
                symbol.Rotate(direction);
            }
            RaiseSymbolsChanged();
        }

        public void ToggleSymbolStatus(Symbol symbolActive, string status) {
            var symbolIcons = GetActiveIconList(symbolActive);
            if (symbolIcons.TrueForAll(s => s.Status == status)) {
                status = string.Empty;
            }
            
            foreach (var symbolIcon in GetActiveIconList(symbolActive)) {
                symbolIcon.Status = status;
            }
            RaiseSymbolsChanged();
        }

        public void MoveSymbolPosition(Symbol symbolActive, Vector move) {
            foreach (var symbol in GetActiveList(symbolActive)) {
                symbol.StartPoint -= move;
            }
            RaiseSymbolsChanged();
        }

        public void MoveSymbolUpDown(Symbol symbolActive, SymbolsViewModel symbolsPmNew) {
            foreach (var symbol in GetActiveList(symbolActive)) {
                Symbols.Remove(symbol.Uid);
                symbolsPmNew.AddSymbol(symbol);
            }
            RaiseSymbolsChanged();
        }

        public void ChangeSymbolSelection(Symbol symbolActive) {
            if (symbolActive == null) {
                return;
            }

            symbolActive.IsSelected = !symbolActive.IsSelected;
            RaiseSymbolsChanged();
        }


        public void NewSymbolSelection(Symbol symbolActive) {
            if (symbolActive == null) {
                return;
            }
            ClearSymbolSelection();
            symbolActive.IsSelected = true;
            RaiseSymbolsChanged();
        }

        public void ClearSymbolSelection() {
            foreach (var symbol in Symbols.Values) {
                symbol.IsSelected = false;
            }

            RaiseSymbolsChanged();
        }
        public void SelectSymbolRectangle(Rect rect) {
            foreach (var symbol in Symbols.Values) {
                if (rect.Contains(symbol.StartPoint)) {
                    symbol.IsSelected = true;
                }
            }

            RaiseSymbolsChanged();
        }

        public void ChangePlayerSizes(double sizeMeterNew) {
            foreach (var symbol in Symbols.Values) {
                if (symbol is SymbolCreature creature) {
                    if (creature.SizeMeter >= 0.5 && creature.SizeMeter <= 1.0) {
                        creature.SizeMeter = sizeMeterNew;
                    }
                }
                if (symbol is SymbolIcon icon) {
                    if (icon.SizeMeter >= 0.5 && icon.SizeMeter <= 1.0) {
                        icon.SizeMeter = sizeMeterNew;
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
            while (i >= 0 && char.IsDigit(caption[i])) {
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

        public bool OpenEditor(Window owner, Point dialogScreenPos, Symbol symbolActive) {
            if (symbolActive == null) {
                return false;
            }

            var result = symbolActive.OpenDialogProp(owner, dialogScreenPos, this);
            RaiseSymbolsChanged();
            return result;
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
            if (symbols == null) {
                return 1.0;
            }
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
