using Configuration;
using System.Globalization;
using System.Threading;
using System.Windows;

namespace DCMarker
{
    /// <summary>
    /// Interaction logic for ManualMainWindow.xaml
    /// </summary>
    public partial class ManualMainWindow : Window
    {
        private MainViewModel mainViewModel;

        public ManualMainWindow()
        {

            DCConfig cfg = DCConfig.Instance;
            string language = cfg.GuiLanguage;
            if (!string.IsNullOrWhiteSpace(language))
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(language);
            }
            InitializeComponent();
            mainViewModel = new MainViewModel();
            DataContext = mainViewModel;
        }

        private void InitViewModel()
        {
            mainViewModel.ArticleNumber = "123456789012";
            mainViewModel.Kant = "1";
            mainViewModel.HasKant = true;
            mainViewModel.Fixture = "999888777555";
            mainViewModel.HasFixture = true;
            mainViewModel.Status = "OffLine";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mainViewModel.Abort();
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            mainViewModel.Test();
        }

        private void ResetSignals_Click(object sender, RoutedEventArgs e)
        {
            mainViewModel.ErrorMessage = string.Empty;
            mainViewModel.ResetAllIoSignals();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnTop_Click(object sender, RoutedEventArgs e)
        {
            Topmost = !Topmost;
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            WPFAboutBox1 dlg = new WPFAboutBox1(this);
            dlg.ShowDialog();
        }
    }
}
