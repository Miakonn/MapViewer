using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Shapes;

// ReSharper disable once CheckNamespace
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

        public static void RemoveAllSymbolsFromOverlay(this Canvas canvas) {
            Debug.WriteLine("RemoveAllSymbolsFromOverlay!!");
            var symbols = canvas.Children.Cast<UIElement>().Where(elem => elem.Uid.StartsWith("Symbol_")).ToList();
            foreach (var elem in symbols) {
                 canvas.Children.Remove(elem);
            }
        }

        public static UIElement FindElementHit(this Canvas canvas) {
            return canvas.Children.Cast<UIElement>().FirstOrDefault(child => child.IsMouseOver);
        }

        public static UIElement FindElementByUid(this Canvas canvas, string uid) {
            return canvas.Children.Cast<UIElement>().FirstOrDefault(child => child.Uid == uid);
        }

        public static IEnumerable<Ellipse> FindPlayerElements(this Canvas canvas) {
            return canvas.Children.Cast<UIElement>().Where(child => child.IsPlayer()).Cast<Ellipse>();
        }

        public static bool IsPlayer(this UIElement elem) {
            return elem.Uid.StartsWith("Player") && elem is Ellipse;
        }

        public static TextBlock GetPlayerNameElement(this Canvas canvas, UIElement elem) {
            return canvas.FindElementByUid(elem.Uid + ".name") as TextBlock;
        }

        public static UIElement GetPlayerParentElement(this Canvas canvas, UIElement elemText)
        {
            var playerUid = elemText.Uid.Replace(".name", "");
            return canvas.FindElementByUid(playerUid);
        }

        public static Size GetTextSize(this Canvas canvas, TextBlock textBlock) {
            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            return textBlock.DesiredSize;

            //textBlock.Arrange(new Rect(textBlock.DesiredSize));

            //Debug.WriteLine(textBlock.ActualWidth); // prints 80.323333333333
            //Debug.WriteLine(textBlock.ActualHeight); // prints 15.96
            //return new Size(textBlock.ActualWidth, textBlock.ActualHeight);
        }

    }
}