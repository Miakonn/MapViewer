using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;

namespace MapViewer.Maps {
    public class MaskedMap {
        #region Properties

        public static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public BitmapImage MapImage;

        public WriteableBitmap BmpMask { get; set; }

        public readonly Canvas CanvasMap = new Canvas();

        public  Canvas CanvasMask = new Canvas();
        
        public CanvasOverlay CanvasOverlay = new CanvasOverlay();

        public TransformGroup DisplayTransform { get; }

        public Image MaskImage { get; private set; }

        public Image BackgroundImage { get; private set; }

        public MapData MapData { get; set; }

        public bool Initiated;

        public double PlayerMinSizePixel { get; set; }

        public double PlayerSizeMeter { get; set; } 

        protected double MaskOpacity { get; set; }

        public double ScreenScaleMMperM { get; set; }

        public const string PublicCursorUid = "Cursor";

        public string PublicPositionUid  { get; set; }

        private readonly RotateTransform _trfRotation;
        public RotateTransform TrfRotation {
            get => _trfRotation;
            set {
                _trfRotation.Angle = value.Angle;
                _trfRotation.CenterX = value.CenterX;
                _trfRotation.CenterY = value.CenterY;
            }
        }

        private readonly ScaleTransform _trfScale;
        public ScaleTransform TrfScale {
            get => _trfScale;
            set {
                _trfScale.ScaleX = value.ScaleX;
                _trfScale.ScaleY = value.ScaleY;
                _trfScale.CenterX = value.CenterX;
                _trfScale.CenterY = value.CenterY;
            }

        }

        private readonly TranslateTransform _trfTranslate;
        public TranslateTransform TrfTranslate {
            get => _trfTranslate;
            set {
                _trfTranslate.X = value.X;
                _trfTranslate.Y = value.Y;
            }
        }

        public MapDrawSettings GetMapDrawSettings(bool isToolACtive) {
            return new MapDrawSettings {
                ZoomScale = ZoomScale,
                ImageScaleMperPix = ImageScaleMperPix,
                MinSymbolSizePixel = PlayerMinSizePixel,
                IsToolActive = isToolACtive
            };
        }

        public double ImageScaleMperPix {
            get => MapData.ImageScaleMperPix;
            set {
                if (value >= 0.0  && value < 1E15) {
                    MapData.ImageScaleMperPix = value;
                    ImageScaleChanged?.Invoke(this, null);
                }
            }
        }

        public string Unit {
            get => MapData.Unit;
            set => MapData.Unit = value;
        }

        public bool IsCalibrated => ImageScaleMperPix > 0.0;

        public bool IsLinked { get; set; }

        public long GroupId { get; set; }

        public long MapId { get; set; }

        public string ImageFilePath { get; set; }

        public double ZoomScale => MapImage != null ? TrfScale.ScaleX : 1.0;

        public double ScaleDpiFix => MapImage != null ? (MapImage.PixelHeight / MapImage.Height) : 1.0;

        public event EventHandler ImageScaleChanged;

        #endregion

        public MaskedMap(long groupId) {
            GroupId = groupId;
            MapId = DateTime.Now.Ticks;

            MaskOpacity = 1.0;
            DisplayTransform = new TransformGroup();

            _trfRotation = new RotateTransform(0.0);
            _trfScale = new ScaleTransform(1.0, 1.0);
            _trfTranslate = new TranslateTransform(0.0, 0.0);

            DisplayTransform.Children.Add(TrfScale);
            DisplayTransform.Children.Add(TrfTranslate);
            DisplayTransform.Children.Add(TrfRotation);

            MapData = new MapData(null);

            PlayerSizeMeter = 0.8;
            PlayerMinSizePixel = 10;
            IsLinked = false;
        }

        public void BitmapFromUri(Uri source) {
            MapImage = new BitmapImage();
            MapImage.BeginInit();
            MapImage.UriSource = source;
            MapImage.CacheOption = BitmapCacheOption.OnLoad;
            MapImage.EndInit();
        }

        public void Create() {
            Initiated = true;

            // CanvasMap
            CanvasMap.Children.Clear();
            CanvasMap.RenderTransform = DisplayTransform;
            BackgroundImage = new Image {
                Source = MapImage,
                Uid = "Map"
            };
            CanvasMap.Children.Add(BackgroundImage);
            
            // CanvasOverlay
            CanvasOverlay.RenderTransform = DisplayTransform;
        }

        public void CreateBmpMaskIfNull() {
            if (BmpMask == null) {
                BmpMask = WritableBitmapUtils.CreateMaskBitmap(MapImage);
            }
        }

