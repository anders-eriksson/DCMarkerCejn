using Configuration;
using DCLog;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
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
                Log.Info(string.Format("DCMarker Cejn - Version: {0}", GetApplicationVersion()));
                InitLanguage();
                InitializeComponent();

                mainViewModel = new NippleMainViewModel();
                mainViewModel.SetFocusToNumberEvent += MainViewModel_SetFocusToNumberEvent;
                DataContext = mainViewModel;

#if DEBUG
                if (!DCConfig.Instance.Debug)
                {
                    ArtNoTextbox.Visibility = Visibility.Hidden;
                    ArtNoButton.Visibility = Visibility.Hidden;
                    StartOkButton.Visibility = Visibility.Hidden;
                    ExecuteButton.Visibility = Visibility.Hidden;
                    Execute2Button.Visibility = Visibility.Hidden;
                }
#else
                ArtNoTextbox.Visibility = Visibility.Hidden;
                ArtNoButton.Visibility = Visibility.Hidden;
                StartOkButton.Visibility = Visibility.Hidden;
                ExecuteButton.Visibility = Visibility.Hidden;
                Execute2Button.Visibility = Visibility.Hidden;

#endif
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

        private string GetApplicationVersion()
        {
            string result = string.Empty;

            System.Reflection.Assembly executingAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            var fieVersionInfo = FileVersionInfo.GetVersionInfo(executingAssembly.Location);
            result = fieVersionInfo.FileVersion;

            return result;
        }

        private static void InitLanguage()
        {
            string language = DCConfig.Instance.GuiLanguage;
            Log.Debug(string.Format("GUI Language: {0}", language));
            if (!string.IsNullOrWhiteSpace(language))
            {
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo(language);
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(language);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //mainViewModel = new NippleMainViewModel();
            //DataContext = mainViewModel;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mainViewModel.Abort();
            Log.Shutdown();
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
            string logfile = Log.GetLogFileName("dclogfile");

            Process.Start(Path.GetDirectoryName(logfile));
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            WPFAboutBox1 dlg = new WPFAboutBox1(this);
            dlg.ShowDialog();
        }

#if DEBUG

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            mainViewModel.Test();
        }

#endif

        private void Window_ContentRendered(object sender, System.EventArgs e)
        {
            mainViewModel.InitializeViewModel();
        }

        private void MainViewModel_SetFocusToNumberEvent(object sender, Contracts.SetFocusToNumberArgs e)
        {
            Log.Trace("SetFocusTONumberEvent");

            Dispatcher.BeginInvoke(DispatcherPriority.Input,
            new Action(delegate ()
            {
                this.Activate();               // Set mainWindow focus
                TOnrTextbox.Focus();         // Set Logical Focus
                Keyboard.Focus(TOnrTextbox); // Set Keyboard Focus
            }));
        }

#if DEBUG

        private void ArtNoButton_Click(object sender, RoutedEventArgs e)
        {
            // Update ArticleNumber explicit
            //BindingExpression be = ArtNoTextbox.GetBindingExpression(TextBox.TextProperty);
            //be.UpdateSource();

            mainViewModel.ArtNo(ArtNoTextbox.Text.Trim());
        }

        private void StartOkButton_Click(object sender, RoutedEventArgs e)
        {
            mainViewModel.StartOk();
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            mainViewModel.Execute();
        }

        private void Execute2Button_Click(object sender, RoutedEventArgs e)
        {
            mainViewModel.Execute2();
        }

#else

        private void ArtNoButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void StartOkButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Execute2Button_Click(object sender, RoutedEventArgs e)
        {
        }

#endif
    }
}