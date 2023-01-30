using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.Ribbon;
using System.Windows.Forms;
using MapViewer.Dialogs;
using MapViewer.Maps;
using MapViewer.Properties;
using MapViewer.Symbols;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Size = System.Windows.Size;

namespace MapViewer {
    public partial class PrivateWindow {

		private static bool CheckToggleState(object source) {
            if (source is RibbonToggleButton button) {
				return (button.IsChecked.HasValue && button.IsChecked.Value);
			}
			return true;
		}

		#region Can execute

		private void ImageNeeded_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = MapPrivate != null;
		}
        private void MaskNeeded_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = MapPrivate != null && MapPrivate.BmpMask != null;
        }

        private void ImagesNeeded_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = (MapPrivate != null && LevelNumber > 1);
        }

        private void CalibratedImageNeeded_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = (MapPrivate != null && MapPrivate.IsCalibrated);
        }

        private void Always_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = true;
		}

		private void Spell_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = (MapPrivate != null && MapPrivate.ImageScaleMperPix > 0.0);
		}

        private void Player_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = (MapPrivate != null && MapPrivate.ImageScaleMperPix > 0.0);
        }

        public bool Publish_CanExecute() {
			return MapPrivate != null && MapPublic != null && PublicWindow.IsVisible;
		}

        public void LevelAndPublish_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = (MapPrivate != null && LevelNumber > 1) && Publish_CanExecute();
        }

        public void Publish_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = Publish_CanExecute();
		}

		private void AddDisplay_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = (MapPrivate != null && !string.IsNullOrWhiteSpace(MapPrivate.ImageFilePath)) && 
				!PublicWindow.IsVisible && PublicWindow.IsCalibrated;
		}

		private void RemoveDisplay_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = PublicWindow.IsVisible;
		}

		private void RotateMap_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = (MapPrivate != null && !MapPrivate.IsLinked);
		}

		private void SymbolNeeded_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = (_lastClickedSymbol != null);
		}

        private void IconNeeded_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = (_lastClickedSymbol is SymbolIcon);
        }

        private void MoveSymbolUp_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = (MapPrivate != null 
                            && LevelNumber > 1
                            && Level < LevelNumber - 1
                            && _lastClickedSymbol != null);
        }

        private void MoveSymbolDown_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = (MapPrivate != null 
                            && LevelNumber > 1 
                            && Level > 0
                            && _lastClickedSymbol != null);
        }

        private void SelectSymbols_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = (MapPrivate != null && MapPrivate.SymbolsPM.SymbolsExists);
        }

        public void OpenLastImage_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            InitSettings();
            try {
                e.CanExecute = (!string.IsNullOrWhiteSpace(Settings.Default.MRU));
                Mru1.Visibility = e.CanExecute ? Visibility.Visible : Visibility.Collapsed;
                Mru1.Header = e.CanExecute ? ExtractFileName : string.Empty;
            }
            catch (Exception) {
                e.CanExecute = false;
            }
        }


        private void SymbolsChanged_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = MapPrivate.SymbolsPM.CanUndo;
        }

        #endregion

        #region Assorted

        private string _lastImageDirectoryUsed;

        private void OpenImage_Execute(object sender, ExecutedRoutedEventArgs e) {
			HideWarning(0);

            var dialog = new OpenFileDialog {
                Filter = "Image Files|*.jpg;*.bmp;*.png",
                Multiselect = true
            };

            if (!string.IsNullOrWhiteSpace(_lastImageDirectoryUsed)) {
                dialog.InitialDirectory = _lastImageDirectoryUsed;
            }

            Array.Sort(dialog.FileNames);

            var result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK) {
                return;
            }

            AddCurrentFilesToMru();
            LoadFiles(dialog.FileNames);
            _lastImageDirectoryUsed = Path.GetDirectoryName(dialog.FileName);
        }

		private void OpenLastImage_Execute(object sender, ExecutedRoutedEventArgs e) {
			if (Settings.Default.MRU == null) {
				return;
			}

            ActiveTool = null;
            var filesToOpen = MruFileNames;
            AddCurrentFilesToMru();
            LoadFiles(filesToOpen);
        }

        private void LoadFiles(string[] fileNames) {
            ActiveTool = null;
            Save_Execute(null, null);
            MapList.Clear();
            long groupId = DateTime.Now.Ticks;

            foreach (var filename in fileNames) {
                if (!File.Exists(filename)) {
                    throw new Exception($"Map {filename} is missing!");
                }
                var map = new PrivateMaskedMap( this, groupId);
                map.LoadImage(filename);
                MapList.Add(map);
            }



            Level = 0;
            MapPrivate = MapList[Level];
            Layer1_Map.Content = MapPrivate.CanvasMap;
            Layer2_Mask.Content = MapPrivate.CanvasMask;
            Layer3_Overlay.Content = MapPrivate.CanvasOverlay;

            MapPrivate.ImageScaleChanged += HandleImageScaleChanged;
            MapPrivate.ZoomChanged += HandleZoomChanged;
            MapPrivate.ScaleToWindow(Layer1_Map);
            HandleImageScaleChanged(null, null);

            MapPrivate.Create();

            SetScale(MapPrivate.MapData.LastFigureScaleUsed);
            if (MapPrivate.IsCalibrated) {
                GamingTab.IsSelected = true;
            }
            else {
                SetupTab.IsSelected = true;
            }
        }

        private void SwitchToNewMap(int newLevel) {
            ActiveTool = null;
            Level = newLevel;

            var oldMap = MapPrivate;
            MapPrivate = MapList[Level];

            if (oldMap != null && oldMap != MapPrivate) {
                MapPrivate.MapData.Copy(oldMap.MapData);
                MapPrivate.CopyTransform(oldMap);
                MapPrivate.IsLinked = oldMap.IsLinked;
                MapPrivate.ZoomChanged -= HandleZoomChanged;
            }

            if (!MapPrivate.Initiated) {
                MapPrivate.Create();
            }

            Layer1_Map.Content = MapPrivate.CanvasMap;
            Layer2_Mask.Content = MapPrivate.CanvasMask;
            Layer3_Overlay.Content = MapPrivate.CanvasOverlay;
            MapPrivate.ZoomChanged += HandleZoomChanged;
        }

        private void ExitApp_Execute(object sender, ExecutedRoutedEventArgs e) {
            Save_Execute(null, null);
            AddCurrentFilesToMru();
			Log.Info("Exiting");
			Application.Current.Shutdown();
		}

        string[] MruFileNames => Settings.Default.MRU?.Split(separator: new[] { ';' }, options: StringSplitOptions.RemoveEmptyEntries);

        private string ExtractFileName {
            get {
                var fileNames = MruFileNames;
                if (fileNames == null || fileNames.Length == 0) {
                    return null;
                }

                if (fileNames.Length == 1) {
                    return Path.GetFileName(fileNames[0]);
                }

                var fileName0 = Path.GetFileName(fileNames[0]);
                var fileName1 = Path.GetFileName(fileNames[1]);
                int s;
                for (s = 0; s < fileName0.Length; s++) {
                    if (fileName0[s] == fileName1[s]) {
                        break;
                    }
                }

                int t;
                for (t = s + 1; t < fileName0.Length; t++) {
                    if (fileName0[t] != fileName1[t]) {
                        break;
                    }
                }

                int e;
                for (e = fileName0.Length - 1; e > s ; e--) {
                    if (fileName0[e] != fileName1[e]) {
                        break;
                    }
                }

                return fileName0.Substring(s, t - s) + "*" + fileName0.Substring(e + 1, fileName0.Length - e - 1);
            }
        }

        public object HasSymbolCross {
            get {
                if (_lastClickedSymbol is SymbolIcon symbolIcon) {
                    return symbolIcon.Status == "Cross";
                }

                return false;
            }
        }

        private void ScaleToFit_Execute(object sender, ExecutedRoutedEventArgs e) {
			MapPrivate.ScaleToWindow(Layer1_Map);
			MapPrivate.UpdateVisibleRectangle(MapPublic);
		}

		private void ZoomIn_Execute(object sender, ExecutedRoutedEventArgs e) {
			MapPrivate.Zoom(1.2, _mouseDownPoint);
			MapPrivate.UpdateVisibleRectangle(MapPublic);
		}

		private void ZoomOut_Execute(object sender, ExecutedRoutedEventArgs e) {
			MapPrivate.Zoom(0.8, _mouseDownPoint);
			MapPrivate.UpdateVisibleRectangle(MapPublic);
		}

		private void Save_Execute(object sender, EventArgs e) {
            foreach (var map in MapList) {
                map.Serialize();
            }
		}
        
        private string _lastSymbolDirectoryUsed;
        private string SaveSymbolFileName() {
            var dialog = new SaveFileDialog {
                Title = "Select Symbol file",
                OverwritePrompt = true,
                DefaultExt = "xml",
                Filter = "Symbol (*.xml)|*.xml|All files (*.*)|*.*"
            };

            if (!string.IsNullOrWhiteSpace(_lastSymbolDirectoryUsed)) {
                dialog.InitialDirectory = _lastSymbolDirectoryUsed;
            }
            var result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK) {
                return string.Empty;
            }
            _lastSymbolDirectoryUsed = Path.GetDirectoryName(dialog.FileName);
            return dialog.FileName;
        }
        
        private void SaveSymbolAs_Execute(object sender, ExecutedRoutedEventArgs e) {
            var filename = SaveSymbolFileName();
            if (!string.IsNullOrWhiteSpace(filename)) {
                MapPrivate.SymbolsPM.Serialize(filename);
            }
        }
        
        private string OpenSymbolFile() {
            var dialog = new OpenFileDialog {
                Title = "Select Symbol file",
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = "xml",
                Filter = "Symbol (*.xml)|*.xml|All files (*.*)|*.*"
            };

            if (!string.IsNullOrWhiteSpace(_lastSymbolDirectoryUsed)) {
                dialog.InitialDirectory = _lastSymbolDirectoryUsed;
            }
            var result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK) {
                return string.Empty;
            }
            _lastSymbolDirectoryUsed = Path.GetDirectoryName(dialog.FileName);

            return dialog.FileName;
        }
        private void LoadSymbols_Execute(object sender, ExecutedRoutedEventArgs e) {
            var filename = OpenSymbolFile();
            if (!string.IsNullOrWhiteSpace(filename)) {

                var imSize = new Size(MapPrivate.MapImage.Width, MapPrivate.MapImage.Height);

                MapPrivate.SymbolsPM.Deserialize(filename, imSize);

            }
        }


        private void LevelUp_Execute(object sender, ExecutedRoutedEventArgs e) {
            SwitchToNewMap(Level + 1);
        }

        private void LevelUpPublish_Execute(object sender, ExecutedRoutedEventArgs e) {
            LevelUp_Execute(sender, e);
            PublishMap_Execute(sender, e);
        }

        private void LevelDown_Execute(object sender, ExecutedRoutedEventArgs e) {
            SwitchToNewMap(Level - 1);
        }

        private void LevelDownPublish_Execute(object sender, ExecutedRoutedEventArgs e) {
            LevelDown_Execute(sender, e);
            PublishMap_Execute(sender, e);
        }

        private void CalibrateDisplay_Execute(object sender, ExecutedRoutedEventArgs e) {
			var dialog = new DialogCalibrateDisplay {
				MonitorSize = PublicWindow.MonitorSize,
				MonitorResolution = PublicWindow.MonitorResolution,
				Owner = this
			};

			var result = dialog.ShowDialog();
			if (!result.HasValue || !result.Value) {
				return;
			}
			PublicWindow.MonitorSize = dialog.MonitorSize;
			PublicWindow.MonitorResolution = dialog.MonitorResolution;
        }

		private void PublishMap_Execute(object sender, ExecutedRoutedEventArgs e) {
            if (MapPrivate == null || MapPublic == null || !PublicWindow.IsVisible) {
                return;
            }

            MapPrivate.Serialize();
			MapPublic.PublishFrom(MapPrivate);
			PublicWindow.Map.DrawRuler(PublicWindow.ActualWidth, PublicWindow.ActualHeight);
			PublicWindow.Map.DrawCompass(PublicWindow.ActualWidth, PublicWindow.ActualHeight);
            PublicWindow.MaximizeToCurrentMonitor();

            MapPrivate.UpdateVisibleRectangle(MapPublic);

            ActiveTool = null;
			Activate();
		}

        private void FullMask_Execute(object sender, ExecutedRoutedEventArgs e) {
            var result = MessageBox.Show("Are you sure you want to mask everything?", "", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes) {
                MapPrivate.MaskAll();
            }
        }

        private void ClearMask_Execute(object sender, ExecutedRoutedEventArgs e) {
			var result =MessageBox.Show("Are you sure you want to clear the mask?" , "", MessageBoxButton.YesNo);
			if (result == MessageBoxResult.Yes) {
                MapPrivate.ClearMaskAll();
			}
		}

		private void ClearSymbols_Execute(object sender, ExecutedRoutedEventArgs e) {
            var result = MessageBox.Show("Are you sure you want to clear the overlay?", "", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) {
                return;
            }
            MapPrivate.CanvasOverlay.Clear();
            MapPrivate.SymbolsPM.DeleteAllSymbols();
            MapPrivate.UpdateVisibleRectangle(MapPublic);
            MapPublic.CanvasOverlay.Clear();
        }

		private void DeleteSymbol_Execute(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			if (_lastClickedSymbol == null) {
				return;
			}
			
            MapPrivate.SymbolsPM.DeleteSymbol(_lastClickedSymbol);
		}

        private void DuplicateSymbol_Execute(object sender, ExecutedRoutedEventArgs e) {
            ActiveTool = null;
            if (_lastClickedSymbol == null) {
                return;
            }

            var offset = new Vector(50 / MapPrivate.ZoomScale, 50 / MapPrivate.ZoomScale);
            MapPrivate.SymbolsPM.DuplicateSymbol(_lastClickedSymbol, offset);
            _lastClickedSymbol = MapPrivate.SymbolsPM.GetSelected();
        }

        private void SetSymbolColor_Execute(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			if (_lastClickedSymbol == null) {
				return;
			}

			var dialog = new DialogColorPicker {Owner = this};
			var result = dialog.ShowDialog();
			if (!result.HasValue || !result.Value) {
				return;
			}
            
            MapPrivate.SymbolsPM.SetSymbolColor(_lastClickedSymbol, dialog.SelectedColor);
        }

        private void SendSymbolToFront_Execute(object sender, ExecutedRoutedEventArgs e) {
            ActiveTool = null;
            if (_lastClickedSymbol == null) {
                return;
            }

            MapPrivate.SymbolsPM.MoveSymbolToFront(_lastClickedSymbol);
        }

        private void SendSymbolToBack_Execute(object sender, ExecutedRoutedEventArgs e) {
            ActiveTool = null;
            if (_lastClickedSymbol == null) {
                return;
            }
 
            MapPrivate.SymbolsPM.MoveSymbolToBack(_lastClickedSymbol);
        }
        
        private void MoveSymbolUp_Execute(object sender, ExecutedRoutedEventArgs e) {
            ActiveTool = null;
            if (_lastClickedSymbol == null) {
                return;
            }
            MapPrivate.SymbolsPM.MoveSymbolUpDown(_lastClickedSymbol, MapAbove.SymbolsPM);
        }

        private void MoveSymbolDown_Execute(object sender, ExecutedRoutedEventArgs e) {
            ActiveTool = null;
            if (_lastClickedSymbol == null) {
                return;
            }
            MapPrivate.SymbolsPM.MoveSymbolUpDown(_lastClickedSymbol, MapBelow.SymbolsPM);
        }

        private void SelectSymbols_Execute(object sender, ExecutedRoutedEventArgs e) {
            if (CheckToggleState(e.OriginalSource)) {
                var tool = new Tools.SelectSymbols(this, e.OriginalSource, true);
                ActiveTool = tool;
            }
        }

        private void RotateMap_Execute(object sender, ExecutedRoutedEventArgs e) {
			PublicWindow.RotateClockwise();
			MapPrivate.UpdateVisibleRectangle(MapPublic);
        }

		private void AddDisplay_Execute(object sender, ExecutedRoutedEventArgs e) {
			PublicWindow.Show();
			PublicWindow.PlaceOnSelectedMonitor();
			PublishMap_Execute(sender, e);
		}

		private void RemoveDisplay_Execute(object sender, ExecutedRoutedEventArgs e) {
			PublicWindow.Visibility = Visibility.Hidden;
		}
		#endregion

		#region Tools
        private void Measure_Execute(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			if (CheckToggleState(e.OriginalSource)) {
				ActiveTool = new Tools.Measure(this, e.OriginalSource);
			}
		}

		private void Calibrate_Execute(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			if (CheckToggleState(e.OriginalSource)) {
				ActiveTool = new Tools.Calibrate(this, e.OriginalSource);
			}
		}

		private void MaskRectangle_Execute(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			if (CheckToggleState(e.OriginalSource)) {
				var tool = new Tools.MaskRectangle(this, e.OriginalSource, true);
				ActiveTool = tool;
			}
		}

		private void UnmaskRectangle_Execute(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			if (CheckToggleState(e.OriginalSource)) {
				var tool = new Tools.MaskRectangle(this, e.OriginalSource, false);
				ActiveTool = tool;
			}
		}

		private void MaskCircle_Execute(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			if (CheckToggleState(e.OriginalSource)) {
				ActiveTool = new Tools.MaskCircle(this, e.OriginalSource, true);
			}
		}

		private void UnmaskCircle_Execute(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			if (CheckToggleState(e.OriginalSource)) {
				ActiveTool = new Tools.MaskCircle(this, e.OriginalSource, false);
			}
		}

		private void MaskPolygon_Execute(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			if (CheckToggleState(e.OriginalSource)) {
				var tool = new Tools.MaskPolygon(this, e.OriginalSource, true);
				ActiveTool = tool;
			}
		}

		private void UnmaskPolygon_Execute(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			if (CheckToggleState(e.OriginalSource)) {
				var tool = new Tools.MaskPolygon(this, e.OriginalSource, false);
				ActiveTool = tool;
			}
		}

		#endregion

		#region Spells
		private void DrawCircle_Execute(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			if (CheckToggleState(e.OriginalSource)) {
				ActiveTool = new Tools.DrawCircle(this, e.OriginalSource);
			}
		}

		private void DrawCone_Execute(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			if (CheckToggleState(e.OriginalSource)) {
				ActiveTool = new Tools.DrawCone(this, e.OriginalSource);
			}
		}

		private void DrawLine_Execute(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			if (CheckToggleState(e.OriginalSource)) {
				ActiveTool = new Tools.DrawLine(this, e.OriginalSource);
			}
		}

		private void DrawRectangle_Execute(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			if (CheckToggleState(e.OriginalSource)) {
				ActiveTool = new Tools.DrawRectangle(this, e.OriginalSource);
			}
		}

		private void DrawText_Execute(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			if (CheckToggleState(e.OriginalSource)) {
				ActiveTool = new Tools.DrawText(this, e.OriginalSource);
			}
		}

		private void SpellCircular7m_Execute(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
            MapPrivate.SymbolsPM.CreateSymbolCircle(_mouseDownPoint, Colors.OrangeRed, 7);
        }

		private void SpellCircular3m_Execute(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
            MapPrivate.SymbolsPM.CreateSymbolCircle(_mouseDownPoint, Colors.LightSkyBlue, 3);
		}

		private void SpellCircular2m_Execute(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
            MapPrivate.SymbolsPM.CreateSymbolCircle(_mouseDownPoint, Colors.Yellow, 2);
		}

        private void CreateCreature(Color color, double size) {
            var symbol = MapPrivate.SymbolsPM.CreateSymbolCreature(_mouseDownPoint, color, size, "");

            var posDialog = ScaleWithWindowsDpi(PointToScreen(_mouseDownPointWindow));
            var result = MapPrivate.SymbolsPM.OpenEditor(this, posDialog, symbol);
            if (!result) {
                MapPrivate.SymbolsPM.DeleteSymbol(symbol);
            }
        }
        
        private void Player_Execute(object sender, ExecutedRoutedEventArgs e) {
            CreateCreature(Colors.LightBlue, 0.8);
        }

        private void NonPlayer_Execute(object sender, ExecutedRoutedEventArgs e) {
            CreateCreature(Colors.Orange, 0.8);
        }
		
        private void SymbolIcon_Execute(object sender, ExecutedRoutedEventArgs e) {
            var symbol = MapPrivate.SymbolsPM.CreateSymbolIcon(_mouseDownPoint);
            var posDialog = ScaleWithWindowsDpi(PointToScreen(_mouseDownPointWindow));
            var result = MapPrivate.SymbolsPM.OpenEditor(this, posDialog, symbol);
            if (!result) {
                MapPrivate.SymbolsPM.DeleteSymbol(symbol);
            }
        }

        private void ShowPublicCursorTemporary_Execute(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			MapPublic.ShowPublicCursorTemporary = true;
			MapPublic.ShowPublicCursor = true;
		}

		private void UnmaskLineOfSight20m_Execute(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			var radius = 20 / MapPrivate.ImageScaleMperPix;
			MapPrivate.UnmaskLineOfSight(_mouseDownPoint, radius);
		}
		#endregion

		#region Other

		private void CheckBoxPublicCursor_OnChecked(object sender, RoutedEventArgs e) {
			ActiveTool = null;
			MapPublic.ShowPublicCursor = true;
		}

		private void CheckBoxPublicCursor_OnUnchecked(object sender, RoutedEventArgs e) {
			ActiveTool = null;
			MapPublic.ShowPublicCursor = false;
		}

        #endregion

        private void RotateSymbolCW_Execute(object sender, ExecutedRoutedEventArgs e) {
            ActiveTool = null;
            if (_lastClickedSymbol == null) {
                return;
            }

            MapPrivate.SymbolsPM.RotateSymbol(_lastClickedSymbol, RotationDirection.Clockwise);
        }

        private void RotateSymbolCCW_Execute(object sender, ExecutedRoutedEventArgs e) {
            ActiveTool = null;
            if (_lastClickedSymbol == null) {
                return;
            }

            MapPrivate.SymbolsPM.RotateSymbol(_lastClickedSymbol, RotationDirection.CounterClockwise);
        }

        private void SetSymbolCross_Execute(object sender, ExecutedRoutedEventArgs e) {
            MapPrivate.SymbolsPM.ToggleSymbolStatus(_lastClickedSymbol, "Cross");
        }

        private void UndoSymbols_Execute(object sender, ExecutedRoutedEventArgs e) {
            MapPrivate.SymbolsPM.Undo();
        }

    }
}
