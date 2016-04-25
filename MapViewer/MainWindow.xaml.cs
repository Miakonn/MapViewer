using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace MapViewer {
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow {

		#region Attributes

		private readonly MaskedMap _mapPrivate;
		private readonly PublicWindow _publicWindow = new PublicWindow();
		private Rectangle _dragRectangle;

		private bool _isDraggingSelectionRect;
		private bool _isMoving;
		private Point _mouseDownPoint;
		private Point _mouseUpPoint;

		private bool _ctrlPressed;

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

		private void MainWinKeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Escape) {
				ClearDragSelectionRect();
			}
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

		#region UI event handler

		private void ButtonOpen(object sender, RoutedEventArgs e) {
			var dialog = new OpenFileDialog();
			var result = dialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK) {
				_mapPrivate.ImageFile = dialog.FileName;
				_publicIsDirty = true;
				Update();
			}
		}

		private void ButtonPublish(object sender, RoutedEventArgs e) {
			_publicWindow.Show();
			_publicWindow.MaximizeToSecondaryMonitor();
			_publicWindow.Map.PublishFrom(_mapPrivate, _publicIsDirty);
			_publicWindow.Map.Draw();
			_publicIsDirty = false;
		}

		private void ButtonClear(object sender, RoutedEventArgs e) {
			_mapPrivate.ClearMask();
			_mapPrivate.Draw();
		}

		private void ButtonClearOverlay(object sender, RoutedEventArgs e) {
			_mapPrivate.ClearOverlay();
			_publicWindow.Map.ClearOverlay();
		}

		private void MainWinSizeChanged(object sender, SizeChangedEventArgs e) {
			Update();
		}

		private void MainWinMouseDown(object sender, MouseButtonEventArgs e) {
			if (e.ChangedButton == MouseButton.Middle && e.ClickCount == 1) {
				_isDraggingSelectionRect = true;
				_mouseDownPoint = e.GetPosition(_mapPrivate.CanvasOverlay);
				e.Handled = true;
				_ctrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl);
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
			if (e.ChangedButton == MouseButton.Middle) {
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

		private void ButtonSetScaleImage(object sender, RoutedEventArgs e) {
			SetScaleDialog();
		}

		#endregion

		private void BtnZoomToFit_OnClick(object sender, RoutedEventArgs e) {
			_mapPrivate.ScaleToWindow();
		}

		private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			System.Windows.Application.Current.Shutdown();
		}
		#region Commands

		private void Fireball_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = true;
		}

		private void Fireball_Executed(object sender, ExecutedRoutedEventArgs e) {
			_mapPrivate.OverlayCircle(_mouseDownPoint, 7, Colors.OrangeRed);
			if (_publicWindow.IsVisible) {
				_publicWindow.Map.OverlayCircle(_mouseDownPoint, 7, Colors.OrangeRed);
			}
		}

		private void Moonbeam_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = true;
		}

		private void Moonbeam_Executed(object sender, ExecutedRoutedEventArgs e) {
			_mapPrivate.OverlayCircle(_mouseDownPoint, 2, Colors.Yellow);
			if (_publicWindow.IsVisible) {
				_publicWindow.Map.OverlayCircle(_mouseDownPoint, 2, Colors.Yellow);
			}
		}

		private void Wall_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = true;
		}

		private void Wall_Executed(object sender, ExecutedRoutedEventArgs e) {
			_mapPrivate.OverlayLine(_mouseDownPoint, _mouseUpPoint, 2, Colors.Yellow);
			if (_publicWindow.IsVisible) {
				_publicWindow.Map.OverlayLine(_mouseDownPoint, _mouseUpPoint , 2, Colors.Yellow);
			}
		}

		private void Measure_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = true;
		}

		private void Measure_Executed(object sender, ExecutedRoutedEventArgs e) {
			var vect = _mouseUpPoint - _mouseDownPoint;
			var dist = _mapPrivate.ImageScaleMperPix * vect.Length;
			MessageBox.Show(string.Format("Length is {0} m", dist));
		}



		#endregion

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
				default:
					break;
			}

		}
	}
}