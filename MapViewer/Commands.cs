﻿using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using System.Windows.Controls.Ribbon;
using MapViewer.Dialogs;
using MapViewer.Properties;

namespace MapViewer {
	public static class CustomCommands {

        public static readonly RoutedUICommand Player = new RoutedUICommand("Player", "Player", typeof(CustomCommands), null);
        public static readonly RoutedUICommand NonPlayer = new RoutedUICommand("NonPlayer", "NonPlayer", typeof(CustomCommands), null);
        public static readonly RoutedUICommand SetText = new RoutedUICommand("Set text", "Set text", typeof(CustomCommands), null);
        public static readonly RoutedUICommand SpellCircular7m = new RoutedUICommand("Spell r=7m", "Spell r=7m", typeof(CustomCommands), null);
		public static readonly RoutedUICommand SpellCircular3m = new RoutedUICommand("Spell r=3m", "Spell r=3m", typeof(CustomCommands), null);
		public static readonly RoutedUICommand SpellCircular2m = new RoutedUICommand("Spell r=2m", "Spell r=2m", typeof(CustomCommands), null);
		public static readonly RoutedUICommand DeleteElement = new RoutedUICommand("Delete element", "Delete element", typeof(CustomCommands), null);
		public static readonly RoutedUICommand SetColorElement = new RoutedUICommand("Set colour", "Set colour", typeof(CustomCommands), null);
		public static readonly RoutedUICommand FullMask = new RoutedUICommand("Full mask", "Full mask", typeof(CustomCommands), null);
		public static readonly RoutedUICommand ShowPublicCursorTemporary = new RoutedUICommand("Show cursor temp.", "Show cursor temp.", typeof(CustomCommands), null);

		public static readonly RoutedUICommand OpenImage = new RoutedUICommand("Open image", "Open image", typeof(CustomCommands), null);
		public static readonly RoutedUICommand ExitApp = new RoutedUICommand("Exit", "Exit", typeof(CustomCommands), null);
		public static readonly RoutedUICommand PublishMap = new RoutedUICommand("Publish", "Publish", typeof(CustomCommands), null);
		public static readonly RoutedUICommand ClearMask = new RoutedUICommand("Clear mask", "Clear mask", typeof(CustomCommands), null);
		public static readonly RoutedUICommand ClearOverlay = new RoutedUICommand("Clear overlay", "Clear overlay", typeof(CustomCommands), null);
		public static readonly RoutedUICommand ScaleToFit= new RoutedUICommand("Scale to fit", "Scale to fit", typeof(CustomCommands), null);
		public static readonly RoutedUICommand ZoomIn = new RoutedUICommand("Zoom in", "Zoom in", typeof(CustomCommands), null);
		public static readonly RoutedUICommand ZoomOut = new RoutedUICommand("Zoom out", "Zoom out", typeof(CustomCommands), null);

		public static readonly RoutedUICommand RotateMap = new RoutedUICommand("Rotate map", "Rotate map", typeof(CustomCommands), null);
		public static readonly RoutedUICommand AddDisplay = new RoutedUICommand("Add display", "Add display", typeof(CustomCommands), null);
		public static readonly RoutedUICommand RemoveDisplay = new RoutedUICommand("Remove display", "Remove display", typeof(CustomCommands), null);

		public static readonly RoutedUICommand Save = new RoutedUICommand("Save", "Save", typeof(CustomCommands), null);
		public static readonly RoutedUICommand CalibrateDisplay = new RoutedUICommand("Calibrate display", "Calibrate display", typeof(CustomCommands), null);
		public static readonly RoutedUICommand OpenLastImage = new RoutedUICommand("Open last image", "Open last image", typeof(CustomCommands), null);

		public static readonly RoutedUICommand Calibrate = new RoutedUICommand("Calibrate", "Calibrate", typeof(CustomCommands), null);
		public static readonly RoutedUICommand Measure = new RoutedUICommand("Measure", "Measure", typeof(CustomCommands), null);
		public static readonly RoutedUICommand DrawLine = new RoutedUICommand("Draw line", "Draw line", typeof(CustomCommands), null);
		public static readonly RoutedUICommand DrawCone = new RoutedUICommand("Draw cone", "Draw cone", typeof(CustomCommands), null);
		public static readonly RoutedUICommand DrawCircle = new RoutedUICommand("Draw circle", "Draw circle", typeof(CustomCommands), null);
		public static readonly RoutedUICommand DrawRectangle = new RoutedUICommand("Draw rectangle", "Draw rectangle", typeof(CustomCommands), null);
		public static readonly RoutedUICommand DrawSquare = new RoutedUICommand("Draw square", "Draw square", typeof(CustomCommands), null);
		public static readonly RoutedUICommand DrawText = new RoutedUICommand("Draw text", "Draw text", typeof(CustomCommands), null);

