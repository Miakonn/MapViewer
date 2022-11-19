using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MapViewer.Symbols;
using Path = System.IO.Path;

namespace MapViewer.Maps {



    public class PrivateMaskedMap: MaskedMap {
        private const string FolderName = "MapViewerFiles";
        
        public SymbolsPresentationModel SymbolsPM;


        public PrivateMaskedMap(Window parent, long groupId) : base(parent, groupId) {
            MaskOpacity = 0.3;
            CreatePalette();
            SymbolsPM = new SymbolsPresentationModel();
            SymbolsPM.SymbolsChanged += HandleSymbolsChanged;
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
            SymbolsPM.Serialize(CreateFilename(ImageFilePath, ".symbols.xml"));
        }

        public void Deserialize()
        {
            MapData.Deserialize();
            BmpMask = BitmapUtils.Deserialize(CreateFilename(ImageFilePath, ".mask.png"));
            SymbolsPM.Deserialize(CreateFilename(ImageFilePath, ".symbols.xml"));
            var shape = CanvasOverlay.FindElementByUid(PublicPositionUid);
            if (shape != null) {
                CanvasOverlay.Children.Remove(shape);
            }
            CanvasOverlay.Loaded += delegate {
                Zoom(1.0, new Point());
            };
            RaiseSymbolsChanged();
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



        public void RaiseSymbolsChanged()
        {
            SymbolsPM.RaiseSymbolsChanged();
        }

        public void HandleSymbolsChanged(object sender, EventArgs e) {
            var se = (SymbolEventArgs)e;
            se.SymbolsPM.UpdateElements(CanvasOverlay, Scale, ImageScaleMperPix);
        }



        #endregion

    }
}
