﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MapViewer.Properties;
using Image = System.Windows.Controls.Image;
using Path = System.IO.Path;
using Point = System.Windows.Point;

namespace MapViewer.Maps {
    public partial class MaskedMap {
        #region Properties

        public static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        public BitmapImage MapImage;

        public BitmapSource BmpMask { get; set; }

        public readonly Canvas CanvasMap = new Canvas();

        public  Canvas CanvasMask = new Canvas();
        
        public Canvas CanvasOverlay = new Canvas();

        public TransformGroup DisplayTransform { get; }

        public Image MaskImage { get; private set; }

        public Image BackgroundImage { get; private set; }

        public BitmapPalette MaskPalette { get; private set; }

        public MapData MapData { get; set; }

        public bool Initiated;

        public double PlayerSizePixel { get; set; } //  == 0 means fixed in meter 

        public double PlayerSizeMeter { get; set; } //  == 0 means dynamic in pixel 

        protected double MaskOpacity { get; set; }

        public double ScreenScaleMMperM { get; set; }

        public const string PublicCursorUid = "Cursor";

        public const string PublicPositionUid = "PublicPos";

        private const string FolderName = "MapViewerFiles";

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

        private Color? _maskColor;
        public Color MaskColor {
            get {
                if (_maskColor.HasValue) {
                    return _maskColor.Value;
                }
                var colorString = Settings.Default.MaskColor;
                try {
                    _maskColor = (Color?) ColorConverter.ConvertFromString(colorString);
                    Log.Info("MaskColor= " + _maskColor);

                    return _maskColor ?? Colors.Black;
                }
                catch {
                    Log.Error("Failed to parse color: " + colorString);
                }
                return Colors.Black;
            }
        }

        public float ImageScaleMperPix {
            get => MapData.ImageScaleMperPix;
            set {
                MapData.ImageScaleMperPix = value;
                ImageScaleChanged?.Invoke(this, null);
            }
        }

        public string Unit {
            get => MapData.Unit;
            set => MapData.Unit = value;
        }

        public bool IsCalibrated => ImageScaleMperPix > 0.0;

        public Window ParentWindow { get; set; }

        public bool IsLinked { get; set; }

        public long GroupId { get; set; }

        public long MapId { get; set; }

        public string ImageFilePath { get; set; }

        public double Scale => MapImage != null ? TrfScale.ScaleX : 1.0;

        public double ScaleDpiFix => MapImage != null ? (MapImage.PixelHeight / MapImage.Height) : 1.0;

        public Dictionary<string, ISymbol> Symbols = new Dictionary<string, ISymbol>();

        public event EventHandler Symbols_Updated;

        public event EventHandler ImageScaleChanged;

        #endregion

        public MaskedMap(Window parent, long groupId) {
            ParentWindow = parent;
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

            PlayerSizeMeter = 0;
            PlayerSizePixel = 20;
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


            // CanvasMask
            CanvasMask.Children.Clear();
            CanvasMask.RenderTransform = DisplayTransform;
            MaskImage = new Image {
                Opacity = MaskOpacity,
                Source = BmpMask,
                Uid = "Mask"

            };
            CanvasMask.Children.Add(MaskImage);

            // CanvasOverlay
            CanvasOverlay.RenderTransform = DisplayTransform;
            UpdatePlayerElementSizes();
        }

