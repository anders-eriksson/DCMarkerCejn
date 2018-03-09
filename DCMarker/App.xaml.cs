using Configuration;
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
                case 2:
                    wnd = new MainWindow();
                    break;

                case 3:
                    wnd = new ManualMainWindow();
                    break;

                default:
                    wnd = new ErrorMainWindow();
                    break;
            }

            wnd.Show();
        }
    }
}