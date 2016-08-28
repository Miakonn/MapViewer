using System;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace MapViewer {
	public class MapData {
		private float _imageScaleMperPix;
		private string _unit;

		private readonly string _xmlFilePath;

		public string Unit {
			get { return _unit;  }
			set {
				_unit = value;
				Serialize();
			}
		}

		public float ImageScaleMperPix {
			get { return _imageScaleMperPix; }
			set {
				_imageScaleMperPix = value;
				Serialize();
			}
		}

		public MapData() {
			_unit = "m";
		}

		public MapData(string path) {
			_xmlFilePath= path;
		}

		public void Copy(MapData source) {
			ImageScaleMperPix = source.ImageScaleMperPix;
			Unit = source.Unit;
		}

		public void Serialize() {
			if (string.IsNullOrWhiteSpace(_xmlFilePath)) {
				return;
			}
			try {
				var serializer = new XmlSerializer(typeof (MapData));
				using (TextWriter writer = new StreamWriter(_xmlFilePath)) {
					serializer.Serialize(writer, this);
				}
			}
			catch (Exception ex) {
				MessageBox.Show(ex.Message);				
			}
		}


		public void Deserialize() {
			try {
				if (!File.Exists(_xmlFilePath)) {
					return;
				}
				var deserializer = new XmlSerializer(typeof (MapData));
				TextReader reader = new StreamReader(_xmlFilePath);
				var mapdata = (MapData) deserializer.Deserialize(reader);
				reader.Close();
				Copy(mapdata);
			}
			catch (Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}

	}
}