        public void CreatePalette() {
            var colors = new List<Color> { Colors.Transparent };
            var color = MaskColor;

            for (var i = 1; i <= 255; i++) {
                colors.Add(color);
            }
            MaskPalette = new BitmapPalette(colors);
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

        public Rect VisibleRectInMap() {
            var rect = new Rect(0.0, 0.0, CanvasMap.ActualWidth, CanvasMap.ActualHeight);

            var inverse = DisplayTransform.Inverse;
            if (inverse != null) {
                var rectOut = inverse.TransformBounds(rect);
                return rectOut;
            }
            return new Rect();
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

        public Point CenterInCanvas() {
            var pos = new Point(CanvasMap.ActualWidth / 2, CanvasMap.ActualHeight / 2);
            return pos;
        }
        
        public Point GetPosInMap(Point pos) {
            var inverse = DisplayTransform.Inverse;
            if (inverse != null) {
                var posOut = inverse.Transform(pos);
                return posOut;
            }
            return new Point();
        }

        public void RotateClockwise() {
            TrfRotation.Angle += 90.0;
            var winSizePix = CanvasMap.RenderSize;
            TrfRotation.CenterX = winSizePix.Width / 2;
            TrfRotation.CenterY = winSizePix.Height / 2;
        }

        public virtual void ScaleToWindow(UIElement element) {

        }

        public double GetMinScale(UIElement element) {
            if (element == null || element.RenderSize.Width == 0) {
                return Math.Min(500 / MapImage.Width, 500 / MapImage.Height);
            }
            return Math.Min(element.RenderSize.Width / MapImage.Width, element.RenderSize.Height / MapImage.Height);
        }

        public void Zoom(double scale, Point pos) {
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

        private static int Between(int val, int min, int max) {
            return Math.Max(Math.Min(val, max), min);
        }

        private static byte[] CreateColorData(int byteCount, byte colorIndex) {
            var colorData = new byte[byteCount];
            for (var i = 0; i < byteCount; i++) {
                colorData[i] = colorIndex;  // B
            }
            return colorData;
        }

        public void MaskCircle(int centerX, int centerY, int radius, byte colorIndex) {

            centerX = (int)(centerX * ScaleDpiFix);
            centerY = (int)(centerY * ScaleDpiFix);
            radius = (int)(radius * ScaleDpiFix);

            var bitmap = BmpMask as WriteableBitmap;
            if (bitmap == null) {
                return;
            }

            var y0 = Between(centerY - radius, 0, bitmap.PixelHeight);
            var yMax = Between(centerY + radius, 0, bitmap.PixelHeight);

            var byteCount = (2 * radius);
            var colorData = CreateColorData(byteCount, colorIndex);

            for (var y = y0; y < yMax; y++) {
                var corda = (int)Math.Sqrt(radius * radius - (y - centerY) * (y - centerY));
                var x0 = Between(centerX - corda, 0, bitmap.PixelWidth);
                var xMax = Between(centerX + corda, 0, bitmap.PixelWidth);
                var rectLine = new Int32Rect(x0, y, xMax - x0, 1);

                bitmap.WritePixels(rectLine, colorData, rectLine.Width, 0);
            }
        }

        public void MaskRectangle(Int32Rect rect, byte colorIndex) {
            rect.X = (int)(rect.X * ScaleDpiFix);
            rect.Y = (int)(rect.Y * ScaleDpiFix);
            rect.Width = (int)(rect.Width * ScaleDpiFix);
            rect.Height = (int)(rect.Height * ScaleDpiFix);

            var bitmap = BmpMask as WriteableBitmap;
            if (bitmap == null) {
                return;
            }

            var x0 = Between(rect.X, 0, bitmap.PixelWidth);
            var y0 = Between(rect.Y, 0, bitmap.PixelHeight);
            var xMax = Between(rect.X + rect.Width, 0, bitmap.PixelWidth);
            var yMax = Between(rect.Y + rect.Height, 0, bitmap.PixelHeight);

            var byteCount = (xMax - x0);
            var colorData = CreateColorData(byteCount, colorIndex);

            var rectLine = new Int32Rect(x0, y0, xMax - x0, 1);
            for (var y = y0; y < yMax; y++) {
                rectLine.Y = y;
                bitmap.WritePixels(rectLine, colorData, byteCount, 0);
            }
        }

        public void MaskPolygon(PointCollection pnts, byte colorIndex) {
            var bitmap = BmpMask as WriteableBitmap;
            if (bitmap == null) {
                return;
            }

            var intPoints = new int[pnts.Count * 2];
            for (var i = 0; i < pnts.Count; i++) {
                intPoints[i * 2] = (int)(ScaleDpiFix * pnts[i].X);
                intPoints[i * 2 + 1] = (int)(ScaleDpiFix * pnts[i].Y);
            }

            bitmap.FillPolygon(intPoints, colorIndex);
        }

        public static Color GetPixelColor(BitmapSource bitmap, int x, int y) {
            if (x < 0 || x >= bitmap.PixelWidth || y < 0 || y >= bitmap.PixelHeight) {
                return Colors.Black;
            }

            const int side = 1;
            var bytesPerPixel = (bitmap.Format.BitsPerPixel + 7) / 8;
            var stride = side * bytesPerPixel;
            var bytes = new byte[side * side * bytesPerPixel];
            var rect = new Int32Rect(x, y, side, side);

            bitmap.CopyPixels(rect, bytes, stride, 0);

            if (bitmap.Format == PixelFormats.Pbgra32) {
                return Color.FromArgb(bytes[3], bytes[2], bytes[1], bytes[0]);
            }
            if (bitmap.Format == PixelFormats.Bgr32) {
                return Color.FromArgb(0xFF, bytes[2], bytes[1], bytes[0]);
            }
            // handle other required formats
            return Colors.Black;
        }

        public static bool GetPixelIsBlack(BitmapSource bitmap, int x, int y)
        {
            if (x < 0 || x >= bitmap.PixelWidth || y < 0 || y >= bitmap.PixelHeight) {
                return true;
            }

            const int side = 1;
            var bytesPerPixel = (bitmap.Format.BitsPerPixel + 7) / 8;
            var stride = side * bytesPerPixel;
            var bytes = new byte[side * side * bytesPerPixel];
            var rect = new Int32Rect(x, y, side, side);

            bitmap.CopyPixels(rect, bytes, stride, 0);

            if (bitmap.Format == PixelFormats.Pbgra32 || bitmap.Format == PixelFormats.Bgr32) {
                return (bytes[2] + bytes[1] + bytes[0]) < 192;
            }
            // handle other required formats
            return true;
        }


        const int DotRadius = 2;
        const int DotSize = DotRadius * 2 + 1;

        public static void WritePixel(WriteableBitmap bitmap, int x, int y, byte[] colorArr) {

            if (x - DotRadius < 0 || y - DotRadius < 0 || x + DotRadius >= bitmap.PixelWidth || y + DotRadius >= bitmap.PixelHeight) {
                return;
            }
            var rect = new Int32Rect(x - DotRadius, y - DotRadius, DotSize, DotSize);
            bitmap.WritePixels(rect, colorArr, rect.Width, 0);
        }

        public void MaskLineOfSight(double centerX, double centerY, double radius, byte colorIndex) {
            centerX = (int)(centerX * ScaleDpiFix);
            centerY = (int)(centerY * ScaleDpiFix);
            radius = (int)(radius * ScaleDpiFix);

            if (!(BmpMask is WriteableBitmap bitmap)) {
                return;
            }

            var colorData = CreateColorData(DotSize * DotSize, colorIndex);
            for (var angle = 0.0; angle <= 2 * Math.PI; angle += 0.005) {
                var cosAngle = Math.Cos(angle);
                var sinAngle = Math.Sin(angle);

                for (var rad = 0.0; rad <= radius; rad += 4.0) {
                    var pntX = (int)(centerX + cosAngle * rad);
                    var pntY = (int)(centerY + sinAngle * rad);
                    if (GetPixelIsBlack(MapImage, pntX, pntY)) {
                        break;
                    }
                    WritePixel(bitmap, pntX, pntY, colorData);
                }
            }
        }

        public void ClearMask() {
            if (BmpMask != null) {
                var rect = new Int32Rect(0, 0, (int)MapImage.Width, (int)MapImage.Height);
                MaskRectangle(rect, 0);
            }
        }

        public void Serialize() {
            MapData.Serialize();
            BitmapUtils.Serialize(BmpMask as WriteableBitmap, CreateFilename(ImageFilePath, ".mask.png"));
            CanvasOverlay.SerializeXaml(CreateFilename(ImageFilePath, ".xaml"));
        }

        public void Deserialize() {
            MapData.Deserialize();
            BmpMask = BitmapUtils.Deserialize(CreateFilename(ImageFilePath, ".mask.png"));
            CanvasOverlay.DeserializeXaml(CreateFilename(ImageFilePath, ".xaml"));
            var shape = CanvasOverlay.FindElementByUid(PublicPositionUid);
            if (shape != null) {
                CanvasOverlay.Children.Remove(shape);
            }
            CanvasOverlay.Loaded += delegate
            {
                 Zoom(1.0,new Point());
            };
        }

        protected static string CreateFilename(string original, string extension) {
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
      
        
        public void AddSymbol(ISymbol symbol) {
            Symbols[symbol.Uid] = symbol;
            Symbols_Updated?.Invoke(this, null);
        }

    }
}
