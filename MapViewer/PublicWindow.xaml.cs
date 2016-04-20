﻿using System.Windows;
using System.Windows.Input;

namespace MapViewer
{
    /// <summary>
    /// Interaction logic for SideWindow.xaml
    /// </summary>
    public partial class PublicWindow {

		private readonly MaskedMap _map;
		private bool _isMoving;
		private Point _origMouseDownPoint;


	    public MaskedMap Map {
		    get { return _map;  }
	    }

	    public PublicWindow() {
            InitializeComponent();

			_map = new MaskedMap(true) {
				ParentWindow = this
			};

			MapPresenterPublic1.Content = _map.CanvasMapMask;
			MapPresenterPublic2.Content = _map.CanvasOverlay;
			Show();

        }

		private void PublicWin_OnMouseDown(object sender, MouseButtonEventArgs e) {
			if (e.ChangedButton == MouseButton.Left) {
				_isMoving = true;
				_origMouseDownPoint = e.GetPosition(_map.CanvasMapMask);
				e.Handled = true;
			}
		}

	    private void PublicWin_OnMouseMove(object sender, MouseEventArgs e) {
			if (_isMoving) {
				var curMouseDownPoint = e.GetPosition(_map.CanvasMapMask);
				Vector move = curMouseDownPoint - _origMouseDownPoint;
				_map.Translate(move);
				_map.Draw();
				_origMouseDownPoint = curMouseDownPoint;
				e.Handled = true;
			}	
	    }

	    private void PublicWin_OnMouseUp(object sender, MouseButtonEventArgs e) {
			if (e.ChangedButton == MouseButton.Left) {
				_isMoving = false;
				e.Handled = true;
			}
	    }


	    private void PublicWinKeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Escape) {
				_isMoving = false;
			}
	    }
    }
}
