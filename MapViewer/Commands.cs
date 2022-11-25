using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using System.Windows.Controls.Ribbon;
using MapViewer.Dialogs;
using MapViewer.Maps;
using MapViewer.Properties;
using MapViewer.Utilities;
using System.Windows.Media.Animation;

namespace MapViewer {
	public static class CustomCommands {
        public static readonly RoutedUICommand Player = new RoutedUICommand("Player", "Player", typeof(CustomCommands), null);
        public static readonly RoutedUICommand NonPlayer = new RoutedUICommand("NonPlayer", "NonPlayer", typeof(CustomCommands), null);
        public static readonly RoutedUICommand DuplicateSymbol = new RoutedUICommand("Duplicate", "Duplicate", typeof(CustomCommands), null);
        public static readonly RoutedUICommand SymbolImage = new RoutedUICommand("Load Image", "Load Image", typeof(CustomCommands), null);
        public static readonly RoutedUICommand SpellCircular7m = new RoutedUICommand("Spell r=7m", "Spell r=7m", typeof(CustomCommands), null);
		public static readonly RoutedUICommand SpellCircular3m = new RoutedUICommand("Spell r=3m", "Spell r=3m", typeof(CustomCommands), null);
		public static readonly RoutedUICommand SpellCircular2m = new RoutedUICommand("Spell r=2m", "Spell r=2m", typeof(CustomCommands), null);
		public static readonly RoutedUICommand DeleteElement = new RoutedUICommand("Delete", "Delete", typeof(CustomCommands), null);
		public static readonly RoutedUICommand SetColorElement = new RoutedUICommand("Set Colour", "Set Colour", typeof(CustomCommands), null);
        public static readonly RoutedUICommand SendSymbolToFront = new RoutedUICommand("Bring to Front", "Bring to Front", typeof(CustomCommands), null);
        public static readonly RoutedUICommand SendSymbolToBack = new RoutedUICommand("Send to Back", "Send to Back", typeof(CustomCommands), null);
        public static readonly RoutedUICommand MoveElementUp = new RoutedUICommand("Move Up", "Move Up", typeof(CustomCommands), null);
        public static readonly RoutedUICommand MoveElementDown = new RoutedUICommand("Move Down", "Move Down", typeof(CustomCommands), null);
		public static readonly RoutedUICommand FullMask = new RoutedUICommand("Full Mask", "Full Mask", typeof(CustomCommands), null);
		public static readonly RoutedUICommand ShowPublicCursorTemporary = new RoutedUICommand("Show Cursor Temp.", "Show Cursor Temp.", typeof(CustomCommands), null);

		public static readonly RoutedUICommand OpenImage = new RoutedUICommand("Open Image", "Open Image", typeof(CustomCommands), null);
		public static readonly RoutedUICommand ExitApp = new RoutedUICommand("Exit", "Exit", typeof(CustomCommands), null);
		public static readonly RoutedUICommand PublishMap = new RoutedUICommand("Publish", "Publish", typeof(CustomCommands), null);
		public static readonly RoutedUICommand ClearMask = new RoutedUICommand("Clear Mask", "Clear Mask", typeof(CustomCommands), null);
		public static readonly RoutedUICommand ClearOverlay = new RoutedUICommand("Clear Overlay", "Clear Overlay", typeof(CustomCommands), null);
		public static readonly RoutedUICommand ScaleToFit= new RoutedUICommand("Scale to Fit", "Scale to Fit", typeof(CustomCommands), null);
		public static readonly RoutedUICommand ZoomIn = new RoutedUICommand("Zoom In", "Zoom In", typeof(CustomCommands), null);
		public static readonly RoutedUICommand ZoomOut = new RoutedUICommand("Zoom Out", "Zoom Out", typeof(CustomCommands), null);
        public static readonly RoutedUICommand LevelDown = new RoutedUICommand("Level Down", "Level Down", typeof(CustomCommands), null);
        public static readonly RoutedUICommand LevelUp = new RoutedUICommand("Level Up", "Level Up", typeof(CustomCommands), null);
        public static readonly RoutedUICommand LevelDownPublish = new RoutedUICommand("Level Down Publish", "Level Down Publish", typeof(CustomCommands), null);
        public static readonly RoutedUICommand LevelUpPublish = new RoutedUICommand("Level Up publish", "Level Up publish", typeof(CustomCommands), null);

