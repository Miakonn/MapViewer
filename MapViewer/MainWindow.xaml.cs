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

namespace MapViewer {
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow {

		#region Attributes
		private readonly Canvas _canvas = new Canvas();

		private readonly MaskedMap _mapPrivate;
		private readonly PublicWindow _publicWindow = new PublicWindow();
		private Rectangle _dragRectangle;

		private bool _isDraggingSelectionRect;
		private bool _isMoving;
		private Point _origMouseDownPoint;

		private bool _ctrlPressed;

		#endregion

		public MainWindow() {
			InitializeComponent();

			_mapPrivate = new MaskedMap(false) {
				ParentWindow = this,
			};
			MapPresenterMain1.Content = _mapPrivate.CanvasMapMask;
			MapPresenterMain2.Content = _mapPrivate.CanvasOverlay;
			MapPresenterMain3.Content = _canvas;
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
			Trace.WriteLine("InitDragSelectionRect");
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
			_canvas.Children.Add(_dragRectangle);
		}

		/// <summary>
		///     Initialize the rectangle used for drag selection.
		/// </summary>
		private void ClearDragSelectionRect() {
			_canvas.Children.Remove(_dragRectangle);
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
				_canvas.Children.Clear();
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
				LeadText = "Map width in m"
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

				Update();
			}
		}

		private void ButtonPublish(object sender, RoutedEventArgs e) {
			_publicWindow.Map.PublishFrom(_mapPrivate);
			_publicWindow.Map.Draw();
		}

		private void ButtonClear(object sender, RoutedEventArgs e) {
			_mapPrivate.ClearMask();
			_mapPrivate.Draw();
		}

		private void MainWinSizeChanged(object sender, SizeChangedEventArgs e) {
			Update();
		}

		private void MainWinMouseDown(object sender, MouseButtonEventArgs e) {
			if (e.ChangedButton == MouseButton.Middle) {
				_isDraggingSelectionRect = true;
				_origMouseDownPoint = e.GetPosition(_mapPrivate.CanvasMapMask);
				e.Handled = true;
				_ctrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl);
			}
			else if (e.ChangedButton == MouseButton.Left) {
				_isDraggingSelectionRect = false;
				_isMoving = true;
				_origMouseDownPoint = e.GetPosition(_mapPrivate.CanvasMapMask);
				e.Handled = true;
			}
		}

		private void MainWinMouseMove(object sender, MouseEventArgs e) {
			if (_isDraggingSelectionRect) {
				var curMouseDownPoint = e.GetPosition(_mapPrivate.CanvasMapMask);
				if (_dragRectangle == null) {
					InitDragSelectionRect(_origMouseDownPoint, curMouseDownPoint);
				}
				UpdateDragSelectionRect(_origMouseDownPoint, curMouseDownPoint);

				e.Handled = true;
			}
			else if (_isMoving) {
				var curMouseDownPoint = e.GetPosition(_mapPrivate.CanvasMapMask);
				Vector move = curMouseDownPoint - _origMouseDownPoint;
				_mapPrivate.Translate(move);
				_mapPrivate.Draw();
				_origMouseDownPoint = curMouseDownPoint;
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

		private void CheckBoxLinked_OnChecked(object sender, RoutedEventArgs e) {
			_mapPrivate.Linked = true;
			_publicWindow.Map.Linked = true;
			Update();
		}

		private void CheckBoxLinked_OnUnchecked(object sender, RoutedEventArgs e) {
			_mapPrivate.Linked = false;
			_publicWindow.Map.Linked = false;
			Update();
		}
		#endregion

		private void BtnZoomToFit_OnClick(object sender, RoutedEventArgs e) {
			_mapPrivate.ScaleToWindow();
		}
	}
}