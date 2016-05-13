using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Markup;


namespace MapViewer {
	public static class BitmapUtils {

		public static void Serialize(WriteableBitmap wbitmap, string filename) {
			if (wbitmap == null) {
				return;
			}
			using (FileStream stream = new FileStream(filename, FileMode.Create)) {
				PngBitmapEncoder encoder = new PngBitmapEncoder();
			
				encoder.Frames.Add(BitmapFrame.Create(wbitmap));
				encoder.Save(stream);
			}
		}

		public static WriteableBitmap Deserialize(string filename) {
			if (!File.Exists(filename)) {
				return null;
			}
			try {
				var img = new BitmapImage(new Uri(filename, UriKind.Relative)) {
					CreateOptions = BitmapCreateOptions.None
				};
				return new WriteableBitmap(img);
			}
			catch {
				return null;
			}
		}

		public static void SerializeXaml(Canvas canvas, string filename ) {
			string mystrXAML = XamlWriter.Save(canvas);
			FileStream filestream = File.Create(filename);
			StreamWriter streamwriter = new StreamWriter(filestream);
			streamwriter.Write(mystrXAML);
			streamwriter.Close();
			filestream.Close();
		}

		public static void DeserializeXaml(Canvas canvas, string filename) {
			if (!File.Exists(filename)) {
				return;
			}

			try {
				var stream = new StreamReader(filename);

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
			catch (Exception ex) {
				MessageBox.Show(ex.Message);
				
			}

		}

		public static void CopyingCanvas(Canvas canvasSource, Canvas canvasDest) {
			canvasDest.Children.Clear();
			foreach (UIElement child in canvasSource.Children) {
				if (child.Uid != "PublicView") {
					var xaml = XamlWriter.Save(child);
					var deepCopy = XamlReader.Parse(xaml) as UIElement;
					if (deepCopy != null) {
						canvasDest.Children.Add(deepCopy);
					}
				}
			}
		}

		public static UIElement FindHitElement(Canvas canvas) {
			return canvas.Children.Cast<UIElement>().FirstOrDefault(child => child.Uid != "PublicView" && child.IsMouseOver);
		}

		public static UIElement FindElementByUid(Canvas canvas, string uid) {
			return canvas.Children.Cast<UIElement>().FirstOrDefault(child => child.Uid == uid);
		}
	}
}