        public void UpdateBmpMaskToLayer() {
            CanvasMask.Children.Clear();
            if (BmpMask == null) {
                return;
            }
            CanvasMask.RenderTransform = DisplayTransform;
            MaskImage = new Image {
                Opacity = MaskOpacity,
                Source = BmpMask,
                Uid = "Mask"

            }; 
            CanvasMask.Children.Add(MaskImage);
        }

        public void CopyTransform(MaskedMap source) {
            TrfTranslate = source.TrfTranslate;
            TrfRotation = source.TrfRotation;
            TrfScale = source.TrfScale;
        }

        protected void ClearTransformExceptRotation() {
            TrfScale.ScaleX = 1;
            TrfScale.ScaleY = 1;
            TrfScale.CenterX = 0;
            TrfScale.CenterY = 0;
            TrfTranslate.X = 0;
            TrfTranslate.Y = 0;
        }

        public Point CenterInMap() {
            var pos = new Point(CanvasMap.ActualWidth / 2, CanvasMap.ActualHeight / 2);
            var inverse = DisplayTransform.Inverse;
            if (inverse != null) {
                var posOut = inverse.Transform(pos);
                return posOut;
            }
            return new Point();
        }
        
        //public Point GetPosInMap(Point pos) {
        //    var inverse = DisplayTransform.Inverse;
        //    if (inverse != null) {
        //        var posOut = inverse.Transform(pos);
        //        return posOut;
        //    }
        //    return new Point();
        //}

        public virtual void RotateClockwise() {
            TrfRotation.Angle += 90.0;
            var winSizePix = CanvasMap.RenderSize;
            TrfRotation.CenterX = winSizePix.Width / 2;
            TrfRotation.CenterY = winSizePix.Height / 2;
        }
        
        public double GetMinScale(UIElement element) {
            if (MapImage == null || MapImage.Width == 0 || MapImage.Height == 0) {
                return 1;
            }

            if (element == null || element.RenderSize.Width == 0) {
                return Math.Min(500 / MapImage.Width, 500 / MapImage.Height);
            }
            
            return Math.Min(element.RenderSize.Width / MapImage.Width, element.RenderSize.Height / MapImage.Height);
        }
        
        public void Translate(Vector move) {
            TrfTranslate.X += move.X;
            TrfTranslate.Y += move.Y;
        }
        
        public void DumpDisplayTransform(string label) {
            foreach (var trf in DisplayTransform.Children) {
                Log.DebugFormat("{0}: Matrix={1}", label, trf.Value);
            }

            Log.DebugFormat("{0}: Value={1}", label, DisplayTransform.Value);
        }
        
        public void MaskCircle(int centerX, int centerY, int radius, byte colorIndex) {
            CreateBmpMaskIfNull();
            if (BmpMask == null) {
                return;
            }

            centerX = (int)(centerX * ScaleDpiFix);
            centerY = (int)(centerY * ScaleDpiFix);
            radius =  (int)(radius * ScaleDpiFix);
            BmpMask.FillCircle(centerX, centerY, radius, colorIndex);
            
            UpdateBmpMaskToLayer();
        }

        public void MaskRectangle(Point pntTL, Point pntBR, byte colorIndex) {
            CreateBmpMaskIfNull();
            if (BmpMask == null) {
                return;
            }

            int left =   (int)(pntTL.X * ScaleDpiFix);
            int top =    (int)(pntTL.Y * ScaleDpiFix);
            int right =  (int)(pntBR.X * ScaleDpiFix);
            int bottom = (int)(pntBR.Y * ScaleDpiFix);
            BmpMask.FillRectangle(left, top, right, bottom, colorIndex);
            UpdateBmpMaskToLayer();
        }

        public void MaskPolygon(PointCollection pnts, byte colorIndex) {
            CreateBmpMaskIfNull();
            if (BmpMask == null) {
                return;
            }

            var intPoints = new int[pnts.Count * 2];
            for (var i = 0; i < pnts.Count; i++) {
                intPoints[i * 2] = (int)(pnts[i].X * ScaleDpiFix);
                intPoints[i * 2 + 1] = (int)(pnts[i].Y * ScaleDpiFix);
            }

            BmpMask.FillPolygon(intPoints, colorIndex);
            UpdateBmpMaskToLayer();
        }

        public void UnmaskLineOfSight(Point center, double radius) {
           CreateBmpMaskIfNull();
            if (BmpMask == null) {
                return;
            }

            int centerX =   (int)(center.X * ScaleDpiFix);
            int centerY =   (int)(center.Y * ScaleDpiFix);
            int radiusInt = (int)(radius * ScaleDpiFix);
            BmpMask.UnmaskLineOfSight(MapImage, centerX, centerY, radiusInt);

            UpdateBmpMaskToLayer();
        }


    }
}
