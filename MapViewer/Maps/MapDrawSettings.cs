using System;

namespace MapViewer.Maps {
    public class MapDrawSettings {
        public double ZoomScale;
        public double ImageScaleMperPix;
        public double MinSymbolSizePixel;
        public bool IsToolActive;

        public double MinSymbolSizePixelScaled => MinSymbolSizePixel / ZoomScale;

        public double GetMinSizePixelFromMeter(double sizeMeter) {
            double sizePixel = sizeMeter / ImageScaleMperPix;
            sizePixel = Math.Max(sizePixel, MinSymbolSizePixelScaled);
            return sizePixel;
        }

    }
}
