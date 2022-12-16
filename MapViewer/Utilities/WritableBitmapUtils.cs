using MapViewer.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;
using Colors = System.Windows.Media.Colors;

// ReSharper disable once CheckNamespace
namespace MapViewer {
	public static class WritableBitmapUtils {

		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);
		private static BitmapPalette MaskPalette => new BitmapPalette(new List<Color> { Colors.Transparent, MaskColor });

        public const byte ColorIndexTransparent = 0;

        public const byte ColorIndexMask = 1;

        public static byte ColorIndex(bool fMask) {
            return fMask ? ColorIndexMask : ColorIndexTransparent;
        }

        public static void Serialize(this WriteableBitmap wBitMap, string filename) {

            try {
                if (wBitMap == null) {
                    if (File.Exists(filename)) {
                        File.Delete(filename);
                    }
                    return;
                }
                using (var stream = new FileStream(filename, FileMode.Create)) {
                    var encoder = new PngBitmapEncoder();

                    encoder.Frames.Add(BitmapFrame.Create(wBitMap));
                    encoder.Save(stream);
                }
            }
            catch (Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}

		public static WriteableBitmap Deserialize(string filename) {
			if (!File.Exists(filename)) {
				return null;
			}
			try {
				var img= new BitmapImage();
				img.BeginInit();
				img.CacheOption = BitmapCacheOption.OnLoad;
				img.UriSource = new Uri(filename, UriKind.Relative);
				img.CreateOptions = BitmapCreateOptions.None;
				img.EndInit();

				var wBmp = new WriteableBitmap(img);
				return wBmp;
			}
			catch (Exception ex) {
				MessageBox.Show(ex.Message);
				return null;
			}
		}

        private static int Between(int val, int min, int max) {
            return Math.Max(Math.Min(val, max), min);
        }

        public static WriteableBitmap CreateMaskBitmap(BitmapImage mapImage) {
            return new WriteableBitmap(
                mapImage.PixelWidth + 2,
                mapImage.PixelHeight + 2,
                mapImage.DpiX, mapImage.DpiY,
                PixelFormats.Indexed1, MaskPalette);
        }

		private static (byte[], int)  CreatePixelData(PixelFormat format, int width, int height, byte colorIndex) {
            if (format != PixelFormats.Indexed8 && format != PixelFormats.Indexed1) {
                var msg = $"Unsupported mask format: {format}";
                Log.Error(msg);
                throw new Exception(msg);
            }
             
            byte data = colorIndex;
            int byteCount = width * height;
            int stride = width;
            if (format == PixelFormats.Indexed1) {
                byteCount = byteCount / 8 + height;
                stride = (width + 7) / 8;
                data = (byte)(colorIndex == ColorIndexTransparent ? 0x00 : 0xFF);
            }

            var pixelData = new byte[byteCount];
			for (var i = 0; i < byteCount; i++) {
				pixelData[i] = data;
			}

            return (pixelData, stride);
        }

		/// <summary>
		/// Draws a filled polygon
		/// Add the first point also at the end of the array if the line should be closed.
		/// </summary>
		/// <param name="bmp">The writable bitmap.</param>
		/// <param name="points">The points of the polygon in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, ..., xn, yn).</param>
		/// <param name="color">The color for the line.</param>
		public static void FillPolygon(this WriteableBitmap bmp, int[] points, byte color) {

			// Use refs for faster access (really important!) speeds up a lot!
			var w = bmp.PixelWidth;
			var h = bmp.PixelHeight;

			var pn = points.Length;
			var pnh = points.Length >> 1;
			var intersectionsX = new int[pnh];

			// Find y min and max (slightly faster than scanning from 0 to height)
			var yMin = h;
			var yMax = 0;
			for (var i = 1; i < pn; i += 2) {
				var py = points[i];
				if (py < yMin) yMin = py;
				if (py > yMax) yMax = py;
			}
			if (yMin < 0) yMin = 0;
			if (yMax >= h) yMax = h - 1;

			var (pixelData, stride) = CreatePixelData(bmp.Format, bmp.PixelWidth, 1, color);

			// Scan line from min to max
			for (var y = yMin; y <= yMax; y++) {
				// Initial point x, y
				float vxi = points[0];
				float vyi = points[1];

				// Find all intersections
				// Based on http://alienryderflex.com/polygon_fill/
				var intersectionCount = 0;
				for (var i = 2; i < pn; i += 2) {
					// Next point x, y
					float vxj = points[i];
					float vyj = points[i + 1];

					// Is the scan line between the two points
					if (vyi < y && vyj >= y
					 || vyj < y && vyi >= y) {
						// Compute the intersection of the scan line with the edge (line between two points)
						intersectionsX[intersectionCount++] = (int)(vxi + (y - vyi) / (vyj - vyi) * (vxj - vxi));
					}
					vxi = vxj;
					vyi = vyj;
				}

				// Sort the intersections from left to right using Insertion sort 
				// It's faster than Array.Sort for this small data set
				for (var i = 1; i < intersectionCount; i++) {
					var t = intersectionsX[i];
					var j = i;
					while (j > 0 && intersectionsX[j - 1] > t) {
						intersectionsX[j] = intersectionsX[j - 1];
						j--;
					}
					intersectionsX[j] = t;
				}

				// Fill the pixels between the intersections
				for (var i = 0; i < intersectionCount - 1; i += 2) {
					var x0 = intersectionsX[i];
					var x1 = intersectionsX[i + 1];

					// Check boundary
					if (x1 > 0 && x0 < w) {
						if (x0 < 0) x0 = 0;
						if (x1 >= w) x1 = w - 1;

						// Fill the pixels
						var rectLine = new Int32Rect(x0, y, x1 - x0, 1);
						bmp.WritePixels(rectLine, pixelData, stride, 0);
					}
				}
			}
		}

        public static void FillCircle(this WriteableBitmap bmp, int centerX, int centerY, int radius, byte colorIndex) {
         
            var y0 = Between(centerY - radius, 0, bmp.PixelHeight);
            var yMax = Between(centerY + radius, 0, bmp.PixelHeight);

            var byteCount = (2 * radius);
            var (pixelData, stride) = CreatePixelData(bmp.Format, byteCount, 1, colorIndex);

            for (var y = y0; y < yMax; y++) {
                var corda = (int)Math.Sqrt(radius * radius - (y - centerY) * (y - centerY));
                var x0 = Between(centerX - corda, 0, bmp.PixelWidth);
                var xMax = Between(centerX + corda, 0, bmp.PixelWidth);
                var rectLine = new Int32Rect(x0, y, xMax - x0, 1);

                bmp.WritePixels(rectLine, pixelData, stride, 0);
            }
        }

        public static void FillRectangle(this WriteableBitmap bmp, int left, int top, int right, int bottom, byte colorIndex) {
            left =   Between(left, 0, bmp.PixelWidth);
            top =    Between(top, 0, bmp.PixelHeight);
            right =  Between(right, 0, bmp.PixelWidth);
            bottom = Between(bottom, 0, bmp.PixelHeight);

            var byteCount = (right - left);
            var (pixelData, stride) = CreatePixelData(bmp.Format, byteCount, 1, colorIndex);

            var rectLine = new Int32Rect(left, top, right - left, 1);
            for (var y = top; y < bottom; y++) {
                rectLine.Y = y;
                bmp.WritePixels(rectLine, pixelData, stride, 0);
            }
        }

        public static void UnmaskLineOfSight(this WriteableBitmap bmp, BitmapImage mapImage, int centerX, int centerY, int radius) {

			var (pixelData, stride) = CreatePixelData(bmp.Format, DotSize, DotSize, ColorIndexTransparent);
            for (var angle = 0.0; angle <= 2 * Math.PI; angle += 0.005) {
                var cosAngle = Math.Cos(angle);
                var sinAngle = Math.Sin(angle);

                for (var rad = 0.0; rad <= radius; rad += 4.0) {
                    var pntX = (int)(centerX + cosAngle * rad);
                    var pntY = (int)(centerY + sinAngle * rad);
                    if (GetPixelIsBlack(mapImage, pntX, pntY)) {
                        break;
                    }
                    bmp.WritePixel(pntX, pntY, pixelData, stride);
                }
            }
        }


        const int DotRadius = 2;
        const int DotSize = DotRadius * 2 + 1;

        private static void WritePixel(this WriteableBitmap bitmap, int x, int y, byte[] pixelArr, int stride) {

            if (x - DotRadius < 0 || y - DotRadius < 0 || x + DotRadius >= bitmap.PixelWidth || y + DotRadius >= bitmap.PixelHeight) {
                return;
            }
            var rect = new Int32Rect(x - DotRadius, y - DotRadius, DotSize, DotSize);
            bitmap.WritePixels(rect, pixelArr, stride, 0);
        }

        //public static Color GetPixelColor(BitmapSource bitmap, int x, int y) {
        //    if (x < 0 || x >= bitmap.PixelWidth || y < 0 || y >= bitmap.PixelHeight) {
        //        return Colors.Black;
        //    }

        //    const int side = 1;
        //    var bytesPerPixel = (bitmap.Format.BitsPerPixel + 7) / 8;
        //    var stride = side * bytesPerPixel;
        //    var bytes = new byte[side * side * bytesPerPixel];
        //    var rect = new Int32Rect(x, y, side, side);

        //    bitmap.CopyPixels(rect, bytes, stride, 0);

        //    if (bitmap.Format == PixelFormats.Pbgra32) {
        //        return Color.FromArgb(bytes[3], bytes[2], bytes[1], bytes[0]);
        //    }
        //    if (bitmap.Format == PixelFormats.Bgr32) {
        //        return Color.FromArgb(0xFF, bytes[2], bytes[1], bytes[0]);
        //    }
        //    if (bitmap.Format == PixelFormats.Indexed8 && bitmap.Palette != null) {
        //        var color = bitmap.Palette.Colors[bytes[0]];
        //        return color;
        //    }
        //    // handle other required formats
        //    return Colors.Black;
        //}

        public static bool GetPixelIsBlack(BitmapSource bitmap, int x, int y) {
            if (x < 0 || x >= bitmap.PixelWidth || y < 0 || y >= bitmap.PixelHeight) {
                return true;
            }

            var bytesPerPixel = (bitmap.Format.BitsPerPixel + 7) / 8;
            var stride = bytesPerPixel;
            var bytes = new byte[bytesPerPixel];
            var rect = new Int32Rect(x, y, 1, 1);

            bitmap.CopyPixels(rect, bytes, stride, 0);

            if (bitmap.Format == PixelFormats.Pbgra32 || bitmap.Format == PixelFormats.Bgr32) {
                return (bytes[2] + bytes[1] + bytes[0]) < 192;
            }
            if (bitmap.Format == PixelFormats.Indexed8 && bitmap.Palette != null) {
                var color = bitmap.Palette.Colors[bytes[0]];
                return (color.R + color.G + color.B) < 192;
            }
            if (bitmap.Format == PixelFormats.Indexed1 && bitmap.Palette != null) {
                var color = bitmap.Palette.Colors[bytes[0]];
                return (color.R + color.G + color.B) < 192;
            }
            // handle other required formats
            return true;
        }



        private static Color? _maskColor;
        public static Color MaskColor {
            get {
                if (_maskColor.HasValue) {
                    return _maskColor.Value;
                }
                var colorString = Settings.Default.MaskColor;
                try {
                    _maskColor = (Color?)ColorConverter.ConvertFromString(colorString);
                   Log.Info("MaskColor= " + _maskColor);

                    return _maskColor ?? Colors.Black;
                }
                catch {
                    Log.Error("Failed to parse color: " + colorString);
                }
                return Colors.Black;
            }
        }
    }
}
