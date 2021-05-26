using Configuration;
using System;
using System.Windows;
using DCLog;
using System.Windows.Threading;

namespace DCMarker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Window wnd = null;

            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Dispatcher.UnhandledException += Dispatcher_UnhandledException;

            DCConfig cfg = DCConfig.Instance;
            switch (cfg.TypeOfMachine)
            {
                case 1:
                case 2:
                    wnd = new MainWindow();
                    break;

                case 3:
                    wnd = new ManualMainWindow();
                    break;

                case 4:
                    wnd = new NippleMainWindow();
                    break;

                case 5:
                    wnd = new FlexibleMainWindow();
                    break;

                case 6:
                    wnd = new LargeFlexibleMainWindow();
                    break;

                case 7:
                    wnd = new Co208MainWindow();
                    break;

                default:
                    wnd = new ErrorMainWindow();
                    break;
            }

            wnd.Show();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            Log.Error(ex, "UnhandledException caught : " + ex.Message);
            Log.Fatal(string.Format("Runtime terminating: {0}", e.IsTerminating));
            MessageBox.Show("Unhandled Exception, see log file for more information! Aborting!", "FATAL ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Process unhandled exception
            var ex = (Exception)e.Exception;
            Log.Error(ex, "UnhandledException caught : " + ex.Message);
            MessageBox.Show("Unhandled Exception, see log file for more information! Aborting!", "FATAL ERROR", MessageBoxButton.OK, MessageBoxImage.Error);

            // Prevent default unhandled exception processing
            e.Handled = true;
        }

        //protected override void OnStartup(StartupEventArgs e)
        //{
        //    SplashScreen splash = new SplashScreen("./LoadingDatabase.png");
        //    splash.Show(autoClose: true, topMost: true);
        //}
    }
}