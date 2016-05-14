﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace MapViewer {
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow {

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

		public MainWindow() {
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
			MapPrivate.Draw();
			MapPublic.Draw();
		}

		private void MovePublic(Vector vector) {
			MapPublic.Translate(vector);
			if (MapPublic.IsLinked) {
				MapPrivate.DeleteShape("VisbileRect");
			}
			else {
				MapPrivate.MoveVisibleRectangle(MapPublic.VisibleRectInMap());
			}
		}

		#endregion

		#region Events

		private void Border_Loaded(object sender, RoutedEventArgs e) {
			var window = GetWindow(this);
			if (window != null) {
				window.KeyDown += MainWinKeyDown;
			}
		}

		private void MainWinKeyDown(object sender, KeyEventArgs e) {

			if (e.Key == Key.Escape) {
				if (ActiveTool != null) {
					ActiveTool = null;
				}
			}
			if (ActiveTool != null) {
				ActiveTool.KeyDown(sender, e);
			}
		}

		private void MainWinSizeChanged(object sender, SizeChangedEventArgs e) {
			MapPrivate.ScaleToWindow();
		}

		private void MainWinMouseDown(object sender, MouseButtonEventArgs e) {

			if (ActiveTool != null) {
				ActiveTool.MouseDown(sender, e);
				return;
			}
			_lastClickedElem = BitmapUtils.FindHitElement(MapPrivate.CanvasOverlay);

			var shape = MapPrivate.CanvasOverlay.FindElementByUid("VisibleRect");

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

		private void MainWinMouseMove(object sender, MouseEventArgs e) {
			var ctrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
			if (ctrlPressed) {
				MapPublic.MovePublicCursor(e.GetPosition(MapPrivate.CanvasOverlay));
			}
			else {
				MapPublic.DeleteShape("PubliCursor");
			}

			if (ActiveTool != null) {
				ActiveTool.MouseMove(sender, e);
				return;
			}

			if (_isDraggingPublicPos) {
				var curMouseDownPoint = e.GetPosition(MapPrivate.CanvasOverlay);
				var scale = MapPublic.Scale;
				MovePublic(new Vector((_mouseDownPoint.X - curMouseDownPoint.X) * scale, (_mouseDownPoint.Y - curMouseDownPoint.Y) * scale));
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

		private void MainWinMouseUp(object sender, MouseButtonEventArgs e) {
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

		private void MainWinMouseWheel(object sender, MouseWheelEventArgs e) {
			var scale = (1.0 + e.Delta / 600.0);

			MapPrivate.Zoom(scale, e.GetPosition(this));
		}

		private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
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
						MapPrivate.IsLinked = true;
						MapPublic.IsLinked = true;
						_publicIsDirty = true;
						break;
					}
			}

		}

		#endregion
	}
}