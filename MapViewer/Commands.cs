using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

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
		public static readonly RoutedUICommand SetScale = new RoutedUICommand("Set scale", "Set scale", typeof(CustomCommands), null);
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

		public static readonly RoutedUICommand MaskRectangle = new RoutedUICommand("Mask rectangle", "Mask rectangle", typeof(CustomCommands), null);
		public static readonly RoutedUICommand UnmaskRectangle = new RoutedUICommand("Unmask rectangle", "Unmask rectangle", typeof(CustomCommands), null);
		public static readonly RoutedUICommand MaskCircle = new RoutedUICommand("Mask circle", "Mask circle", typeof(CustomCommands), null);
		public static readonly RoutedUICommand UnmaskCircle = new RoutedUICommand("Unmask circle", "Unmask circle", typeof(CustomCommands), null);
	}

	public partial class MainWindow {

		private void OpenImage_Executed(object sender, ExecutedRoutedEventArgs e) {
			var dialog = new OpenFileDialog();
			dialog.Filter = "Image Files|*.jpg;*.bmp;*.png";
			var result = dialog.ShowDialog();
			if (result != null && result.Value) {
				_mapPrivate.ImageFile = dialog.FileName;
				_publicIsDirty = true;
				Update();
			}
		}

		private void SetScale_Executed(object sender, ExecutedRoutedEventArgs e) {
			SetScaleDialog();
		}

		private void ScaleToFit_Executed(object sender, ExecutedRoutedEventArgs e) {
			_mapPrivate.ScaleToWindow();
		}

		private void Save_Executed(object sender, ExecutedRoutedEventArgs e) {
			_mapPrivate.Serialize();
		}

		private void UpdateVisibleRectangle() {
			var rect = _publicWindow.Map.VisibleRectInMap();
			if (_dragPublicRect != null) {
				_mapPrivate.CanvasOverlay.Children.Remove(_dragPublicRect);
			}
			if (!_publicWindow.Map.Linked) {
				_dragPublicRect = _mapPrivate.OverlayRectPixel(rect, Colors.Red);
			}			
		}

		private void PublishMap_Executed(object sender, ExecutedRoutedEventArgs e) {
			_publicWindow.Map.PublishFrom(_mapPrivate, _publicIsDirty);
			_publicWindow.Map.Draw();
			_publicIsDirty = false;

			UpdateVisibleRectangle();

			Activate();
		}

		private void ClearMask_Executed(object sender, ExecutedRoutedEventArgs e) {
			_mapPrivate.ClearMask();
			_mapPrivate.Draw();
		}

		private void ClearOverlay_Executed(object sender, ExecutedRoutedEventArgs e) {
			_mapPrivate.ClearOverlay();
			if (_dragPublicRect != null) {
				_mapPrivate.CanvasOverlay.Children.Add(_dragPublicRect);
			}

			_publicWindow.Map.ClearOverlay();
		}

		private void Fireball_Executed(object sender, ExecutedRoutedEventArgs e) {
			var radius = 7 / _mapPrivate.ImageScaleMperPix;
			_mapPrivate.OverlayCircle(_mouseDownPoint, radius, Colors.OrangeRed, "Fireball");
			if (_publicWindow.IsVisible) {
				_publicWindow.Map.OverlayCircle(_mouseDownPoint, radius, Colors.OrangeRed, "Fireball");
			}
		}

		private void Moonbeam_Executed(object sender, ExecutedRoutedEventArgs e) {
			var radius = 2 / _mapPrivate.ImageScaleMperPix;
			_mapPrivate.OverlayCircle(_mouseDownPoint, radius, Colors.Yellow, "Moonbeam");
			if (_publicWindow.IsVisible) {
				_publicWindow.Map.OverlayCircle(_mouseDownPoint, radius, Colors.Yellow, "Moonbeam");
			}
		}

		private void DeleteElement_Executed(object sender, ExecutedRoutedEventArgs e) {


			if (_lastClickedElem == null) {
				return;
			}

			var uid = _lastClickedElem.Uid;
			_mapPrivate.CanvasOverlay.Children.Remove(_lastClickedElem);
			if (_publicWindow.IsVisible) {
				var elemPublic = BitmapUtils.FindElementByUID(_publicWindow.Map.CanvasOverlay, uid);
				if (elemPublic != null) {
					_publicWindow.Map.CanvasOverlay.Children.Remove(elemPublic);
				}
			}
		}

		private void FullMask_Executed(object sender, ExecutedRoutedEventArgs e) {
			var rect = new Int32Rect(0, 0, (int)_mapPrivate.Image.Width, (int)_mapPrivate.Image.Height);
			_mapPrivate.MaskRectangle(rect, 255);
		}

		private void RotateMap_Executed(object sender, ExecutedRoutedEventArgs e) {
			//_publicWindow.Map.RotationAngle = (_publicWindow.Map.RotationAngle + 90) % 360;
			_publicWindow.Map.RotateClockwise();
			UpdateVisibleRectangle();
		}

		private void AddDisplay_Executed(object sender, ExecutedRoutedEventArgs e) {
			_publicWindow.Show();
			_publicWindow.MaximizeToSecondaryMonitor();
		}

		private void RemoveDisplay_Executed(object sender, ExecutedRoutedEventArgs e) {
			_publicWindow.Visibility = Visibility.Hidden;
		}

		#region Can execute

		private void ImageNeeded_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = (_mapPrivate != null && !string.IsNullOrWhiteSpace(_mapPrivate.ImageFile));
		}

		private void Allways_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = true;
		}

		private void Spell_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = (_mapPrivate != null && !string.IsNullOrWhiteSpace(_mapPrivate.ImageFile));
		}

		#endregion

		#region Tools
		private void Measure_Executed(object sender, ExecutedRoutedEventArgs e) {
			var tool = new Tools.Measure(this, e.OriginalSource);
			ActiveTool = tool;
		}

		private void Calibrate_Executed(object sender, ExecutedRoutedEventArgs e) {
			var tool = new Tools.Calibrate(this, e.OriginalSource);
			ActiveTool = tool;
		}

		private void MaskRectangle_Executed(object sender, ExecutedRoutedEventArgs e) {
			var tool = new Tools.MaskRectangle(this, e.OriginalSource, true);
			ActiveTool = tool;
		}

		private void UnmaskRectangle_Executed(object sender, ExecutedRoutedEventArgs e) {
			var tool = new Tools.MaskRectangle(this, e.OriginalSource, false);
			ActiveTool = tool;
		}

		private void MaskCircle_Executed(object sender, ExecutedRoutedEventArgs e) {
			var tool = new Tools.MaskCircle(this, e.OriginalSource, true);
			ActiveTool = tool;
		}

		private void UnmaskCircle_Executed(object sender, ExecutedRoutedEventArgs e) {
			var tool = new Tools.MaskCircle(this, e.OriginalSource, false);
			ActiveTool = tool;
		}

		#endregion
		#region Spells
		private void DrawCircle_Executed(object sender, ExecutedRoutedEventArgs e) {
			var tool = new Tools.DrawCircle(this, e.OriginalSource);
			ActiveTool = tool;
		}

		private void DrawCone_Executed(object sender, ExecutedRoutedEventArgs e) {
			var tool = new Tools.DrawCone(this, e.OriginalSource);
			ActiveTool = tool;
		}

		private void DrawLine_Executed(object sender, ExecutedRoutedEventArgs e) {
			var tool = new Tools.DrawLine(this, e.OriginalSource);
			ActiveTool = tool;
		}

		#endregion

	}
}
