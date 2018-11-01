using Configuration;
using DCLog;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using GlblRes = global::DCMarker.Properties.Resources;

namespace DCMarker
{
    /// <summary>
    /// Interaction logic for ManualMainWindow.xaml
    /// </summary>
    public partial class FlexibleMainWindow : Window
    {
        private FlexibleMainViewModel mainViewModel;

        public FlexibleMainWindow()
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
            mainViewModel = new FlexibleMainViewModel();
            mainViewModel.FocusEvent += MainViewModel_FocusEvent;
            DataContext = mainViewModel;
        }

        private void MainViewModel_FocusEvent(object sender, Contracts.FocusEventArgs e)
        {
            if (e.Text == "TO-Number")
            {
                Log.Trace("FocusEvent");

                Dispatcher.BeginInvoke(DispatcherPriority.Input,
                new Action(delegate ()
                {
                    this.Activate();               // Set mainWindow focus
                    TOnrTextbox.Focus();         // Set Logical Focus
                    Keyboard.Focus(TOnrTextbox); // Set Keyboard Focus
                }));
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // We alway save the quantity, but only load it if it's configured
            var q = mainViewModel.Quantity;
            Properties.Settings.Default.Quantity = q;
            Properties.Settings.Default.Save();

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

#if DEBUG

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            Log.Trace("ExecuteButton_Click");
            mainViewModel.Execute();
        }

#else

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
        }

#endif

        private void ResetZaxis_Click(object sender, RoutedEventArgs e)
        {
            bool brc = mainViewModel.ResetZAxis();
            if (!brc)
            {
                MessageBox.Show(GlblRes.No_Connection_with_Z_axis, GlblRes.ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogPath_Click(object sender, RoutedEventArgs e)
        {
            string LogfilePath = NLogHelper.GetLogFileDirectory("dclogfile");
            Process.Start(LogfilePath);
        }
    }
}