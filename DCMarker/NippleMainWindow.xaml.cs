using Configuration;
using DCLog;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
using GlblRes = global::DCMarker.Properties.Resources;

namespace DCMarker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class NippleMainWindow : Window
    {
        private NippleMainViewModel mainViewModel;

        public NippleMainWindow()
        {
            try
            {
                DCConfig cfg = DCConfig.Instance;
                string language = cfg.GuiLanguage;
                Log.Debug(string.Format("GUI Language: {0}", language));
                if (!string.IsNullOrWhiteSpace(language))
                {
                    CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo(language);
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(language);
                }
                InitializeComponent();

                if (!DCConfig.Instance.Debug)
                {
                    TestButton.Visibility = Visibility.Hidden;
                    ExecuteButton.Visibility = Visibility.Hidden;
                }
                Services.Tracker.Configure(this)//the object to track
                                               .IdentifyAs("MainWindow")                                                                           //a string by which to identify the target object
                                               .AddProperties<Window>(w => w.Height, w => w.Width, w => w.Top, w => w.Left, w => w.WindowState)     //properties to track
                                               .RegisterPersistTrigger(nameof(SizeChanged))                                                         //when to persist data to the store
                                               .Apply();                                                                                            //apply any previously stored data
            }
            catch (System.Exception ex)
            {
                Log.Fatal(ex, "Error creating NippleMainWindow");
                MessageBox.Show(GlblRes.Error_Creating_MainWindow_Aborting);
                this.Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mainViewModel = new NippleMainViewModel();
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

        private void LogFiles_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(@"c:\ProgramData\DCLasersystem\DCMarker\Logs");
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            WPFAboutBox1 dlg = new WPFAboutBox1(this);
            dlg.ShowDialog();
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            mainViewModel.Test();
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            mainViewModel.Execute();
        }
    }
}