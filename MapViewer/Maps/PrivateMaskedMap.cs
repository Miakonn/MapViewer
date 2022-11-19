using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace MapViewer.Maps {


    public class SymbolEventArgs : EventArgs {
        public Dictionary<string, ISymbol> Symbols;

        public SymbolEventArgs(Dictionary<string, ISymbol> symbols) {
            Symbols = symbols;
        }
    }

    public class PrivateMaskedMap: MaskedMap {


        private const string FolderName = "MapViewerFiles";


        public Dictionary<string, ISymbol> Symbols = new Dictionary<string, ISymbol>();

        public event EventHandler Symbols_Updated;


        public PrivateMaskedMap(Window parent, long groupId) : base(parent, groupId) {
            MaskOpacity = 0.3;
            CreatePalette();


            Symbols_Updated += MaskedMap_SymbolsUpdated;
        }


        public override void MoveElement(UIElement elem, Vector move)
        {
            if (elem.Uid.StartsWith("Symbol")) {
                if (Symbols.ContainsKey(elem.Uid)) {
                    Symbols[elem.Uid].Move(move);
                    RaiseSymbolsChanged();
                }

                return;
            }

            Canvas.SetLeft(elem, Canvas.GetLeft(elem) - move.X);
            Canvas.SetTop(elem, Canvas.GetTop(elem) - move.Y);

            if (!elem.IsPlayer()) {
                return;
            }

            var elemName = CanvasOverlay.GetPlayerNameElement(elem);
            CenterPlayerName(elem, elemName);
        }




        public void Zoom(double scale, Point pos)
        {
            var posCenterBefore = CenterInMap();
            TrfScale.ScaleX *= scale;
            TrfScale.ScaleY *= scale;

            if (TrfScale.ScaleX < GetMinScale(CanvasMap)) {
                ScaleToWindow(CanvasMap);
                UpdatePlayerElementSizes();
            }
            else {
                TrfTranslate = new TranslateTransform();
                var offs = CenterInMap() - posCenterBefore;
                UpdatePlayerElementSizes();

                TrfTranslate = new TranslateTransform(offs.X * TrfScale.ScaleX, offs.Y * TrfScale.ScaleY);
            }
            RaiseSymbolsChanged();
        }




        public override void ScaleToWindow(UIElement element) {
            var scale = GetMinScale(element);
            TrfScale.ScaleX = scale;
            TrfScale.ScaleY = scale;
            TrfTranslate.X = 0;
            TrfTranslate.Y = 0;
            UpdatePublicViewRectangle();
        }

        private void UpdatePublicViewRectangle() {
            if (ParentWindow is PrivateWindow privateWin && privateWin.MapPrivate != null) {
                if (!IsLinked && MapId == privateWin.MapPrivate.MapId) {
                    UpdateVisibleRectangle(privateWin.MapPublic.VisibleRectInMap());
                }
            }
        }

        public void LoadImage(string imagePath) {
            try {
                ImageFilePath = imagePath;
                Log.InfoFormat("Loading image {0}", ImageFilePath);
                BitmapFromUri(new Uri(ImageFilePath));
                MapData = new MapData(CreateFilename(ImageFilePath, ".xml"));

                BmpMask = null;
                CanvasOverlay.Children.Clear();

                UpdatePublicViewRectangle();

                Deserialize();
                UpdatePlayerElementSizes();
                CreatePalette();

                if (MapData.LastFigureScaleUsed != 0) {
                    ScreenScaleMMperM = 1000.0 / MapData.LastFigureScaleUsed;
                }

                if (BmpMask == null) {
                    BmpMask = new WriteableBitmap(MapImage.PixelWidth + 2, MapImage.PixelHeight + 2, MapImage.DpiX, MapImage.DpiY,
                        PixelFormats.Indexed8, MaskPalette);
                }
            }
            catch (Exception ex) {
                Log.Error("LoadImage", ex);
                MessageBox.Show("Failed to load image", ex.Message);
                BmpMask = null;
                MapImage = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }


        public void Serialize()
        {
            MapData.Serialize();
            BitmapUtils.Serialize(BmpMask as WriteableBitmap, CreateFilename(ImageFilePath, ".mask.png"));
            CanvasOverlay.RemoveAllSymbolsFromOverlay();
            CanvasOverlay.SerializeXaml(CreateFilename(ImageFilePath, ".xaml"));
            RaiseSymbolsChanged();
        }

        public void Deserialize()
        {
            MapData.Deserialize();
            BmpMask = BitmapUtils.Deserialize(CreateFilename(ImageFilePath, ".mask.png"));
            CanvasOverlay.DeserializeXaml(CreateFilename(ImageFilePath, ".xaml"));
            var shape = CanvasOverlay.FindElementByUid(PublicPositionUid);
            if (shape != null) {
                CanvasOverlay.Children.Remove(shape);
            }
            CanvasOverlay.Loaded += delegate {
                Zoom(1.0, new Point());
            };
        }

        protected static string CreateFilename(string original, string extension)
        {
            var originalFolder = Path.GetDirectoryName(original);
            var originalFilename = Path.GetFileName(original);

            if (originalFolder != null) {
                var folder = Path.Combine(originalFolder, FolderName);
                if (!Directory.Exists(folder)) {
                    try {
                        Directory.CreateDirectory(folder);
                    }
                    catch (Exception ex) {
                        MessageBox.Show(ex.Message);
                        return "";
                    }
                }

                return Path.Combine(folder, originalFilename + extension);
            }
            return "";
        }


        #region Symbols



        public void AddSymbol(ISymbol symbol)
        {
            Symbols[symbol.Uid] = symbol;
            RaiseSymbolsChanged();
        }

        public void RaiseSymbolsChanged()
        {
            Symbols_Updated?.Invoke(this, new SymbolEventArgs(Symbols));

        }

        public void MaskedMap_SymbolsUpdated(object sender, EventArgs e) {
           var se = (SymbolEventArgs)e;

            CanvasOverlay.RemoveAllSymbolsFromOverlay();

            foreach (var symbol in se.Symbols.Values) {
                symbol.CreateElements(CanvasOverlay, Scale, ImageScaleMperPix);
            }



            Debug.WriteLine("MaskedMap_Symbols_Updated!!");
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
                Layer = 50,
                Position = pos,
                SizeMeter = sizeMeter
            };

            AddSymbol(symbol);
        }


        #endregion

    }
}
