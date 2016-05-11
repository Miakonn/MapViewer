using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Xml.Serialization;
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
				var img = new BitmapImage(new Uri(filename, UriKind.Relative));
				img.CreateOptions = BitmapCreateOptions.None;
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

				var childrenList = canvasFile.Children.Cast<System.Windows.UIElement>().ToArray();
				canvasFile.Children.Clear();
				foreach (System.Windows.UIElement child in childrenList) {
					canvas.Children.Add(child);
				}

			}
			catch (Exception ex) {
				System.Windows.MessageBox.Show(ex.Message);
				
			}

		}

		public static void CopyingCanvas(Canvas canvasSource, Canvas canvasDest) {
			canvasDest.Children.Clear();
			foreach (System.Windows.UIElement child in canvasSource.Children) {
				var xaml = System.Windows.Markup.XamlWriter.Save(child);
				var deepCopy = System.Windows.Markup.XamlReader.Parse(xaml) as UIElement;
				canvasDest.Children.Add(deepCopy);
			}
		}

	}
}
