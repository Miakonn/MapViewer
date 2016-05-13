using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using System.Windows.Controls.Ribbon;

namespace MapViewer {
	public static class CustomCommands {

		public static readonly RoutedUICommand Fireball = new RoutedUICommand("Fireball", "Fireball", typeof(CustomCommands), null);
		public static readonly RoutedUICommand Moonbeam = new RoutedUICommand("Moonbeam", "Moonbeam", typeof(CustomCommands), null);
		public static readonly RoutedUICommand DeleteElement = new RoutedUICommand("Delete element", "Delete element", typeof(CustomCommands), null);
		public static readonly RoutedUICommand FullMask = new RoutedUICommand("Full mask", "Full mask", typeof(CustomCommands), null);

		public static readonly RoutedUICommand OpenImage = new RoutedUICommand("Open image", "Open image", typeof(CustomCommands), null);
		public static readonly RoutedUICommand PublishMap = new RoutedUICommand("Publish", "Publish", typeof(CustomCommands), null);
		public static readonly RoutedUICommand ClearMask = new RoutedUICommand("Clear mask", "Clear mask", typeof(CustomCommands), null);
		public static readonly RoutedUICommand ClearOverlay = new RoutedUICommand("Clear overlay", "Clear overlay", typeof(CustomCommands), null);
		public static readonly RoutedUICommand ScaleToFit= new RoutedUICommand("Scale to fit", "Scale to fit", typeof(CustomCommands), null);

		public static readonly RoutedUICommand RotateMap = new RoutedUICommand("Rotate map", "Rotate map", typeof(CustomCommands), null);
		public static readonly RoutedUICommand AddDisplay = new RoutedUICommand("Add display", "Add display", typeof(CustomCommands), null);
		public static readonly RoutedUICommand RemoveDisplay = new RoutedUICommand("Remove display", "Remove display", typeof(CustomCommands), null);

		public static readonly RoutedUICommand Save = new RoutedUICommand("Save", "Save", typeof(CustomCommands), null);

		public static readonly RoutedUICommand Calibrate = new RoutedUICommand("Calibrate", "Calibrate", typeof(CustomCommands), null);
		public static readonly RoutedUICommand Measure = new RoutedUICommand("Measure", "Measure", typeof(CustomCommands), null);
		public static readonly RoutedUICommand DrawLine = new RoutedUICommand("Draw line", "Draw line", typeof(CustomCommands), null);
		public static readonly RoutedUICommand DrawCone = new RoutedUICommand("Draw cone", "Draw cone", typeof(CustomCommands), null);
		public static readonly RoutedUICommand DrawCircle = new RoutedUICommand("Draw circle", "Draw circle", typeof(CustomCommands), null);
		public static readonly RoutedUICommand DrawRectangle = new RoutedUICommand("Draw rectangle", "Draw rectangle", typeof(CustomCommands), null);

		public static readonly RoutedUICommand MaskRectangle = new RoutedUICommand("Mask rectangle", "Mask rectangle", typeof(CustomCommands), null);
		public static readonly RoutedUICommand UnmaskRectangle = new RoutedUICommand("Unmask rectangle", "Unmask rectangle", typeof(CustomCommands), null);
		public static readonly RoutedUICommand MaskCircle = new RoutedUICommand("Mask circle", "Mask circle", typeof(CustomCommands), null);
		public static readonly RoutedUICommand UnmaskCircle = new RoutedUICommand("Unmask circle", "Unmask circle", typeof(CustomCommands), null);
	}

	public partial class MainWindow {

		private void OpenImage_Executed(object sender, ExecutedRoutedEventArgs e) {
			var dialog = new OpenFileDialog {Filter = "Image Files|*.jpg;*.bmp;*.png"};
			var result = dialog.ShowDialog();
			if (result != null && result.Value) {
				MapPrivate.ImageFile = dialog.FileName;
				_publicIsDirty = true;
				Update();
			}
		}

		private void ScaleToFit_Executed(object sender, ExecutedRoutedEventArgs e) {
			MapPrivate.ScaleToWindow();
		}

		private void Save_Executed(object sender, ExecutedRoutedEventArgs e) {
			MapPrivate.Serialize();
		}

		private void UpdateVisibleRectangle() {
			var rect = MapPublic.VisibleRectInMap();
			if (_dragPublicRect != null) {
				MapPrivate.CanvasOverlay.Children.Remove(_dragPublicRect);
			}
			if (!MapPublic.Linked) {
				_dragPublicRect = MapPrivate.OverlayRectangle(rect, Colors.Red);
			}			
		}

		private void PublishMap_Executed(object sender, ExecutedRoutedEventArgs e) {
			MapPublic.PublishFrom(MapPrivate, _publicIsDirty);
			MapPublic.Draw();
			PublicWindow.SetRuler(MapPublic.ImageScaleMperPix / MapPublic.Scale);
			_publicIsDirty = false;

			UpdateVisibleRectangle();

			Activate();
		}

		private void ClearMask_Executed(object sender, ExecutedRoutedEventArgs e) {
			MapPrivate.ClearMask();
			MapPrivate.Draw();
		}

