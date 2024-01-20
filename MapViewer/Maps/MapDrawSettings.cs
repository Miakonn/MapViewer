using System;

namespace MapViewer.Maps {
    public class MapDrawSettings {
        public double ZoomScale { get; }

        public double ImageScaleMperPix { get; }

        public double MinSymbolSizePixel { get; }

        public bool IsToolActive { get; }

        public MapDrawSettings(double zoomScale, double imageScaleMperPix, double minSymbolSizePixel, bool isToolActive)
        {
            if (zoomScale < 1E-20)
            {
                MaskedMap.Log.Error($"MapDrawSettings : zoomscale = {zoomScale}");
            }

            if (zoomScale < 1E-20) {
                MaskedMap.Log.Error($"MapDrawSettings : imageScaleMperPix = {imageScaleMperPix}");
            }

            ZoomScale = zoomScale < 1E-20 ? 1.0: zoomScale;
            ImageScaleMperPix = imageScaleMperPix < 1E-20 ? 1.0 : zoomScale;
            MinSymbolSizePixel = minSymbolSizePixel;
            IsToolActive = isToolActive;
        }

        public double MinSymbolSizePixelScaled => MinSymbolSizePixel / ZoomScale;

        public double GetMinSizePixelFromMeter(double sizeMeter) {
            double sizePixel = sizeMeter / ImageScaleMperPix;
            sizePixel = Math.Max(sizePixel, MinSymbolSizePixelScaled);
            return sizePixel;
        }

    }
}
