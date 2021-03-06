﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MapViewer.Maps {
    public class PublicMaskedMap : MaskedMap {
        
        public bool ShowPublicCursor { get; set; }

        public bool ShowPublicCursorTemporary { get; set; }
        
        public PublicMaskedMap(Window parent, long groupId) : base(parent, groupId) {

            MaskOpacity = 1.0;
        }

        public override void ScaleToWindow(UIElement element) {
           
        }

        public virtual void ScaleToReal() {
            if (MapImage != null) {
                Log.DebugFormat("ScaleToReal {0} {1}", ScreenScaleMMperM, ImageScaleMperPix);

                if (MapData.ImageScaleMperPix < 1E-15) {
                    MessageBox.Show("Image not calibrated!");
                    return;
                }

                if (ParentWindow is PublicWindow publicWindow) {
                    var cp = new Point(CanvasOverlay.ActualWidth / 2, CanvasOverlay.ActualHeight / 2);
                    Point center;
                    if ((TrfScale.Value.IsIdentity && TrfTranslate.Value.IsIdentity) || !DisplayTransform.Value.HasInverse) {
                        center = new Point(MapImage.Width / 2, MapImage.Height / 2);
                    }
                    else {
                        // ReSharper disable once PossibleNullReferenceException
                        center = DisplayTransform.Inverse.Transform(cp);
                    }
                    Log.DebugFormat("ScaleToReal1 MonitorScaleMMperPixel={0}", publicWindow.MonitorScaleMMperPixel);
                    Log.DebugFormat("ScaleToReal2 CanvasOverlay.ActualWidth={0} MapImage.Width={1}", CanvasOverlay.ActualWidth, MapImage.Width);
                    Log.DebugFormat("ScaleToReal3 center={0} ", center);

                    var scale = ScreenScaleMMperM * ImageScaleMperPix / publicWindow.MonitorScaleMMperPixel;

                    TrfScale.CenterX = 0;
                    TrfScale.CenterY = 0;
                    TrfScale.ScaleX = scale;
                    TrfScale.ScaleY = scale;
                    TrfTranslate.X = (CanvasOverlay.ActualWidth / 2) - scale * center.X;
                    TrfTranslate.Y = (CanvasOverlay.ActualHeight / 2) - scale * center.Y;
                }
            }
        }

        public virtual void ScaleToLinked(MaskedMap mapSource) {
            if (MapImage != null) {

                var privateWindow = mapSource.ParentWindow as PrivateWindow;
                if (privateWindow == null) {
                    return;
                }
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
        }

        public void PublishFrom(MaskedMap mapSource, bool scaleNeedsToRecalculate) {
            Log.InfoFormat("Publish : scaleNeedsToRecalculate={0}", scaleNeedsToRecalculate);

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
                    mapSource.CanvasOverlay.CopyingCanvas(CanvasOverlay);
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
            else if (scaleNeedsToRecalculate) {
                ScaleToReal();
            }
        }

        public void MovePublicCursor(Point pnt, long privateMapId) {
            if (ShowPublicCursor && MapId == privateMapId) {
                var elemCursor = CanvasOverlay.FindElementByUid(PublicCursorUid) as Ellipse;

                var radius = 30 / Scale;
                if (elemCursor == null) {
                    OverlayRing(pnt, radius, Colors.Red, PublicCursorUid);
                }
                else {
                    elemCursor.Width = 2 * radius;
                    elemCursor.Height = 2 * radius;
                    elemCursor.StrokeThickness = 10 / Scale;
                    Canvas.SetLeft(elemCursor, pnt.X - radius);
                    Canvas.SetTop(elemCursor, pnt.Y - radius);
                }
            }
            else {
                RemoveElement(PublicCursorUid);
            }
        }
    }
}
