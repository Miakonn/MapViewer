﻿using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace MapViewer {
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow {

		#region Attributes

		private readonly MaskedMap _mapPrivate;
		private readonly PublicWindow _publicWindow = new PublicWindow();
		private Rectangle _dragRectangle;
		private Rectangle _dragPublicRect;

		private bool _isDraggingSelectionRect;
		private bool _isMoving;
		private Point _mouseDownPoint;
		private Point _mouseUpPoint;

		private bool _ctrlPressed;
		private bool _altPressed; 
		private bool _shiftPressed;


		private bool _publicIsDirty;
	
		#endregion

		public MainWindow() {
			InitializeComponent();

			_mapPrivate = new MaskedMap(false) {
				ParentWindow = this,
			};
			MapPresenterMain1.Content = _mapPrivate.CanvasMapMask;
			MapPresenterMain2.Content = _mapPrivate.CanvasOverlay;

			ComboBoxPublicScale.SelectedIndex = 0;
		}

		#region Private methods

		private void Update() {
			_mapPrivate.Draw();
			_publicWindow.Map.Draw();
		}

		/// <summary>
		///     Initialize the rectangle used for drag selection.
		/// </summary>
		private void InitDragSelectionRect(Point pt1, Point pt2) {
			var x = Math.Min(pt1.X, pt2.X);
			var y = Math.Min(pt1.Y, pt2.Y);

			_dragRectangle = new Rectangle {
				Width = 5,
				Height = 5,
				Fill = new SolidColorBrush(_ctrlPressed ? Colors.White : Colors.Black),
				Opacity = 0.5
			};

			Canvas.SetLeft(_dragRectangle, x);
			Canvas.SetTop(_dragRectangle, y);
			_mapPrivate.CanvasOverlay.Children.Add(_dragRectangle);
		}

		/// <summary>
		///     Initialize the rectangle used for drag selection.
		/// </summary>
		private void ClearDragSelectionRect() {
			_mapPrivate.CanvasOverlay.Children.Remove(_dragRectangle);
			_dragRectangle = null;
			_isDraggingSelectionRect = false;
			_isMoving = false;
		}

		/// <summary>
		///     Update the position and size of the rectangle used for drag  selection.
		/// </summary>
		private void UpdateDragSelectionRect(Point pt1, Point pt2) {
			Trace.WriteLine("UpdateDragSelectionRect");
			var x = Math.Min(pt1.X, pt2.X);
			var y = Math.Min(pt1.Y, pt2.Y);
			var width = Math.Abs(pt1.X - pt2.X);
			var height = Math.Abs(pt1.Y - pt2.Y);
			Canvas.SetLeft(_dragRectangle, x);
			Canvas.SetTop(_dragRectangle, y);
			_dragRectangle.Width = width;
			_dragRectangle.Height = height;
		}

		private Int32Rect GetElementRect(FrameworkElement element) {
			var buttonTransform = element.TransformToVisual(_mapPrivate.CanvasMapMask);
			var point = buttonTransform.Transform(new Point());
			return new Int32Rect((int)point.X, (int)point.Y, (int)element.ActualWidth, (int)element.ActualHeight);
		}

		private void ApplyDragSelectionRect() {
			if (_dragRectangle != null) {
				_mapPrivate.RenderRectangle(GetElementRect(_dragRectangle), (byte)(_ctrlPressed ? 0 : 255));
				ClearDragSelectionRect();
			}
		}


		private void MovePublic(Vector vector) {
			_publicWindow.Map.Translate(vector);
			_publicWindow.Map.Draw();
			var rect = _publicWindow.Map.VisibleRectInMap();

			Canvas.SetLeft(_dragPublicRect, rect.X);
			Canvas.SetTop(_dragPublicRect, rect.Y);
			_dragPublicRect.Width = rect.Width;
			_dragPublicRect.Height = rect.Height;
		}

		#endregion

		#region Public Methods

		public bool SetScaleDialog() {
			var dialog = new DialogGetFloatValue {
				LeadText = "Map width in m",
				Value = _mapPrivate.MapData.ImageLengthM
			};

			var result = dialog.ShowDialog();
			if (!result.HasValue || !result.Value) {
				return false;
			}
			_mapPrivate.MapData.ImageLengthM = dialog.Value;
			return true;
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
			_ctrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

			if (e.Key == Key.Escape) {
				if (_isDraggingSelectionRect) {
					ClearDragSelectionRect();
				}
			}
			else if (e.Key == Key.Left) {
				MovePublic(new Vector(_ctrlPressed ? 80 : 10, 0));
			}
			else if (e.Key == Key.Right) {
				MovePublic(new Vector(_ctrlPressed ? -80 : -10, 0));
			}
			else if (e.Key == Key.Up) {
				MovePublic(new Vector(0, _ctrlPressed ? 80 : 10));
			}
			else if (e.Key == Key.Down) {
				MovePublic(new Vector(0, _ctrlPressed ? -80 : -10));
			}
		}

		private void MainWinSizeChanged(object sender, SizeChangedEventArgs e) {
			_mapPrivate.ScaleToWindow();
		}

		private void MainWinMouseDown(object sender, MouseButtonEventArgs e) {

			_ctrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
			_altPressed = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
			_shiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

			if (e.ChangedButton == MouseButton.Left && _shiftPressed  && e.ClickCount == 1) {
				_isDraggingSelectionRect = true;
				_mouseDownPoint = e.GetPosition(_mapPrivate.CanvasOverlay);
				e.Handled = true;
			}
			else if (e.ChangedButton == MouseButton.Left && e.ClickCount == 1) {
				_isDraggingSelectionRect = false;
				_isMoving = true;
				_mouseDownPoint = e.GetPosition(this);
				e.Handled = true;
			}
			else if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2) {
				var pos = e.GetPosition(_mapPrivate.CanvasMapMask);
				_mapPrivate.OverlayCircle(pos, 25 * _mapPrivate.ImageScaleMperPix, Colors.GreenYellow);
				if (_publicWindow.IsVisible) {
					_publicWindow.Map.OverlayCircle(pos, 25*_publicWindow.Map.ImageScaleMperPix, Colors.GreenYellow);
				}
			}
			else if (e.ChangedButton == MouseButton.Right && e.ClickCount == 1) {
				_mouseDownPoint = e.GetPosition(_mapPrivate.CanvasMapMask);
			}

		}

		private void MainWinMouseMove(object sender, MouseEventArgs e) {
			if (_isDraggingSelectionRect) {
				var curMouseDownPoint = e.GetPosition(_mapPrivate.CanvasOverlay);
				if (_dragRectangle == null) {
					InitDragSelectionRect(_mouseDownPoint, curMouseDownPoint);
				}
				UpdateDragSelectionRect(_mouseDownPoint, curMouseDownPoint);

				e.Handled = true;
			}
			else if (_isMoving) {
				var curMouseDownPoint = e.GetPosition(this);
				Vector move = curMouseDownPoint - _mouseDownPoint;
				_mapPrivate.Translate(move);
				//_mapPrivate.Draw();
				_mouseDownPoint = curMouseDownPoint;
				e.Handled = true;
			}			
		}

		private void MainWinMouseUp(object sender, MouseButtonEventArgs e) {
			if (e.ChangedButton == MouseButton.Left && _shiftPressed) {
				if (_isDraggingSelectionRect) {
					ApplyDragSelectionRect();
					ClearDragSelectionRect();
					e.Handled = true;
				}
			}
			else if (e.ChangedButton == MouseButton.Right) {
				_mouseUpPoint = e.GetPosition(_mapPrivate.CanvasMapMask);
			}
			else if (e.ChangedButton == MouseButton.Left) {
				_isMoving = false;
				e.Handled = true;
			}
		}

		private void MainWinMouseWheel(object sender, MouseWheelEventArgs e) {
			double scale = (1.0 + e.Delta / 600.0);

			_mapPrivate.Zoom(scale, e.GetPosition(this));
			_mapPrivate.Draw();
		}

		private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			Application.Current.Shutdown();
		}

		private void ComboBoxPublicScale_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
			switch (ComboBoxPublicScale.SelectedIndex) {
				case 0: {
						_mapPrivate.Linked = false;
						_publicWindow.Map.Linked = false;
						_publicWindow.Map.ScreenScaleMMperM = 20.0;
						_publicIsDirty = true;
						break;
					}
				case 1: {
						_mapPrivate.Linked = false;
						_publicWindow.Map.Linked = false;
						_publicWindow.Map.ScreenScaleMMperM = 10.0;
						_publicIsDirty = true;
						break;
					}
				case 2: {
						_mapPrivate.Linked = true;
						_publicWindow.Map.Linked = true;
						_publicIsDirty = true;
						break;
					}
			}

		}

		#endregion
	}
}