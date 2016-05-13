using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace MapViewer
{
    /// <summary>
    /// Interaction logic for SideWindow.xaml
    /// </summary>
    public partial class PublicWindow {

		private readonly MaskedMap _map;

	    public MaskedMap Map {
		    get { return _map;  }
	    }

	    public PublicWindow() {
            InitializeComponent();

			_map = new MaskedMap(true) {
				ParentWindow = this,
			};

			MapPresenterPublic1.Content = _map.CanvasMapMask;
			MapPresenterPublic2.Content = _map.CanvasOverlay;
        }

	    public void MaximizeToSecondaryMonitor() {
		    var screen2 = Screen.AllScreens.FirstOrDefault(screen => screen.Primary == false);
		    if (screen2 == null) {
			    return;
		    }

		    var rect = screen2.WorkingArea;
			Top = rect.Top;
		    Left = rect.Left;
		    Width = rect.Width;
		    Height = rect.Height;
		    if (IsLoaded) {
			    WindowState = WindowState.Maximized;
		    }
	    }


		private void PublicWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			e.Cancel = true;
			Visibility = Visibility.Hidden;
		} 

    }
}
