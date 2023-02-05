using System.Windows;
using System.Windows.Threading;
using log4net;
using MapViewer.Utilities;

namespace MapViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App {

        public App() {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            // Prevent default unhandled exception processing
            MessageBox.Show($"An error occurred: {e.Exception.Message}", "Error");
            Log.Error(e.Exception.Message, e.Exception);
            
            e.Handled = true;
        }
    }
}
