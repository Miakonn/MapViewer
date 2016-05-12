using System;
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

		public readonly MaskedMap _mapPrivate;
		public readonly PublicWindow _publicWindow = new PublicWindow();
		private Rectangle _dragPublicRect;

		private UIElement _lastClickedElem;

		private bool _isDraggingPublicPos;
		private bool _isMoving;
		private Point _mouseDownPoint;
		private Point _mouseUpPoint;

		private bool _ctrlPressed;
		private bool _altPressed; 
		private bool _shiftPressed;

		public ICanvasTool ActiveTool {get; set;}

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

		private Int32Rect GetElementRect(FrameworkElement element) {
			var buttonTransform = element.TransformToVisual(_mapPrivate.CanvasMapMask);
			var point = buttonTransform.Transform(new Point());
			return new Int32Rect((int)point.X, (int)point.Y, (int)element.ActualWidth, (int)element.ActualHeight);
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

			if (e.Key == Key.Escape) {
				if (ActiveTool != null) {
					ActiveTool.Deactivate();
				}
			}
			if (ActiveTool != null) {
				ActiveTool.KeyDown(sender, e);
				return;
			}

			_ctrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

		}

		private void MainWinSizeChanged(object sender, SizeChangedEventArgs e) {
			_mapPrivate.ScaleToWindow();
		}

		private void MainWinMouseDown(object sender, MouseButtonEventArgs e) {

			if (ActiveTool != null) {
				ActiveTool.MouseDown(sender, e);
				return;
			}
			_lastClickedElem = BitmapUtils.FindHitElement(_mapPrivate.CanvasOverlay);

			_ctrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
			_altPressed = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
			_shiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

			var isPublicPos = _dragPublicRect != null && _dragPublicRect.IsMouseOver;
			if (e.ChangedButton == MouseButton.Left && isPublicPos && e.ClickCount == 1) {
				_isDraggingPublicPos= true;
				_mouseDownPoint = e.GetPosition(_mapPrivate.CanvasOverlay);
				Trace.WriteLine("MouseDown=" + _mouseDownPoint);
				e.Handled = true;
			}
			else if (e.ChangedButton == MouseButton.Left && e.ClickCount == 1) {
				_isMoving = true;
				_mouseDownPoint = e.GetPosition(this);
				e.Handled = true;
			}
			else if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2) {
				var pos = e.GetPosition(_mapPrivate.CanvasMapMask);
				_mapPrivate.OverlayCircle(pos, 25 * _mapPrivate.ImageScaleMperPix, Colors.GreenYellow, "Action");
				if (_publicWindow.IsVisible) {
					_publicWindow.Map.OverlayCircle(pos, 25 * _publicWindow.Map.ImageScaleMperPix, Colors.GreenYellow, "Action");
				}
			}
			else if (e.ChangedButton == MouseButton.Right && e.ClickCount == 1) {
				_mouseDownPoint = e.GetPosition(_mapPrivate.CanvasMapMask);
			}

		}

		private void MainWinMouseMove(object sender, MouseEventArgs e) {
			if (ActiveTool != null) {
				ActiveTool.MouseMove(sender, e);
				return;
			}

			if (_isDraggingPublicPos) {
				var curMouseDownPoint = e.GetPosition(_mapPrivate.CanvasOverlay);
				var scale = _mapPrivate.Scale;
				var scale2 = _publicWindow.Map.Scale;
				MovePublic(new Vector((_mouseDownPoint.X - curMouseDownPoint.X) * scale2, (_mouseDownPoint.Y - curMouseDownPoint.Y) * scale2));
				_mouseDownPoint = curMouseDownPoint;
				Trace.WriteLine("MouseMOve=" + _mouseDownPoint);

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
			if (ActiveTool != null) {
				ActiveTool.MouseUp(sender, e);
				return;
			}


			if (e.ChangedButton == MouseButton.Left && _isDraggingPublicPos) {
				_isDraggingPublicPos = false;
			}

			if (e.ChangedButton == MouseButton.Right) {
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