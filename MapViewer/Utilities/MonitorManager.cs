using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;


namespace MapViewer.Utilities {


	public class MonitorManager {
		private readonly Edid _edid;

		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public MonitorManager() {
			_edid = new Edid(RunDumpEdid());
		}

		private static string RunDumpEdid() {


			Mouse.OverrideCursor = Cursors.Wait;
			var process = new Process();
			try {
				var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
				if (path == null) {
					return null;
				}
				process.StartInfo.FileName = Path.Combine(path, "DumpEDID\\DumpEDID.exe");
				process.StartInfo.Arguments = "";
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.CreateNoWindow = true;
				process.Start();
				Thread.Sleep(1000);
				Mouse.OverrideCursor = null;

#if DEBUG
				return File.ReadAllText(Path.Combine(path, "Mickes-skärmar.txt"));
#else 
				return process.StandardOutput.ReadToEnd();
#endif
			}
			catch (Exception ex) {
				Log.Error("Failed to run DumpEDID.exe.", ex);
				MessageBox.Show("Failed to run DumpEDID.exe: \n" + ex.Message);
			}
			Mouse.OverrideCursor = null;
			return null;
		}


		public Monitor GetLargestActiveMonitor() {
			if (_edid.Monitors.Count == 0) {
				return null;
			}
			Monitor monitorLargest = null;
			double valueLargest = -1;
			foreach (var monitor in _edid.Monitors) {
				if (monitor.ImageSize.HasValue && monitor.ImageSize.Value.Width > valueLargest) {
					monitorLargest = monitor;
					valueLargest = monitor.ImageSize.Value.Width;
				}
			}
			if (monitorLargest == null) {
				Log.InfoFormat("Found no active monitors!");
			}
			else  if (monitorLargest.Parameters.ContainsKey("Serial Number")) {
				Log.InfoFormat("Selected monitor: " + monitorLargest.Parameters["Serial Number"]);
			}
			else if (monitorLargest.Parameters.ContainsKey("Serial Number")) {
				Log.InfoFormat("Selected monitor: " + monitorLargest.Parameters["Serial Number"]);
			}
			else if (monitorLargest.Parameters.ContainsKey("Registry Key")) {
				Log.InfoFormat("Selected monitor: " + monitorLargest.Parameters["Registry Key"]);
			}
			else {
				Log.InfoFormat("Selected monitor: Unknown");
			}
			return monitorLargest;
		}
	}

	public class Monitor {
		private readonly Dictionary<string, string> _parameters = new Dictionary<string, string>();

		public bool? Active {
			get {
				if (_parameters.ContainsKey("Active")) {
					return _parameters["Active"].Contains("Yes");
				}
				return null;
			}
		}

		/// <summary>
		/// Return Image Size in mm, fall back to Maximum Images Size
		/// </summary>
		public Size? ImageSize {
			get {
				var size = ParseSizeFloat10("Image Size");
				if (!size.HasValue) {
					size = ParseSizeFloat10("Maximum Image Size");
				}
				return size;
			}
		}

		/// <summary>
		/// Return Maximum Resolution in pixels
		/// </summary>
		public Size? MaximumResolution {
			get {
				return ParseSizeInt("Maximum Resolution");
			}
		}

		public Dictionary<string, string> Parameters {
			get { return _parameters; }
		}


		private Size? ParseSizeFloat10(string key) {
			if (_parameters.ContainsKey(key)) {
				double width, height;
				var parts = _parameters[key].Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length >= 3 && double.TryParse(parts[0], out width) && double.TryParse(parts[2], out height)) {
					return new Size((int)(10 * width), (int)(10 * height));
				}
			}
			return null;
		}

		private Size? ParseSizeInt(string key) {
			if (_parameters.ContainsKey(key)) {
				int width, height;
				var parts = _parameters[key].Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length >= 3 && int.TryParse(parts[0], out width) && int.TryParse(parts[2], out height)) {
					return new Size(width, height);
				}
			}
			return null;
		}

	}

	internal class Edid {
		private readonly List<Monitor> _monitors = new List<Monitor>();
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public List<Monitor> Monitors {
			get { return _monitors; }
		}

		private static void ParseLine(string line, Monitor monitor) {
			var parts = line.Split(":".ToCharArray());
			if (parts.Length == 2) {
				monitor.Parameters.Add(parts[0].Trim(), parts[1].Trim());
			}
		}

		public Edid(string text) {
			text = text.Replace("\r\r\n", "\r\n");
			if (string.IsNullOrWhiteSpace(text)) {
				return;
			}
			Log.Info(text);
			var lines = text.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

			Monitor monitor = null;
			foreach (var line in lines) {
				if (line.StartsWith("Active")) {
					monitor = new Monitor();
					_monitors.Add(monitor);
					ParseLine(line, monitor);
				}
				else if (monitor != null) {
					ParseLine(line, monitor);
				}
			}
		}
	}


}
