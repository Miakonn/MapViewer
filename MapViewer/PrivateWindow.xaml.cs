﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace MapViewer {
	/// <summary>
	///     Interaction logic for PrivateWindow.xaml
	/// </summary>
	public partial class PrivateWindow {

		#region Attributes

		public readonly MaskedMap MapPrivate;
		public readonly MaskedMap MapPublic;
		public readonly PublicWindow PublicWindow = new PublicWindow();

		private UIElement _lastClickedElem;
		private bool _isDraggingPublicPos;
		private bool _isMoving;
		private Point _mouseDownPoint;
		private ICanvasTool _activeTool;

		public ICanvasTool ActiveTool {
			get { 
				return _activeTool; 
			}

			set {
				if (_activeTool != null) {
					_activeTool.Deactivate();
				}
				_activeTool = value;
			}
		}

		private bool _publicIsDirty;
	
		#endregion

		public PrivateWindow() {
			InitializeComponent();

			MapPrivate = new MaskedMap(false) {
				ParentWindow = this,
			};

			MapPublic = PublicWindow.Map;
			MapPresenterMain1.Content = MapPrivate.CanvasMapMask;
			MapPresenterMain2.Content = MapPrivate.CanvasOverlay;

			ComboBoxPublicScale.SelectedIndex = 0;
		}

		#region Private methods

		private void CreateWindows() {
			MapPrivate.Create();
			MapPublic.Create();
		}

		private void MovePublic(Vector vector) {
			MapPublic.Translate(vector / MapPublic.ScaleDpiFix);
			if (MapPublic.IsLinked) {
				MapPrivate.DeleteShape(MaskedMap.PublicPositionUid);
			}
			else {
				MapPrivate.UpdateVisibleRectangle(MapPublic.VisibleRectInMap());
			}
		}

		#endregion

		#region Events

		private void Border_Loaded(object sender, RoutedEventArgs e) {
			var window = GetWindow(this);
			if (window != null) {
				window.KeyDown += PrivateWinKeyDown;
			}
		}

		private void PrivateWinKeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Escape) {
				if (ActiveTool != null) {
					ActiveTool = null;
				}
			}
			if (e.Key == Key.F12 && Publish_CanExecute()) {
				PublishMap_Executed(null, null);
			}
			if (ActiveTool != null) {
				ActiveTool.KeyDown(sender, e);
			}
		}

		private void PrivateWinSizeChanged(object sender, SizeChangedEventArgs e) {
			MapPrivate.ScaleToWindow();
		}

		private void PrivateWinMouseDown(object sender, MouseButtonEventArgs e) {

			if (ActiveTool != null) {
				ActiveTool.MouseDown(sender, e);
				return;
			}
			_lastClickedElem = BitmapUtils.FindHitElement(MapPrivate.CanvasOverlay);

			var shape = MapPrivate.CanvasOverlay.FindElementByUid(MaskedMap.PublicPositionUid);

			var isPublicPos = shape != null && shape.IsMouseOver;
			if (e.ChangedButton == MouseButton.Left && isPublicPos && e.ClickCount == 1) {
				_isDraggingPublicPos= true;
				_mouseDownPoint = e.GetPosition(MapPrivate.CanvasOverlay);
				e.Handled = true;
			}
			else if (e.ChangedButton == MouseButton.Left && e.ClickCount == 1) {
				_isMoving = true;
				_mouseDownPoint = e.GetPosition(this);
				e.Handled = true;
			}
			else if (e.ChangedButton == MouseButton.Right && e.ClickCount == 1) {
				_mouseDownPoint = e.GetPosition(MapPrivate.CanvasMapMask);
			}
		}

		private void PrivateWinMouseMove(object sender, MouseEventArgs e) {
			MapPublic.MovePublicCursor(e.GetPosition(MapPrivate.CanvasOverlay));

			if (ActiveTool != null) {
				ActiveTool.MouseMove(sender, e);
				return;
			}

			if (_isDraggingPublicPos) {
				var curMouseDownPoint = e.GetPosition(MapPrivate.CanvasOverlay);
				MovePublic(new Vector((_mouseDownPoint.X - curMouseDownPoint.X), (_mouseDownPoint.Y - curMouseDownPoint.Y)) * MapPublic.Scale * MapPublic.ScaleDpiFix);
				_mouseDownPoint = curMouseDownPoint;

				e.Handled = true;
			}
			else if (_isMoving) {
				var curMouseDownPoint = e.GetPosition(this);
				var move = curMouseDownPoint - _mouseDownPoint;
				MapPrivate.Translate(move);
				_mouseDownPoint = curMouseDownPoint;
				e.Handled = true;
			}


		}

		private void PrivateWinMouseUp(object sender, MouseButtonEventArgs e) {
			if (ActiveTool != null) {
				ActiveTool.MouseUp(sender, e);
				return;
			}


			if (e.ChangedButton == MouseButton.Left && _isDraggingPublicPos) {
				_isDraggingPublicPos = false;
			}

			if (e.ChangedButton == MouseButton.Left) {
				_isMoving = false;
				e.Handled = true;
			}
		}

		private void PrivateWinMouseWheel(object sender, MouseWheelEventArgs e) {
			var scale = (1.0 + e.Delta / 600.0);

			MapPrivate.Zoom(scale, e.GetPosition(this));
		}

		private void PrivateWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			Application.Current.Shutdown();
		}

		private void ComboBoxPublicScale_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
			switch (ComboBoxPublicScale.SelectedIndex) {
				case 0: {
						MapPrivate.IsLinked = false;
						MapPublic.IsLinked = false;
						MapPublic.ScreenScaleMMperM = 20.0;
						_publicIsDirty = true;
						break;
					}
				case 1: {
						MapPrivate.IsLinked = false;
						MapPublic.IsLinked = false;
						MapPublic.ScreenScaleMMperM = 10.0;
						_publicIsDirty = true;
						break;
					}
				case 2: {
						MapPrivate.IsLinked = false;
						MapPublic.IsLinked = false;
						MapPublic.ScreenScaleMMperM = 5.0;
						_publicIsDirty = true;
						break;
					}
				case 3: {
						MapPrivate.IsLinked = false;
						MapPublic.IsLinked = false;
						MapPublic.ScreenScaleMMperM = 2.0;
						_publicIsDirty = true;
						break;
					}
				case 4: {
						MapPrivate.IsLinked = true;
						MapPublic.IsLinked = true;
						_publicIsDirty = true;
						break;
					}
			}

		}
		private void Tab_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
			ActiveTool = null;
		}
		#endregion


#region Public methods


		public void DisplayPopup(string text) {
			PopupDisplay.IsOpen = true;
			var popupText = PopupDisplay.Child as TextBlock;
			if (popupText != null) {
				popupText.Text = text;
			}
		}

		public void HidePopup() {
			PopupDisplay.IsOpen = false;
		}

#endregion region


	}
}