using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapViewer.Maps {
    public class PrivateMaskedMap: MaskedMap {
        public PrivateMaskedMap(Window parent, long groupId) : base(parent, groupId) {
            MaskOpacity = 0.3;
            CreatePalette();
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
    }
}
