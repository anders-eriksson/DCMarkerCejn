﻿using Configuration;
using System.Windows;

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

            DCConfig cfg = DCConfig.Instance;
            switch (cfg.TypeOfMachine)
            {
                case 1:
                    wnd = new MainWindow();
                    break;

                //case 2:
                //    wnd = new ManualMainWindow();
                //    break;

                default:
                    wnd = new ErrorMainWindow();
                    break;
            }
            // Create the startup window

            // Show the window
            if (wnd != null)
            {
                wnd.Show();
            }
        }
    }
}