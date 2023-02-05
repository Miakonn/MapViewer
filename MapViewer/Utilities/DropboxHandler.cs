using System;
using System.IO;

namespace MapViewer.Utilities {
    static class DropboxHandler { 
        private static string _workingPath = string.Empty;

        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType);

        private const string Dropbox = "Dropbox";

        public static void AddWorkingPath(string path) {
            if (string.IsNullOrEmpty(_workingPath)) {
                var indexDropbox = path.IndexOf(Dropbox, StringComparison.Ordinal);
                if (indexDropbox > 0) {
                    _workingPath = path.Substring(0, indexDropbox);
                    Log.Debug($"Dropbox-path='{_workingPath}'");
                }
            }
        }
 
        public static string TryFixingPath(string path) {
            if (string.IsNullOrWhiteSpace(_workingPath)) {
                return string.Empty;
            }

            var index = path.IndexOf(Dropbox, StringComparison.Ordinal);
            if (index < 0) {
                return string.Empty;
            }

            var newPath = _workingPath + path.Substring(index);
            return File.Exists(newPath) ? newPath : string.Empty;
        }

    }
}