		public static readonly RoutedUICommand MaskRectangle = new RoutedUICommand("Mask rectangle", "Mask rectangle", typeof(CustomCommands), null);
		public static readonly RoutedUICommand UnmaskRectangle = new RoutedUICommand("Unmask rectangle", "Unmask rectangle", typeof(CustomCommands), null);
		public static readonly RoutedUICommand MaskCircle = new RoutedUICommand("Mask circle", "Mask circle", typeof(CustomCommands), null);
		public static readonly RoutedUICommand UnmaskCircle = new RoutedUICommand("Unmask circle", "Unmask circle", typeof(CustomCommands), null);
		public static readonly RoutedUICommand MaskPolygon = new RoutedUICommand("Mask polygon", "Mask polygon", typeof(CustomCommands), null);
		public static readonly RoutedUICommand UnmaskPolygon = new RoutedUICommand("Unmask polygon", "Unmask polygon", typeof(CustomCommands), null);
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
			e.CanExecute = (MapPrivate != null && !string.IsNullOrWhiteSpace(MapPrivate.ImageFilePath));
		}

		private void Always_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = true;
		}

		private void Spell_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = (MapPrivate != null && !string.IsNullOrWhiteSpace(MapPrivate.ImageFilePath) && MapPrivate.ImageScaleMperPix > 0.0);
		}

        private void Player_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = (MapPrivate != null && !string.IsNullOrWhiteSpace(MapPrivate.ImageFilePath) && MapPrivate.ImageScaleMperPix > 0.0);
        }

        public bool Publish_CanExecute() {
			return (MapPrivate != null && !string.IsNullOrWhiteSpace(MapPrivate.ImageFilePath)) && PublicWindow.IsVisible;
		}

		public void OpenLastImage_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			InitSettings();
			try {
				e.CanExecute = (!string.IsNullOrWhiteSpace(Settings.Default.MRU));
				Mru1.Visibility = e.CanExecute ? Visibility.Visible : Visibility.Collapsed;
				Mru1.Header = e.CanExecute ? Path.GetFileName(Settings.Default.MRU) : "";
			}
			catch (Exception) {
				e.CanExecute = false;
			}
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
			e.CanExecute = (MapPrivate != null && !string.IsNullOrWhiteSpace(MapPrivate.ImageFilePath) && !MapPrivate.IsLinked);
		}

		private void ElementNeeded_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = (_lastClickedElem != null);
		}
		#endregion

		#region Assorted
		private void OpenImage_Execute(object sender, ExecutedRoutedEventArgs e) {
			var dialog = new OpenFileDialog {Filter = "Image Files|*.jpg;*.bmp;*.png"};
			var result = dialog.ShowDialog();
			if (result != null && result.Value) {
				MapPrivate.LoadImage(dialog.FileName);
				_publicIsDirty = true;
				CreateWindows();
				SetScale(MapPrivate.MapData.LastFigureScaleUsed);

				if (MapPrivate.IsCalibrated) {
					GamingTab.IsSelected = true;
				}
				else {
					SetupTab.IsSelected = true;
				}
			}
		}

		private void OpenLastImage_Execute(object sender, ExecutedRoutedEventArgs e) {
			if (Settings.Default.MRU == null) {
				return;
			}

			MapPrivate.LoadImage(Settings.Default.MRU);
			_publicIsDirty = true;
			CreateWindows();
			SetScale(MapPrivate.MapData.LastFigureScaleUsed);
			if (MapPrivate.IsCalibrated) {
				GamingTab.IsSelected = true;
			}
			else {
				SetupTab.IsSelected = true;
			}
			
		}

		private void ExitApp_Execute(object sender, ExecutedRoutedEventArgs e) {
			AddToMru(MapPrivate.ImageFilePath);
			Settings.Default.Save();
			Log.Info("Exiting");
			Application.Current.Shutdown();
		}

		private void ScaleToFit_Execute(object sender, ExecutedRoutedEventArgs e) {
			MapPrivate.ScaleToWindow();
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

		private void Save_Execute(object sender, ExecutedRoutedEventArgs e) {
			MapPrivate.Serialize();
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
			MapPrivate.Serialize(); 
			MapPublic.PublishFrom(MapPrivate, _publicIsDirty);
			PublicWindow.SetRuler();
			PublicWindow.DrawCompass();
			_publicIsDirty = false;

			if (MapPublic.IsLinked) {
				MapPrivate.DeleteShape(MaskedMap.PublicPositionUid);
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
			MapPrivate.ClearOverlay();
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

			var uid = _lastClickedElem.Uid;
			MapPrivate.CanvasOverlay.Children.Remove(_lastClickedElem);
			if (PublicWindow.IsVisible) {
				var elemPublic = MapPublic.CanvasOverlay.FindElementByUid(uid);
				if (elemPublic != null) {
					MapPublic.CanvasOverlay.Children.Remove(elemPublic);
				}
			}
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
			_lastClickedElem.SetColor(dialog.SelectedColor);
			if (PublicWindow.IsVisible) {
				var elemPublic = MapPublic.CanvasOverlay.FindElementByUid(uid);
                elemPublic?.SetColor(dialog.SelectedColor);
            }
		}

        private void SetText_Execute(object sender, ExecutedRoutedEventArgs e) {
            ActiveTool = null;
            if (_lastClickedElem == null || _lastClickedElem.Uid == MaskedMap.PublicPositionUid) {
                return;
            }

            var dialog = new DialogColorPicker { Owner = this };
            var result = dialog.ShowDialog();
            if (!result.HasValue || !result.Value) {
                return;
            }

            var uid = _lastClickedElem.Uid;
            _lastClickedElem.SetColor(dialog.SelectedColor);
            if (PublicWindow.IsVisible) {
                var elemPublic = MapPublic.CanvasOverlay.FindElementByUid(uid);
                elemPublic?.SetColor(dialog.SelectedColor);
            }
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
				MapPrivate.DeleteShape(MaskedMap.PublicPositionUid);
			}
			else {
				MapPrivate.UpdateVisibleRectangle(MapPublic.VisibleRectInMap());
			}
		}

		private void AddDisplay_Execute(object sender, ExecutedRoutedEventArgs e) {
			PublicWindow.Show();
			PublicWindow.MaximizeToSelectedMonitor();
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

		private void DrawSquare_Execute(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			if (CheckToggleState(e.OriginalSource)) {
				ActiveTool = new Tools.DrawSquare(this, e.OriginalSource);
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
			var radius = 7 / MapPrivate.ImageScaleMperPix;
			MapPrivate.OverlayCircle(_mouseDownPoint, radius, Colors.OrangeRed, "Spell7m");
			if (PublicWindow.IsVisible) {
				MapPublic.OverlayCircle(_mouseDownPoint, radius, Colors.OrangeRed, "Spell7m");
			}
		}

		private void SpellCircular3m_Execute(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			var radius = 3 / MapPrivate.ImageScaleMperPix;
			MapPrivate.OverlayCircle(_mouseDownPoint, radius, Colors.LightSkyBlue, "Spell3m");
			if (PublicWindow.IsVisible) {
				MapPublic.OverlayCircle(_mouseDownPoint, radius, Colors.LightSkyBlue, "Spell3m");
			}
		}

		private void SpellCircular2m_Execute(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			var radius = 2 / MapPrivate.ImageScaleMperPix;
			MapPrivate.OverlayCircle(_mouseDownPoint, radius, Colors.Yellow, "Spell2m");
			if (PublicWindow.IsVisible) {
				MapPublic.OverlayCircle(_mouseDownPoint, radius, Colors.Yellow, "Spell2m");
			}
		}
       private void CreatePlayer(Color color) {
            ActiveTool = null;
            var radius = 0.5 / MapPrivate.ImageScaleMperPix;

            var dialog = new DialogGetSingleValue {
                LeadText = "Text",
                Owner = this
            };

            var result = dialog.ShowDialog();
            if (!result.HasValue || !result.Value) {
                return;
            }
            var parts = dialog.TextValue.Split(new[] { ',' }, 2);
            string text = " ";
            try {
                if (parts.Length > 0 && !string.IsNullOrEmpty(parts[0])) {
                    text = parts[0];
                }

                if (parts.Length == 2) {
                    if (ColorConverter.ConvertFromString(parts[1]) is Color colorNew) {
                        color = colorNew;
                    }
                }
            }
            catch { // ignored
            }

            MapPrivate.OverlayPlayer(_mouseDownPoint, radius, color, "Player", text);
            if (PublicWindow.IsVisible) {
                MapPublic.OverlayPlayer(_mouseDownPoint, radius, color, "Player", text);
            }
        }


       private void Player_Execute(object sender, ExecutedRoutedEventArgs e) {
           CreatePlayer(Colors.LightBlue);
       }

        private void NonPlayer_Execute(object sender, ExecutedRoutedEventArgs e) {
            CreatePlayer(Colors.Aquamarine);
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