        public static readonly RoutedUICommand RotateMap = new RoutedUICommand("Rotate Map", "Rotate Map", typeof(CustomCommands), null);
		public static readonly RoutedUICommand AddDisplay = new RoutedUICommand("Add Display", "Add Display", typeof(CustomCommands), null);
		public static readonly RoutedUICommand RemoveDisplay = new RoutedUICommand("Remove Display", "Remove Display", typeof(CustomCommands), null);

		public static readonly RoutedUICommand Save = new RoutedUICommand("Save", "Save", typeof(CustomCommands), null);
		public static readonly RoutedUICommand CalibrateDisplay = new RoutedUICommand("Calibrate Display", "Calibrate Display", typeof(CustomCommands), null);
		public static readonly RoutedUICommand OpenLastImage = new RoutedUICommand("Open Last Image", "Open Last Image", typeof(CustomCommands), null);

		public static readonly RoutedUICommand Calibrate = new RoutedUICommand("Calibrate", "Calibrate", typeof(CustomCommands), null);
		public static readonly RoutedUICommand Measure = new RoutedUICommand("Measure", "Measure", typeof(CustomCommands), null);
		public static readonly RoutedUICommand DrawLine = new RoutedUICommand("Draw Line", "Draw Line", typeof(CustomCommands), null);
		public static readonly RoutedUICommand DrawCone = new RoutedUICommand("Draw Cone", "Draw Cone", typeof(CustomCommands), null);
		public static readonly RoutedUICommand DrawCircle = new RoutedUICommand("Draw Circle", "Draw Circle", typeof(CustomCommands), null);
		public static readonly RoutedUICommand DrawRectangle = new RoutedUICommand("Draw Rectangle", "Draw Rectangle", typeof(CustomCommands), null);
		public static readonly RoutedUICommand DrawText = new RoutedUICommand("Draw Text", "Draw Text", typeof(CustomCommands), null);

		public static readonly RoutedUICommand MaskRectangle = new RoutedUICommand("Mask Rectangle", "Mask Rectangle", typeof(CustomCommands), null);
		public static readonly RoutedUICommand UnmaskRectangle = new RoutedUICommand("Unmask Rectangle", "Unmask Rectangle", typeof(CustomCommands), null);
		public static readonly RoutedUICommand MaskCircle = new RoutedUICommand("Mask Circle", "Mask Circle", typeof(CustomCommands), null);
		public static readonly RoutedUICommand UnmaskCircle = new RoutedUICommand("Unmask Circle", "Unmask Circle", typeof(CustomCommands), null);
		public static readonly RoutedUICommand MaskPolygon = new RoutedUICommand("Mask Polygon", "Mask Polygon", typeof(CustomCommands), null);
		public static readonly RoutedUICommand UnmaskPolygon = new RoutedUICommand("Unmask Polygon", "Unmask Polygon", typeof(CustomCommands), null);
		public static readonly RoutedUICommand UnmaskLineOfSight20m = new RoutedUICommand("Unmask LOS 20m", "Unmask LOS 20m", typeof(CustomCommands), null);
	}

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

        private void ImagesNeeded_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = (MapPrivate != null && LevelNumber > 1);
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

