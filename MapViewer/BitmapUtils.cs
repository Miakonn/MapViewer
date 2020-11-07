using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using Brush = System.Drawing.Brush;
using Brushes = System.Drawing.Brushes;
using FontFamily = System.Drawing.FontFamily;
using Rectangle = System.Windows.Shapes.Rectangle;


namespace MapViewer {
	public static class BitmapUtils {
		public static void Serialize(WriteableBitmap wBitMap, string filename) {
			if (wBitMap == null) {
				return;
			}
			try {
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

		private static byte[] CreateColorData(int byteCount, byte colorIndex) {
			var colorData = new byte[byteCount];
			for (var i = 0; i < byteCount; i++) {
				colorData[i] = colorIndex;	// B
			}
			return colorData;
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

			var colorData = CreateColorData(bmp.PixelWidth, color);

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
						j = j - 1;
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
						bmp.WritePixels(rectLine, colorData, x1 - x0, 0);
					}
				}
			}
		}
	}

	public static class CanvasUtils {
		public static void SerializeXaml(this Canvas canvas, string filename ) {
			try {
				var strXAML = XamlWriter.Save(canvas);
				using (var fileStream = File.Create(filename)) {
					using (var streamWriter = new StreamWriter(fileStream)) {
						streamWriter.Write(strXAML);
						streamWriter.Close();
						fileStream.Close();
					}
				}
			}
			catch (Exception ex) {
				MessageBox.Show(ex.Message);				
			}
		}

		public static void DeserializeXaml(this Canvas canvas, string filename) {
			if (!File.Exists(filename)) {
				return;
			}

			try {
				using (var stream = new StreamReader(filename)) {
					var canvasFile = XamlReader.Load(stream.BaseStream) as Canvas;
					if (canvasFile == null || canvasFile.Children.Count == 0) {
						return;
					}

					var childrenList = canvasFile.Children.Cast<UIElement>().ToArray();
					canvasFile.Children.Clear();
					foreach (var child in childrenList) {
						canvas.Children.Add(child);
					}
				}
			}
			catch (Exception ex) {
				MessageBox.Show(ex.Message);
				
			}

		}

		public static void CopyingCanvas(this Canvas canvasSource, Canvas canvasDest) {
			canvasDest.Children.Clear();
			foreach (UIElement child in canvasSource.Children) {
				if (child.Uid != MaskedMap.PublicPositionUid) {
					var xaml = XamlWriter.Save(child);
                    if (XamlReader.Parse(xaml) is UIElement deepCopy) {
						canvasDest.Children.Add(deepCopy);
					}
				}
			}
		}

		public static UIElement FindElementHit(this Canvas canvas) {
			return canvas.Children.Cast<UIElement>().FirstOrDefault(child => child.IsMouseOver);
		}

		public static UIElement FindElementByUid(this Canvas canvas, string uid) {
			return canvas.Children.Cast<UIElement>().FirstOrDefault(child => child.Uid == uid);
		}

        public static IEnumerable<UIElement> FindElementsByUidPrefix(this Canvas canvas, string uid) {
            return canvas.Children.Cast<UIElement>().Where(child => child.Uid.StartsWith(uid));
        }
    }
}
