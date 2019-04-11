using AlphaChiTech.Virtualization;
using Configuration;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Threading;

namespace DCHistory
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainViewModel mainVM;
        private DCConfig cfg;

        public MainWindow()
        {
            cfg = DCConfig.Instance;
            SetLocalization();
            mainVM = new MainViewModel();
            InitializeComponent();
            StartDatePicker.DisplayDate = DateTime.Now;
            EndDatePicker.DisplayDate = DateTime.Now;
            this.DataContext = mainVM;
#if VIRTUALIZATION
            //this routine only needs to run once, so first check to make sure the
            //VirtualizationManager isn’t already initialized
            if (!VirtualizationManager.IsInitialized)
            {
                //set the VirtualizationManager’s UIThreadExcecuteAction. In this case
                //we’re using Dispatcher.Invoke to give the VirtualizationManager access
                //to the dispatcher thread, and using a DispatcherTimer to run the background
                //operations the VirtualizationManager needs to run to reclaim pages and manage memory.
                VirtualizationManager.Instance.UIThreadExcecuteAction =
                    (a) => Dispatcher.Invoke(a);
                new DispatcherTimer(
                    TimeSpan.FromSeconds(1),
                    DispatcherPriority.Background,
                    delegate (object s, EventArgs a)
                    {
                        VirtualizationManager.Instance.ProcessActions();
                    },
                    this.Dispatcher).Start();
            }
#endif
            Services.Tracker.Configure(this)//the object to track
                                          .IdentifyAs("main window")                                                                           //a string by which to identify the target object
                                          .AddProperties<Window>(w => w.Height, w => w.Width, w => w.Top, w => w.Left, w => w.WindowState)     //properties to track
                                          .RegisterPersistTrigger(nameof(SizeChanged))                                                         //when to persist data to the store
                                          .Apply();                                                                                            //apply any previously stored data
        }

        private static void SetLocalization()
        {
            var cfg = DCConfig.Instance;
            var language = cfg.GuiLanguage;
            if (!string.IsNullOrWhiteSpace(language))
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(language);

                // set culture for xaml...
                FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(language)));
            }
        }

        private void OnTop_Click(object sender, RoutedEventArgs e)
        {
            this.Topmost = !this.Topmost;
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new WPFAboutBox1(this);
            dlg.ShowDialog();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            mainVM.GetAllHistoryData();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mainVM.GetAllHistoryData();
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            SearchError.Text = string.Empty;
            var selectedRow = mainVM.FindArticleAndScrollIntoView(FindTextbox.Text.Trim());
            if (selectedRow != null)
            {
                HistoryGrid.Focus();
                HistoryGrid.ScrollToCenterOfView(selectedRow);
            }
            else
            {
                SearchError.Text = "Can't find Article number";
            }
        }

        private void FindArticleAndScrollIntoView()
        {
            var entity = mainVM.FindSerialNumber(FindTextbox.Text);
            ScrollToView(entity);
        }

        private void ScrollToView(Model.HistoryData entity)
        {
            if (entity != null)
            {
                SearchError.Text = "";
                HistoryGrid.SelectedItem = entity;
                HistoryGrid.ScrollToCenterOfView(entity);
            }
            else
            {
                SearchError.Text = "Can't find Article number!";
            }
        }

        private void SearchArticleNumber_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                e.Handled = true;
                FindArticleAndScrollIntoView();
            }
        }

        private void SearchArticleNumber_LostFocus(object sender, RoutedEventArgs e)
        {
            SearchError.Text = "";
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            SetColumnWidthToCell(HistoryGrid);
        }

        private static void SetColumnWidthToCell(DataGrid dgrid)
        {
            var columns = dgrid.Columns;
            foreach (var column in columns)
            {
                column.Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
            }
        }

        private void FilterDateButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime? start = StartDatePicker.SelectedDate;
            DateTime? end = EndDatePicker.SelectedDate;

            if (start.HasValue && end.HasValue)
            {
                GetFilteredDate(start, end);
            }
            else if (start.HasValue)
            {
                DateTime? tmp = DateTime.Now;
                GetFilteredDate(start, tmp);
            }
            else if (end.HasValue)
            {
                DateTime? tmp = DateTime.MinValue;
                GetFilteredDate(tmp, end);
            }
            else
            {
                mainVM.GetAllHistoryData();
            }
        }

        private void GetFilteredDate(DateTime? start, DateTime? end)
        {
            mainVM.ExecuteFilterDate(start.Value, end.Value);
        }

        private void ClearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            StartDatePicker.SelectedDate = null;
            EndDatePicker.SelectedDate = null;
            mainVM.FilterKey = string.Empty;
            mainVM.FilterValue = string.Empty;
        }

        private void LogDirectory_Click(object sender, RoutedEventArgs e)
        {
            string logfileDirectory = GetLogDirectoryPath();
            System.Diagnostics.Process.Start(logfileDirectory);
        }

        private static string GetLogDirectoryPath()
        {
            var result = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DCLasersystem\\DCHistory\\Logs");

            return result;
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            SplashScreen splash = new SplashScreen("./LoadingDatabase.png");
            splash.Show(autoClose: true, topMost: true);

            ToggleButton tmp = (ToggleButton)sender;
            mainVM.ErrorMessage = string.Empty;
            if (tmp.IsChecked.Value)
            {
                if (mainVM.HasFilterType == FilterType.Text)
                {
                    if (!string.IsNullOrWhiteSpace(mainVM.FilterKey))
                    {
                        mainVM.ExecuteFilter();
                    }
                    else
                    {
                        tmp.IsChecked = false;
                        mainVM.ErrorMessage = "You must select a column to filter on!";
                        return;
                    }
                }
                else if (mainVM.HasFilterType == FilterType.Bool)
                {
                    mainVM.ExecuteFilterBool();
                }
                else if (mainVM.HasFilterType == FilterType.Date)
                {
                    DateTime? start = StartDatePicker.SelectedDate;
                    DateTime? end = EndDatePicker.SelectedDate;
                    if (start > end)
                    {
                        tmp.IsChecked = false;
                        mainVM.ErrorMessage = "End date must be later than or equal to start date";
                        return;
                    }
                    if (start.HasValue && end.HasValue)
                    {
                        GetFilteredDate(start, end);
                    }
                    else if (start.HasValue)
                    {
                        DateTime? tmpDate = DateTime.Now;
                        GetFilteredDate(start, tmpDate);
                    }
                    else if (end.HasValue)
                    {
                        DateTime? tmpDate = DateTime.MinValue;
                        GetFilteredDate(tmpDate, end);
                    }
                    else
                    {
                        tmp.IsChecked = false;
                        mainVM.ErrorMessage = "You must enter a date!";
                        return;
                    }
                }

                HistoryGrid.DataContext = null;
                HistoryGrid.DataContext = mainVM;
                HistoryGrid.Items.Refresh();
            }
            else
            {
                mainVM.ExecuteNoFilter();
                HistoryGrid.DataContext = null;
                HistoryGrid.DataContext = mainVM;
                HistoryGrid.Items.Refresh();
            }
        }

        private void FilterCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilterCombobox != null && FilterCombobox.SelectedItem != null)
            {
                string key = FilterCombobox.SelectedItem.ToString();
                if (key == "Issued")
                {
                    mainVM.HasFilterType = FilterType.Date;
                }
                else if (key == "Careful" || key == "ExternTest" || key == "EnableTO")
                {
                    mainVM.HasFilterType = FilterType.Bool;
                }
                else
                {
                    mainVM.HasFilterType = FilterType.Text;
                }
                mainVM.FilterKey = key;
            }
        }
    }
}