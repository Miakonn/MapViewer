﻿using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

namespace MapViewer {
	public static class CustomCommands {

		public static readonly RoutedUICommand Fireball = new RoutedUICommand("Fireball", "Fireball", typeof(CustomCommands), null);
		public static readonly RoutedUICommand Moonbeam = new RoutedUICommand("Moonbeam", "Moonbeam", typeof(CustomCommands), null);
		public static readonly RoutedUICommand Wall = new RoutedUICommand("Wall", "Wall", typeof(CustomCommands), null);
		public static readonly RoutedUICommand Measure = new RoutedUICommand("Measure", "Measure", typeof(CustomCommands), null);
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
	}

	public partial class MainWindow {


		private void OpenImage_Executed(object sender, ExecutedRoutedEventArgs e) {
			var dialog = new OpenFileDialog();
			var result = dialog.ShowDialog();
			if (result != null && result.Value) {
				_mapPrivate.ImageFile = dialog.FileName;
				_publicIsDirty = true;
				Update();
			}
		}

		public void SetScale_Executed(object sender, ExecutedRoutedEventArgs e) {
			SetScaleDialog();
		}

		private void ScaleToFit_Executed(object sender, ExecutedRoutedEventArgs e) {
			_mapPrivate.ScaleToWindow();
		}


		private void UpdateVisibleRectangle() {
			var rect = _publicWindow.Map.VisibleRectInMap();
			if (_dragPublicRect != null) {
				_mapPrivate.CanvasOverlay.Children.Remove(_dragPublicRect);
			}
			if (!_publicWindow.Map.Linked) {
				_dragPublicRect = _mapPrivate.OverlayRectPixel(rect, Colors.LightGreen);
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
			_mapPrivate.OverlayCircle(_mouseDownPoint, 7, Colors.OrangeRed);
			if (_publicWindow.IsVisible) {
				_publicWindow.Map.OverlayCircle(_mouseDownPoint, 7, Colors.OrangeRed);
			}
		}

		private void Moonbeam_Executed(object sender, ExecutedRoutedEventArgs e) {
			_mapPrivate.OverlayCircle(_mouseDownPoint, 2, Colors.Yellow);
			if (_publicWindow.IsVisible) {
				_publicWindow.Map.OverlayCircle(_mouseDownPoint, 2, Colors.Yellow);
			}
		}

		private void Wall_Executed(object sender, ExecutedRoutedEventArgs e) {
			_mapPrivate.OverlayLine(_mouseDownPoint, _mouseUpPoint, 2, Colors.Yellow);
			if (_publicWindow.IsVisible) {
				_publicWindow.Map.OverlayLine(_mouseDownPoint, _mouseUpPoint, 2, Colors.Yellow);
			}
		}

		private void Measure_Executed(object sender, ExecutedRoutedEventArgs e) {
			var vect = _mouseUpPoint - _mouseDownPoint;
			var dist = _mapPrivate.ImageScaleMperPix * vect.Length;
			MessageBox.Show(string.Format("Length is {0} m", dist));
		}

		private void FullMask_Executed(object sender, ExecutedRoutedEventArgs e) {
			var rect = new Int32Rect(0, 0, (int)_mapPrivate.Image.Width, (int)_mapPrivate.Image.Height);
			_mapPrivate.RenderRectangle(rect, 255);
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



		private void ImageNeeded_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = (_mapPrivate != null && !string.IsNullOrWhiteSpace(_mapPrivate.ImageFile));
		}

		private void Allways_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = true;
		}
		private void Spell_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = true;
		}


	}
}