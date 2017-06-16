using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DCLog;

namespace DCAdmin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            Log.Error(ex, "UnhandledException caught : " + ex.Message);
            Log.Fatal(string.Format("Runtime terminating: {0}", e.IsTerminating));
            MessageBox.Show("Unhandled Exception, see log file for more information! Aborting!", "FATAL ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            SplashScreen splash = new SplashScreen("./LoadingDatabase.png");
            splash.Show(autoClose: true, topMost: true);
        }
    }
}