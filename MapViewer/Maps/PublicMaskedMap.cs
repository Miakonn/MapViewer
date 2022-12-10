using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using MapViewer.Symbols;

namespace MapViewer.Maps {
    public class PublicMaskedMap : MaskedMap {
        
        public bool ShowPublicCursor { get; set; }
            
        public bool ShowPublicCursorTemporary { get; set; }

        public PublicWindow ParentWindow { get; set; }

        private PrivateMaskedMap _mapSource;

        public PublicMaskedMap(PublicWindow parent, long groupId) : base(groupId) {
            ParentWindow = parent;
            MaskOpacity = 1.0;
        }
        
        public virtual void ScaleToReal() {
            if (MapImage == null) {
                return;
            }

            if (MapData.ImageScaleMperPix < 1E-15) {
                //MessageBox.Show("Image is not calibrated!");
                return;
            }
                
            var cp = new Point(CanvasOverlay.ActualWidth / 2, CanvasOverlay.ActualHeight / 2);
            Point center;
            if ((TrfScale.Value.IsIdentity && TrfTranslate.Value.IsIdentity) || !DisplayTransform.Value.HasInverse) {
                center = new Point(MapImage.Width / 2, MapImage.Height / 2);
            }
            else {
                // ReSharper disable once PossibleNullReferenceException
                center = DisplayTransform.Inverse.Transform(cp);
            }

            var scale = ScreenScaleMMperM * ImageScaleMperPix / ParentWindow.MonitorScaleMMperPixel;

            if (scale != TrfScale.ScaleX) {
                TrfScale.CenterX = 0;
                TrfScale.CenterY = 0;
                TrfScale.ScaleX = scale;
                TrfScale.ScaleY = scale;
                TrfTranslate.X = (CanvasOverlay.ActualWidth / 2) - scale * center.X;
                TrfTranslate.Y = (CanvasOverlay.ActualHeight / 2) - scale * center.Y;
            }
        }
        
        public Rect VisibleRectInMap() {
            var rect = new Rect(0.0, 0.0, CanvasMap.ActualWidth, CanvasMap.ActualHeight);

            var inverse = DisplayTransform.Inverse;
            if (inverse != null) {
                var rectOut = inverse.TransformBounds(rect);
                return rectOut;
            }
            return new Rect();
        }

        public virtual void ScaleToLinked(PrivateMaskedMap mapSource) {
            if (MapImage == null || mapSource.ParentWindow == null) {
                return;
            }
            var privateWindow = mapSource.ParentWindow;
            var privateWidth = privateWindow.DrawingSpace.ActualWidth;
            var privateHeight = privateWindow.DrawingSpace.ActualHeight;
            var privateAspectRatio = privateWidth / privateHeight;

            var publicWinSizePix = ParentWindow.RenderSize;
            var publicAspectRatio = publicWinSizePix.Width / publicWinSizePix.Height;

            Log.DebugFormat("ScaleToLinked private aspect ratio={0}", privateAspectRatio);
            Log.DebugFormat("ScaleToLinked public aspect ratio={0}", publicAspectRatio);

            if (privateAspectRatio > publicAspectRatio) {
                publicWinSizePix.Height = publicWinSizePix.Width / privateAspectRatio;
            }
            else {
                publicWinSizePix.Width = publicWinSizePix.Height * privateAspectRatio;
            }

            var scale = Math.Min(publicWinSizePix.Width / privateWidth, publicWinSizePix.Height / privateHeight);
            Log.DebugFormat("ScaleToLinked public scale={0}", scale);

            TrfScale = mapSource.TrfScale;
            TrfScale.ScaleX *= scale;
            TrfScale.ScaleY *= scale;

            TrfTranslate = new TranslateTransform();
            var offs = CenterInMap() - mapSource.CenterInMap();
            TrfTranslate = new TranslateTransform(offs.X * TrfScale.ScaleX, offs.Y * TrfScale.ScaleY);

            CanvasMap.RenderTransform = DisplayTransform;
            CanvasMask.RenderTransform = DisplayTransform;
        }

        public void PublishFrom(PrivateMaskedMap mapSource) {

            HookUnhookFromSymbolsVM(mapSource);
            _mapSource = mapSource;

            Unit = mapSource.Unit;
            var newImageLoaded = MapId != mapSource.MapId;
            var newGroupLoaded = GroupId != mapSource.GroupId;

            if (mapSource.BmpMask != null && MaskImage != null) {
                try {
                    BmpMask = mapSource.BmpMask.CloneCurrentValue();
                }
                catch (Exception ex) {
                    Log.Error(ex.Message);
                    MessageBox.Show(ex.Message);
                }
                MaskImage.Source = BmpMask;
            }
            if (mapSource.MapImage != null) {
                if (newImageLoaded) {
                    MapImage = mapSource.MapImage.CloneCurrentValue();
                    BackgroundImage.Source = MapImage;
                    MapId = mapSource.MapId;
                }

                if (newGroupLoaded) {
                    ClearTransformExceptRotation();
                    GroupId = mapSource.GroupId;
                }
            }

            MapData.ImageScaleMperPix = mapSource.MapData.ImageScaleMperPix;

            if (IsLinked) {
                ScaleToLinked(mapSource);
            }
            else{
                ScaleToReal();
            }

            //mapSource.SymbolsPM.RaiseSymbolsChanged();
            HandleSymbolsChanged(mapSource.SymbolsPM, null);
        }

        public void HookUnhookFromSymbolsVM(PrivateMaskedMap mapSourceNew) {
            if (mapSourceNew == _mapSource) {
                return;
            }

            if (_mapSource != null) {
                _mapSource.SymbolsPM.SymbolsChanged -= HandleSymbolsChanged;
            }
            mapSourceNew.SymbolsPM.SymbolsChanged += HandleSymbolsChanged;
        }


        public void MovePublicCursor(Point pnt, long privateMapId) {
            if (ShowPublicCursor && MapId == privateMapId) {
                var elemCursor = CanvasOverlay.FindElementByUid(PublicCursorUid) as Ellipse;

                var radius = 30 / ZoomScale;
                if (elemCursor == null) {
                    OverlayRing(pnt, radius, Colors.Red, PublicCursorUid);
                }
                else {
                    elemCursor.Width = 2 * radius;
                    elemCursor.Height = 2 * radius;
                    elemCursor.StrokeThickness = 10 / ZoomScale;
                    Canvas.SetLeft(elemCursor, pnt.X - radius);
                    Canvas.SetTop(elemCursor, pnt.Y - radius);
                }
            }
            else {
                RemoveElement(PublicCursorUid);
            }
        }

        private void HandleSymbolsChanged(object sender, EventArgs e) {
            var drawSettings = new MapDrawSettings {
                ZoomScale = ZoomScale,
                ImageScaleMperPix = ImageScaleMperPix,
                MinSymbolSizePixel = PlayerMinSizePixel,
                IsToolActive = false,
                UseTextBackground = UseTextBackground
            };

            var symbolsPm = (SymbolsViewModel)sender;
            symbolsPm?.DrawSymbols(CanvasOverlay, drawSettings);
        }

    }
}
