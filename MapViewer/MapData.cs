using System;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace MapViewer {
	public class MapData {
		private float _imageLengthM;
		private readonly string _xmlFilePath;

		public float ImageLengthM {
			get { return _imageLengthM; }
			set {
				_imageLengthM = value;
				Serialize();
			}
		}

		public MapData() {}

		public MapData(string imPath) {
			_xmlFilePath= imPath + ".xml";
		}

		public void Copy(MapData source) {
			ImageLengthM = source.ImageLengthM;
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