		private void ElementNeeded_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = (_lastClickedElem != null);
		}

        private void MoveElementUp_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = (MapPrivate != null 
                            && LevelNumber > 1
                            && Level < LevelNumber - 1
                            && _lastClickedElem != null);
        }

        private void MoveElementDown_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = (MapPrivate != null 
                            && LevelNumber > 1 
                            && Level > 0
                            && _lastClickedElem != null);
        }


        public void OpenLastImage_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            InitSettings();
            try {
                e.CanExecute = (!string.IsNullOrWhiteSpace(Settings.Default.MRU));
                Mru1.Visibility = e.CanExecute ? Visibility.Visible : Visibility.Collapsed;
                Mru1.Header = e.CanExecute ? ExtractFileName : String.Empty;
            }
            catch (Exception) {
                e.CanExecute = false;
            }
        }
        #endregion

        #region Assorted
        private void OpenImage_Execute(object sender, ExecutedRoutedEventArgs e) {
            var dialog = new OpenFileDialog {
                Filter = "Image Files|*.jpg;*.bmp;*.png",
                Multiselect = true
            };

            Array.Sort(dialog.FileNames);

            var result = dialog.ShowDialog();
            if (result == null || !result.Value) {
                return;
            }

            AddCurrentFilesToMru();
            LoadFiles(dialog.FileNames);
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
                var map = new PrivateMaskedMap( this, groupId);
                map.LoadImage(filename);
                MapList.Add(map);
            }

            Level = 0;
            MapPrivate = MapList[Level];
            LayerMap.Content = MapPrivate.CanvasMap;
            LayerMask.Content = MapPrivate.CanvasMask;
            LayerOverlay.Content = MapPrivate.CanvasOverlay;

            MapPrivate.ImageScaleChanged += HandleImageScaleChanged;
            MapPrivate.ScaleToWindow(LayerMap);
            HandleImageScaleChanged(null, null);

            PublicNeedsRescaling = true;
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
            }

            if (!MapPrivate.Initiated) {
                MapPrivate.Create();
            }

            LayerMap.Content = MapPrivate.CanvasMap;
            LayerMask.Content = MapPrivate.CanvasMask;
            LayerOverlay.Content = MapPrivate.CanvasOverlay;
            PublicNeedsRescaling = false;
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

		private void ScaleToFit_Execute(object sender, ExecutedRoutedEventArgs e) {
			MapPrivate.ScaleToWindow(LayerMap);
			if (!MapPrivate.IsLinked) {
				MapPrivate.UpdateVisibleRectangle(MapPublic.VisibleRectInMap());
			}
		}

		private void ZoomIn_Execute(object sender, ExecutedRoutedEventArgs e) {
			MapPrivate.Zoom(1.2, new Point(0,0));
			if (!MapPrivate.IsLinked) {
				MapPrivate.UpdateVisibleRectangle(MapPublic.VisibleRectInMap());
			}
		}

		private void ZoomOut_Execute(object sender, ExecutedRoutedEventArgs e) {
			MapPrivate.Zoom(0.8, new Point(0, 0));
			if (!MapPrivate.IsLinked) {
				MapPrivate.UpdateVisibleRectangle(MapPublic.VisibleRectInMap());
			}
		}

		private void Save_Execute(object sender, EventArgs e) {
            foreach (var map in MapList) {
                map.Serialize();
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
            PublicNeedsRescaling = true;
        }

		private void PublishMap_Execute(object sender, ExecutedRoutedEventArgs e) {
			MapPrivate.Serialize();
			MapPublic.PublishFrom(MapPrivate, PublicNeedsRescaling);
			PublicWindow.SetRuler();
			PublicWindow.DrawCompass();
            PublicWindow.MaximizeToCurrentMonitor();
			PublicNeedsRescaling = false;

			if (MapPublic.IsLinked) {
				MapPrivate.RemoveElement(MaskedMap.PublicPositionUid);
			}
			else {
				MapPrivate.UpdateVisibleRectangle(MapPublic.VisibleRectInMap());
			}

			ActiveTool = null;
			Activate();
		}

		private void ClearMask_Execute(object sender, ExecutedRoutedEventArgs e) {
			var result =MessageBox.Show("Are you sure you want to clear the mask?" , "", MessageBoxButton.YesNo);
			if (result == MessageBoxResult.Yes) {
				MapPrivate.ClearMask();
			}
		}

		private void ClearOverlay_Execute(object sender, ExecutedRoutedEventArgs e) {
            var result = MessageBox.Show("Are you sure you want to clear the overlay?", "", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) {
                return;
            }
            MapPrivate.ClearOverlay();
            MapPrivate.SymbolsPM.DeleteAllSymbols();
            if (!MapPublic.IsLinked) {
                MapPrivate.UpdateVisibleRectangle(MapPublic.VisibleRectInMap());
            }

            MapPublic.ClearOverlay();
        }

		private void DeleteElement_Execute(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			if (_lastClickedElem == null || _lastClickedElem.Uid == MaskedMap.PublicPositionUid) {
				return;
			}
			
            MapPrivate.SymbolsPM.DeleteSymbol(_lastClickedElem.Uid);
		}



        private void DuplicateSymbol_Execute(object sender, ExecutedRoutedEventArgs e) {
            ActiveTool = null;
            if (_lastClickedElem == null || _lastClickedElem.Uid == MaskedMap.PublicPositionUid) {
                return;
            }
 
            MapPrivate.SymbolsPM.DuplicateSymbol(_lastClickedElem.Uid);
        }

        private void SetColorElement_Execute(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			if (_lastClickedElem == null || _lastClickedElem.Uid == MaskedMap.PublicPositionUid) {
				return;
			}

			var dialog = new DialogColorPicker {Owner = this};
			var result = dialog.ShowDialog();
			if (!result.HasValue || !result.Value) {
				return;
			}

			var uid = _lastClickedElem.Uid;
            MapPrivate.SymbolsPM.SetSymbolColor(uid, dialog.SelectedColor);

			_lastClickedElem.SetColor(dialog.SelectedBrush);
			if (PublicWindow.IsVisible) {
				var elemPublic = MapPublic.CanvasOverlay.FindElementByUid(uid);
                elemPublic?.SetColor(dialog.SelectedBrush);
            }
		}

        private void SendSymbolToFront_Execute(object sender, ExecutedRoutedEventArgs e) {
            ActiveTool = null;
            if (_lastClickedElem == null) {
                return;
            }

            MapPrivate.SymbolsPM.MoveSymbolToFront(_lastClickedElem.Uid);
        }

        private void SendSymbolToBack_Execute(object sender, ExecutedRoutedEventArgs e) {
            ActiveTool = null;
            if (_lastClickedElem == null) {
                return;
            }
 
            MapPrivate.SymbolsPM.MoveSymbolToBack(_lastClickedElem.Uid);
        }
        
        private void MoveElementUp_Execute(object sender, ExecutedRoutedEventArgs e) {
            ActiveTool = null;
            if (_lastClickedElem == null) {
                return;
            }
            MapPrivate.SymbolsPM.MoveSymbolUpDown(_lastClickedElem.Uid, MapAbove.SymbolsPM);
        }

        private void MoveElementDown_Execute(object sender, ExecutedRoutedEventArgs e) {
            ActiveTool = null;
            if (_lastClickedElem == null) {
                return;
            }
            MapPrivate.SymbolsPM.MoveSymbolUpDown(_lastClickedElem.Uid, MapBelow.SymbolsPM);
        }

        private void FullMask_Execute(object sender, ExecutedRoutedEventArgs e) {
			var result = MessageBox.Show("Are you sure you want to mask everything?", "", MessageBoxButton.YesNo);
			if (result == MessageBoxResult.Yes) {
				var rect = new Int32Rect(0, 0, (int)MapPrivate.MapImage.Width, (int)MapPrivate.MapImage.Height);
				MapPrivate.MaskRectangle(rect, 255);
			}
		}

		private void RotateMap_Execute(object sender, ExecutedRoutedEventArgs e) {
			PublicWindow.RotateClockwise();
			if (MapPublic.IsLinked) {
				MapPrivate.RemoveElement(MaskedMap.PublicPositionUid);
			}
			else {
				MapPrivate.UpdateVisibleRectangle(MapPublic.VisibleRectInMap());
			}
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

        private void CreateCreature(Color color, Point pos, double size) {
            ActiveTool = null;
            var dialog = new DialogGetSingleValue {
                LeadText = "Text",
                Owner = this
            };

            var result = dialog.ShowDialog();
            if (!result.HasValue || !result.Value) {
                return;
            }
            
            MapPrivate.SymbolsPM.CreateSymbolCreature(pos, color, size, dialog.TextValue);
        }

        private void CreateSymbolImage(Point pos) {
            var symbol = MapPrivate.SymbolsPM.CreateSymbolImage(pos);
			var result = symbol.OpenEditor(pos, MapPrivate.SymbolsPM);
            if (!result) {
                MapPrivate.SymbolsPM.DeleteSymbol(symbol.Uid);
            }
        }

        private void Player_Execute(object sender, ExecutedRoutedEventArgs e) {
            CreateCreature(Colors.LightBlue, _mouseDownPoint, 0.8);
        }

        private void NonPlayer_Execute(object sender, ExecutedRoutedEventArgs e) {
            CreateCreature(Colors.Orange, _mouseDownPoint, 0.8);
        }
		
        private void SymbolImage_Execute(object sender, ExecutedRoutedEventArgs e) {
            CreateSymbolImage(_mouseDownPoint);
        }

        private void ShowPublicCursorTemporary_Execute(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			MapPublic.ShowPublicCursorTemporary = true;
			MapPublic.ShowPublicCursor = true;
		}

		private void UnmaskLineOfSight20m_Execute(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			var radius = 20 / MapPrivate.ImageScaleMperPix;
			MapPrivate.MaskLineOfSight(_mouseDownPoint.X, _mouseDownPoint.Y, radius, 0);
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
	}
}
