using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MapViewer.Symbols;
using Path = System.IO.Path;
using Size = System.Windows.Size;

namespace MapViewer.Maps {

    public class PrivateMaskedMap: MaskedMap {
        private const string FolderName = "MapViewerFiles";
        
        public SymbolsViewModel SymbolsPM;

        public SymbolsViewModel SystemSymbolsPM;

        public PrivateWindow ParentWindow { get; set; }

        public PrivateMaskedMap(PrivateWindow parent, long groupId) : base(groupId) {
            ParentWindow = parent;
            MaskOpacity = 0.3;
            CreatePalette();
            SymbolsPM = new SymbolsViewModel("S");
            SymbolsPM.SymbolsChanged += HandleSymbolsChanged;

            SystemSymbolsPM = new SymbolsViewModel("Q");
            SystemSymbolsPM.SymbolsChanged += HandleSymbolsChanged;
        }

        public void Zoom(double scale, Point pos)
        {
            var posCenterBefore = CenterInMap();
            TrfScale.ScaleX *= scale;
            TrfScale.ScaleY *= scale;

            if (TrfScale.ScaleX < GetMinScale(CanvasMap)) {
                ScaleToWindow(CanvasMap);
            }
            else {
                TrfTranslate = new TranslateTransform();
                var offs = CenterInMap() - posCenterBefore;
                TrfTranslate = new TranslateTransform(offs.X * TrfScale.ScaleX, offs.Y * TrfScale.ScaleY);
            }
            UpdatePublicViewRectangle();
            SymbolsPM.RaiseSymbolsChanged();
        }

        public void ScaleToWindow(UIElement element) {
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
                    UpdateVisibleRectangle(privateWin.MapPublic);
                }
            }
        }

        public void SetCursor(Cursor cursor) {
            if (cursor == null) {
                cursor = Cursors.Arrow;
            }
            CanvasMap.Cursor = cursor;
            CanvasMask.Cursor = cursor;
            CanvasOverlay.Cursor = cursor;
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
                MessageBox.Show("Failed to load image" + ex.Message, "Error");
                BmpMask = null;
                MapImage = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }


        public void Serialize()
        {
            MapData.Serialize();
            (BmpMask as WriteableBitmap).Serialize(CreateFilename(ImageFilePath, ".mask.png"));
            SymbolsPM.Serialize(CreateFilename(ImageFilePath, ".symbols.xml"));
        }

        public void Deserialize()
        {
            MapData.Deserialize();
            BmpMask = BitmapUtils.Deserialize(CreateFilename(ImageFilePath, ".mask.png"));
            var imSize = new Size(MapImage.PixelWidth, MapImage.PixelHeight);

            SymbolsPM.Deserialize(CreateFilename(ImageFilePath, ".symbols.xml"), imSize);
            var shape = CanvasOverlay.FindElementByUid(PublicPositionUid);
            if (shape != null) {
                CanvasOverlay.Children.Remove(shape);
            }
            CanvasOverlay.Loaded += delegate {
                Zoom(1.0, new Point());
            };
            SymbolsPM.RaiseSymbolsChanged();
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

        public void HandleSymbolsChanged(object sender, EventArgs e) {

            var drawSettings = new MapDrawSettings {
                ZoomScale = ZoomScale,
                ImageScaleMperPix = ImageScaleMperPix,
                MinSymbolSizePixel = PlayerMinSizePixel,
                IsToolActive = ParentWindow.ActiveTool != null,
                UseTextBackground = UseTextBackground
            };

            SymbolsPM?.DrawSymbols(CanvasOverlay, drawSettings);
            SystemSymbolsPM?.DrawSymbols(CanvasOverlay, drawSettings);
        }

        public void RemoveVisibleRectangle() {
            var symbolFrame = SystemSymbolsPM.FindSymbolFromUid(PublicPositionUid);
            SystemSymbolsPM.DeleteSymbol(symbolFrame);
        }

        public void UpdateVisibleRectangle(PublicMaskedMap mapPublic) {
            if (mapPublic.IsLinked || mapPublic.MapId != MapId) {
                RemoveVisibleRectangle();
                return;
            }
            
            var rect = mapPublic.VisibleRectInMap();

            var symbol = SystemSymbolsPM.FindSymbolFromUid(PublicPositionUid);
            var sizeMeter = rect.Width * ImageScaleMperPix;
            var widthMeter = rect.Height * ImageScaleMperPix;

            var pos = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2); 
            if (symbol == null) {
                symbol = SystemSymbolsPM.CreateSymbolFrame(pos, sizeMeter, widthMeter , 0, Colors.Red);
                PublicPositionUid = symbol.Uid;
            }
            else if (symbol is SymbolFrame symbolFrame) {
                symbolFrame.StartPoint = pos;
                symbolFrame.SizeMeter = sizeMeter;
                symbolFrame.WidthMeter = widthMeter;

            }
            SystemSymbolsPM.RaiseSymbolsChanged();
        }

        #endregion

    }
}
