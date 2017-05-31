using Configuration;
using DCLog;
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
        private ManualMainViewModel mainViewModel;

        public ManualMainWindow()
        {
            DCConfig cfg = DCConfig.Instance;
            string language = cfg.GuiLanguage;
            if (!string.IsNullOrWhiteSpace(language))
            {
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo(language);
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(language);
            }
            InitializeComponent();
            if (!DCConfig.Instance.Debug)
            {
                LoadButton.Visibility = Visibility.Hidden;
                ExecuteButton.Visibility = Visibility.Hidden;
            }
            mainViewModel = new ManualMainViewModel();
            DataContext = mainViewModel;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mainViewModel.Abort();
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ArticleTextBox.Focus();
            ArticleTextBox.SelectAll();
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            mainViewModel.Test();
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            Log.Trace("ExecuteButton_Click");
            mainViewModel.Execute();
        }
    }
}