		private void ClearOverlay_Executed(object sender, ExecutedRoutedEventArgs e) {
			MapPrivate.ClearOverlay();
			if (_dragPublicRect != null) {
				MapPrivate.CanvasOverlay.Children.Add(_dragPublicRect);
			}

			MapPublic.ClearOverlay();
		}

		private void Fireball_Executed(object sender, ExecutedRoutedEventArgs e) {
			var radius = 7 / MapPrivate.ImageScaleMperPix;
			MapPrivate.OverlayCircle(_mouseDownPoint, radius, Colors.OrangeRed, "Fireball");
			if (PublicWindow.IsVisible) {
				MapPublic.OverlayCircle(_mouseDownPoint, radius, Colors.OrangeRed, "Fireball");
			}
		}

		private void Moonbeam_Executed(object sender, ExecutedRoutedEventArgs e) {
			var radius = 2 / MapPrivate.ImageScaleMperPix;
			MapPrivate.OverlayCircle(_mouseDownPoint, radius, Colors.Yellow, "Moonbeam");
			if (PublicWindow.IsVisible) {
				MapPublic.OverlayCircle(_mouseDownPoint, radius, Colors.Yellow, "Moonbeam");
			}
		}

		private void DeleteElement_Executed(object sender, ExecutedRoutedEventArgs e) {
			if (_lastClickedElem == null) {
				return;
			}

			var uid = _lastClickedElem.Uid;
			MapPrivate.CanvasOverlay.Children.Remove(_lastClickedElem);
			if (PublicWindow.IsVisible) {
				var elemPublic = BitmapUtils.FindElementByUid(MapPublic.CanvasOverlay, uid);
				if (elemPublic != null) {
					MapPublic.CanvasOverlay.Children.Remove(elemPublic);
				}
			}
		}

		private void FullMask_Executed(object sender, ExecutedRoutedEventArgs e) {
			var rect = new Int32Rect(0, 0, (int)MapPrivate.Image.Width, (int)MapPrivate.Image.Height);
			MapPrivate.MaskRectangle(rect, 255);
		}

		private void RotateMap_Executed(object sender, ExecutedRoutedEventArgs e) {
			MapPublic.RotateClockwise();
			UpdateVisibleRectangle();
		}

		private void AddDisplay_Executed(object sender, ExecutedRoutedEventArgs e) {
			PublicWindow.Show();
			PublicWindow.MaximizeToSecondaryMonitor();
		}

		private void RemoveDisplay_Executed(object sender, ExecutedRoutedEventArgs e) {
			PublicWindow.Visibility = Visibility.Hidden;
		}

		#region Can execute

		private void ImageNeeded_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = (MapPrivate != null && !string.IsNullOrWhiteSpace(MapPrivate.ImageFile));
		}

		private void Allways_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = true;
		}

		private void Spell_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = (MapPrivate != null && !string.IsNullOrWhiteSpace(MapPrivate.ImageFile));
		}

		#endregion

		#region Tools

		private bool CheckToggleState(object source) {
			var button = source as RibbonToggleButton;
			if (button != null) {
				return (button.IsChecked.HasValue && button.IsChecked.Value);
			}
			return true;
		}

		private void Measure_Executed(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			if (CheckToggleState(e.OriginalSource)) {
				ActiveTool = new Tools.Measure(this, e.OriginalSource);
			}
		}

		private void Calibrate_Executed(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			if (CheckToggleState(e.OriginalSource)) {
				ActiveTool = new Tools.Calibrate(this, e.OriginalSource);
			}
		}

		private void MaskRectangle_Executed(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			if (CheckToggleState(e.OriginalSource)) {
				var tool = new Tools.MaskRectangle(this, e.OriginalSource, true);
				ActiveTool = tool;
			}
		}

		private void UnmaskRectangle_Executed(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			if (CheckToggleState(e.OriginalSource)) {
				var tool = new Tools.MaskRectangle(this, e.OriginalSource, false);
				ActiveTool = tool;
			}
		}

		private void MaskCircle_Executed(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			if (CheckToggleState(e.OriginalSource)) {
				ActiveTool = new Tools.MaskCircle(this, e.OriginalSource, true);
			}
		}

		private void UnmaskCircle_Executed(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			if (CheckToggleState(e.OriginalSource)) {
				ActiveTool = new Tools.MaskCircle(this, e.OriginalSource, false);
			}
		}

		#endregion
		#region Spells
		private void DrawCircle_Executed(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			if (CheckToggleState(e.OriginalSource)) {
				ActiveTool = new Tools.DrawCircle(this, e.OriginalSource);
			}
		}

		private void DrawCone_Executed(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			if (CheckToggleState(e.OriginalSource)) {
				ActiveTool = new Tools.DrawCone(this, e.OriginalSource);
			}
		}

		private void DrawLine_Executed(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			if (CheckToggleState(e.OriginalSource)) {
				ActiveTool = new Tools.DrawLine(this, e.OriginalSource);
			}
		}

		private void DrawRectangle_Executed(object sender, ExecutedRoutedEventArgs e) {
			ActiveTool = null;
			if (CheckToggleState(e.OriginalSource)) {
				ActiveTool = new Tools.DrawRectangle(this, e.OriginalSource);
			}
		}
		#endregion

	}
}
