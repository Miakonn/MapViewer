using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace MapViewer.Utilities {
    
    public class MonitorManager {
        private readonly Edid _edid;

        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MonitorManager() {
            _edid = new Edid(RunDumpEdid());
        }

        private static string RunDumpEdid() {
            Mouse.OverrideCursor = Cursors.Wait;
            try {
                var path = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
                if (path == null) {
                    return null;
                }

                var process = new Process {
                    StartInfo = {
                        FileName = Path.Combine(path, "DumpEDID\\DumpEDID.exe"),
                        Arguments = "-a",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                Thread.Sleep(1000);
                Mouse.OverrideCursor = null;

                return process.StandardOutput.ReadToEnd();
            }
            catch (Exception ex) {
                Log.Error("Failed to run DumpEDID.exe.", ex);
                MessageBox.Show("Failed to run DumpEDID.exe: \n" + ex.Message);
            }

            Mouse.OverrideCursor = null;
            return null;
        }

        public List<string> MonitorList => (from monitor in _edid.Monitors
            where !monitor.Active.HasValue || monitor.Active.Value
            select monitor.Name).ToList();

        public List<Monitor> Monitors => _edid.Monitors;


        public class Monitor {
            public bool? Active {
                get {
                    if (Parameters.ContainsKey("Active")) {
                        return Parameters["Active"].Contains("Yes");
                    }

                    return null;
                }
            }

            /// <summary>
            /// Return Image Size in mm, fall back to Maximum Images Size
            /// </summary>
            public Size? ImageSize => ParseSizeFloat10("Image Size") ?? ParseSizeFloat10("Maximum Image Size");

            /// <summary>
            /// Return Maximum Resolution in pixels
            /// </summary>
            public Size? MaximumResolution => ParseSizeInt("Maximum Resolution");

            /// <summary>
            /// Return some format of name
            /// </summary>
            public string Name {
                get {
                    if (Parameters.ContainsKey("Monitor Name")) {
                        return Parameters["Monitor Name"];
                    }

                    if (Parameters.ContainsKey("Registry Key")) {
                        var param = Parameters["Registry Key"];
                        var parts = param.Split('\\');
                        if (parts.Length == 3) {
                            return parts[1];
                        }
                    }

                    if (Parameters.ContainsKey("ManufacturerID")) {
                        return Parameters["ManufacturerID"];
                    }

                    return "Unknown";
                }
            }

            public Dictionary<string, string> Parameters { get; } = new Dictionary<string, string>();

            private Size? ParseSizeFloat10(string key) {
                if (Parameters.ContainsKey(key)) {
                    var parts = Parameters[key].Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3 && double.TryParse(parts[0], out var width) &&
                        double.TryParse(parts[2], out var height)) {
                        return new Size((int) (10 * width), (int) (10 * height));
                    }
                }

                return null;
            }

            private Size? ParseSizeInt(string key) {
                if (Parameters.ContainsKey(key)) {
                    var parts = Parameters[key].Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3 && int.TryParse(parts[0], out var width) &&
                        int.TryParse(parts[2], out var height)) {
                        return new Size(width, height);
                    }
                }

                return null;
            }

        }

        internal class Edid {
            public List<Monitor> Monitors { get; } = new List<Monitor>();

            private static void ParseLine(string line, Monitor monitor) {
                var parts = line.Split(":".ToCharArray());
                if (parts.Length == 2) {
                    monitor.Parameters.Add(parts[0].Trim(), parts[1].Trim());
                }
            }

            public Edid(string text) {
                if (string.IsNullOrWhiteSpace(text)) {
                    return;
                }

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
                        Monitors.Add(monitor);
                        ParseLine(line, monitor);
                    }
                    else if (monitor != null) {
                        ParseLine(line, monitor);
                    }
                }
            }
        }
    }
}

