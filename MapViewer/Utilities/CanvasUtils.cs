using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace MapViewer {
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

        public static UIElement CopyElement(UIElement elemSource) {
            var xaml = XamlWriter.Save(elemSource);
            if (XamlReader.Parse(xaml) is UIElement deepCopy) {
                return deepCopy;
            }
            return null;
        }

        public static void CopyingCanvas(this Canvas canvasSource, Canvas canvasDest) {
            canvasDest.Children.Clear();
            foreach (UIElement child in canvasSource.Children) {
                if (child.Uid != Maps.MaskedMap.PublicPositionUid) {
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