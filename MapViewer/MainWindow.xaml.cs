using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Forms;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MapViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Canvas _canvasMain1 = new Canvas();
        private Canvas _canvasMain2 = new Canvas();
        private Canvas _canvasMain3 = new Canvas();
        private Canvas _canvasSide1 = new Canvas();
        private Canvas _canvasSide2 = new Canvas();

        private readonly SideWindow _sideWindow = new SideWindow();

        private Map _mapAlfa;

        private bool _isDraggingSelectionRect = false;
        private Point _origMouseDownPoint;

	    private bool _stateInvert;


        private Rectangle _dragRectangle;


        public MainWindow()
        {
            InitializeComponent();
            MapPresenterMain1.Content = _canvasMain1;
            MapPresenterMain2.Content = _canvasMain3;
            MapPresenterMain3.Content = _canvasMain2;

            _mapAlfa = new Map();

            _sideWindow.MapPresenterSide1.Content = _canvasSide1;
            _sideWindow.MapPresenterSide2.Content = _canvasSide2;
            //this.ReleaseMouseCapture();


            _sideWindow.Show();
        }

        private void Update()
        {
            //_canvasMain1.Children.Clear();
            //_canvasMain2.Children.Clear();
            _mapAlfa.DrawMain(_canvasMain1);
        }

        private void ButtonOpen(object sender, RoutedEventArgs e)
        {
			var dialog = new OpenFileDialog();
			var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK) {
                _mapAlfa.ImageFile = dialog.FileName;

                Update();
            }
        }

        private void ButtonPublish(object sender, RoutedEventArgs e)
        {
            _mapAlfa.Publish();
            _mapAlfa.DrawSide(_canvasSide2);
        }

        private void ButtonClear(object sender, RoutedEventArgs e)
        {
	        _mapAlfa.ClearMask();
			_mapAlfa.DrawMain(_canvasMain2);
        }

        private void MainWinSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Update();
        }

        private void MainWinMouseDown(object sender, MouseButtonEventArgs e) {
            Trace.WriteLine("MainWinMouseDown");
            if (e.ChangedButton == MouseButton.Left) {
                _isDraggingSelectionRect = true;
                _origMouseDownPoint = e.GetPosition(this);
                e.Handled = true;
				_stateInvert = Keyboard.IsKeyDown(Key.LeftCtrl);
            }            
        }

        private void MainWinMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_isDraggingSelectionRect) {
                Trace.Write("*");
                Point curMouseDownPoint = e.GetPosition(this);
                if (_dragRectangle == null) {
                    InitDragSelectionRect(_origMouseDownPoint, curMouseDownPoint);
                }
                UpdateDragSelectionRect(_origMouseDownPoint, curMouseDownPoint);

                e.Handled = true;
            }
            else {
                Trace.Write(".");                
            }
        }

        private void MainWinMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) {
                if (_isDraggingSelectionRect) {
                    ApplyDragSelectionRect();
                    ClearDragSelectionRect();
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Initialize the rectangle used for drag selection.
        /// </summary>
        private void InitDragSelectionRect(Point pt1, Point pt2)
        {
            Trace.WriteLine("InitDragSelectionRect");
            _dragRectangle = new Rectangle();
            var x = Math.Min(pt1.X, pt2.X);
            var y = Math.Min(pt1.Y, pt2.Y);
            var width = 5;
            var height = 5;

            Canvas.SetLeft(_dragRectangle, x);
            Canvas.SetTop(_dragRectangle, y);
            _dragRectangle.Width = width;
            _dragRectangle.Height = height;
            _dragRectangle.Fill =  new SolidColorBrush(_stateInvert ? Colors.White : Colors.Black);
            _dragRectangle.Opacity = 0.5;
            //(_canvasLayer1.Children.Add(_dragRectangle);
            _canvasMain2.Children.Add(_dragRectangle);

        }


        /// <summary>
        /// Initialize the rectangle used for drag selection.
        /// </summary>
        private void ClearDragSelectionRect() {
            _dragRectangle = null;

            _isDraggingSelectionRect = false;

        }

        /// <summary>
        /// Update the position and size of the rectangle used for drag  selection.
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

		public Int32Rect GetElementRect(FrameworkElement element) {
			GeneralTransform buttonTransform = element.TransformToVisual(_canvasMain1);
			Point point = buttonTransform.Transform(new Point());
			return new Int32Rect((int)point.X, (int)point.Y, (int)element.ActualWidth, (int) element.ActualHeight);
		}

        private void ApplyDragSelectionRect()
        {
	        if (_dragRectangle != null) {
		        if (_stateInvert) {
					_mapAlfa.RenderRectangle(GetElementRect(_dragRectangle), 0);
		        }
		        else {
			        _mapAlfa.RenderRectangle(GetElementRect(_dragRectangle), 255);
		        }
		        _canvasMain2.Children.Clear();
	        }

        }

        private void MainWinKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape) {
                ClearDragSelectionRect();
            }
        }

 



    }
}
