using System;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace MapViewer {
	public class MapData {
		private float _imageScaleMperPix;
		private int _lastFigureScaleUsed;
		private string _unit;
		
		private readonly string _xmlFilePath;

		public string Unit {
			get => _unit;
            set {
				_unit = value;
				Serialize();
			}
		}

		public float ImageScaleMperPix {
			get => _imageScaleMperPix;
            set {
				_imageScaleMperPix = value;
				Serialize();
			}
		}

		public int LastFigureScaleUsed {
			get => _lastFigureScaleUsed;
            set {
				_lastFigureScaleUsed = value;
				Serialize();
			}
		}

        // ReSharper disable once UnusedMember.Global
        public MapData() {
			_unit = "m";
		}

		public MapData(string path) {
			_xmlFilePath= path;
            _unit = "m";
		}

        public void Copy(MapData source) {
			ImageScaleMperPix = source.ImageScaleMperPix;
			Unit = source.Unit;
			LastFigureScaleUsed = source.LastFigureScaleUsed;
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
