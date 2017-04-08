using Configuration;
using DCAdmin.ViewModel;
using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace DCAdmin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DB db;
        private FixtureViewModel fixtureVM;
        private LaserDataViewModel laserVM;
        private QuarterCodeViewModel quarterVM;
        private WeekCodeViewModel weekVM;

        public MainWindow()
        {
            string language = DCConfig.Instance.GuiLanguage;
            if (!string.IsNullOrWhiteSpace(language))
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(language);
            }

            db = new DB();

            InitializeComponent();
            SaveChangesPopup.IsOpen = false;

            // TODO: is this a good idea?
            // we make configurable
            if (DCConfig.Instance.ClearClipboard)
            {
                Clipboard.Clear();
            }

            // Init all datagrid view models
            InitializeViewModels();

            // init save and restore window position.
            Services.Tracker.Configure(this)//the object to track
                                           .IdentifyAs("main window")                                                                           //a string by which to identify the target object
                                           .AddProperties<Window>(w => w.Height, w => w.Width, w => w.Top, w => w.Left, w => w.WindowState)     //properties to track
                                           .RegisterPersistTrigger(nameof(SizeChanged))                                                         //when to persist data to the store
                                           .Apply();                                                                                            //apply any previously stored data

            ErrorMsg = "";
        }

        private void InitializeViewModels()
        {
            this.LaserDataRoot.DataContext = null;
            this.QuarterCodeRoot.DataContext = null;
            this.WeekCodeRoot.DataContext = null;
            this.FixtureRoot.DataContext = null;

            laserVM = new LaserDataViewModel();
            quarterVM = new QuarterCodeViewModel();
            weekVM = new WeekCodeViewModel();
            fixtureVM = new FixtureViewModel();

            this.LaserDataRoot.DataContext = laserVM;
            this.QuarterCodeRoot.DataContext = quarterVM;
            this.WeekCodeRoot.DataContext = weekVM;
            this.FixtureRoot.DataContext = fixtureVM;
        }

        public static double MaxScreenSize
        {
            get
            {
                return System.Windows.SystemParameters.PrimaryScreenWidth;
            }
        }

        public string ErrorMsg { get; set; }

        private static void SetColumnWidthToCell(DataGrid dgrid)
        {
            var columns = dgrid.Columns;
            foreach (var column in columns)
            {
                column.Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new WPFAboutBox1(this);
            dlg.ShowDialog();
        }

        private void AddNewRecord(int index)
        {
            switch (index)
            {
                case (int)ViewModelEnum.LaserDataViewModel:
                    {
                        laserVM.AddNewRecord();
                        break;
                    }
                case (int)ViewModelEnum.WeekCodeViewModel:
                    {
                        weekVM.AddNewRecord();
                        break;
                    }
                case (int)ViewModelEnum.QuarterCodeViewModel:
                    {
                        quarterVM.AddNewRecord();
                        break;
                    }
                case (int)ViewModelEnum.FixtureViewModel:
                    {
                        fixtureVM.AddNewRecord();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationDeleteRow dlg = new ConfirmationDeleteRow();
            var currentItem = laserVM.SelectedLaserDataRow;
            string machineId = "AME";
#if MACHINEID
            string machineId = currentItem.MachineId;
#endif
            string article = currentItem.F1;
            string kant = currentItem.Kant;

            dlg.InitValues(machineId, article, kant);

            bool? rc = dlg.ShowDialog();

            if (rc.HasValue && rc.Value)
            {
                int index = tcControl.SelectedIndex;
                DeleteSelectedRecord(index);
            }
        }

        private void DeleteSelectedRecord(int index)
        {
            switch (index)
            {
                case (int)ViewModelEnum.LaserDataViewModel:
                    {
                        laserVM.DeleteSelectedRecord();
                        break;
                    }
                case (int)ViewModelEnum.WeekCodeViewModel:
                    {
                        weekVM.DeleteSelectedRecord();
                        break;
                    }
                case (int)ViewModelEnum.QuarterCodeViewModel:
                    {
                        quarterVM.DeleteSelectedRecord();
                        break;
                    }
                case (int)ViewModelEnum.FixtureViewModel:
                    {
                        fixtureVM.DeleteSelectedRecord();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            int index = tcControl.SelectedIndex;
            AddNewRecord(index);
        }

        private void OnTop_Click(object sender, RoutedEventArgs e)
        {
            this.Topmost = !this.Topmost;
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            int index = tcControl.SelectedIndex;
            RefreshDatabase(index);
        }

        private void RefreshDatabase(int index)
        {
            switch (index)
            {
                case (int)ViewModelEnum.LaserDataViewModel:
                    {
                        var saveChanges = IsChangesPending();
                        laserVM.RefreshDatabase(saveChanges);
                        break;
                    }
                case (int)ViewModelEnum.WeekCodeViewModel:
                    {
                        var saveChanges = IsChangesPending();
                        weekVM.RefreshDatabase(saveChanges);
                        break;
                    }
                case (int)ViewModelEnum.QuarterCodeViewModel:
                    {
                        var saveChanges = IsChangesPending();
                        quarterVM.RefreshDatabase(saveChanges);
                        break;
                    }
                case (int)ViewModelEnum.FixtureViewModel:
                    {
                        var saveChanges = IsChangesPending();
                        fixtureVM.RefreshDatabase(saveChanges);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            //db.Refresh();
            //InitializeViewModels();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            int index = tcControl.SelectedIndex;
            SaveChanges(index);
            DisplaySavedPopup();
        }

        private void SaveChanges(int index)
        {
            switch (index)
            {
                case (int)ViewModelEnum.LaserDataViewModel:
                    {
                        if (IsChangesPending())
                        {
                            laserVM.SaveChanges();
                        }
                        break;
                    }
                case (int)ViewModelEnum.WeekCodeViewModel:
                    {
                        if (IsChangesPending())
                        {
                            weekVM.SaveChanges();
                        }
                        break;
                    }
                case (int)ViewModelEnum.QuarterCodeViewModel:
                    {
                        if (IsChangesPending())
                        {
                            quarterVM.SaveChanges();
                        }
                        break;
                    }
                case (int)ViewModelEnum.FixtureViewModel:
                    {
                        if (IsChangesPending())
                        {
                            fixtureVM.SaveChanges();
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        private void DisplaySavedPopup()
        {
            SaveChangesPopup.IsOpen = true;
            var timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(1000)
            };

            timer.Tick += delegate (object sender, EventArgs e)
            {
                ((DispatcherTimer)timer).Stop();
                if (SaveChangesPopup.IsOpen) SaveChangesPopup.IsOpen = false;
            };

            timer.Start();
        }

        private bool IsChangesPending()
        {
            bool result = false;
            var pending = db.IsChangesPending();

            if (db.IsChangesPending())
            {
                var mbResult = MessageBox.Show("Changes exists! Do you want to save them?", "Pending Changes", MessageBoxButton.YesNo);
                if (mbResult == MessageBoxResult.No)
                {
                    result = false;
                }

                if (mbResult == MessageBoxResult.Yes)
                {
                    result = true;
                }
            }

            return result;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (db.IsChangesPending())
            {
                var result = MessageBox.Show("Changes exists! Do you want to save them?", "Pending Changes", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (System.Exception ex)
                    {
                        e.Cancel = true;
                        //ErrorMessage.Text = GetFirstExceptionMessage(ex);
                    }
                }
            }
            db.Dispose();
        }

        private void NewFromSelected_Click(object sender, RoutedEventArgs e)
        {
        }

        private void CopyCommandBinding_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CopyCommandBinding_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            int index = tcControl.SelectedIndex;
            switch (index)
            {
                case (int)ViewModelEnum.LaserDataViewModel:
                    Clipboard.SetText(LaserDataGrid.laserDataDataGrid.SelectedItem.ToString());
                    break;

                case (int)ViewModelEnum.QuarterCodeViewModel:
                    Clipboard.SetText(QuarterCodeGrid.quarterCodeDataGrid.SelectedItem.ToString());
                    break;

                case (int)ViewModelEnum.FixtureViewModel:
                    Clipboard.SetText(FixtureGrid.fixtureDataGrid.SelectedItem.ToString());
                    break;

                case (int)ViewModelEnum.WeekCodeViewModel:
                    Clipboard.SetText(WeekCodeGrid.weekCodeDataGrid.SelectedItem.ToString());
                    break;

                default:
                    break;
            }
        }

        private void CutCommandBinding_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CutCommandBinding_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            int index = tcControl.SelectedIndex;
            switch (index)
            {
                case (int)ViewModelEnum.LaserDataViewModel:
                    Clipboard.SetText(LaserDataGrid.laserDataDataGrid.SelectedItem.ToString());
                    break;

                case (int)ViewModelEnum.QuarterCodeViewModel:
                    Clipboard.SetText(QuarterCodeGrid.quarterCodeDataGrid.SelectedItem.ToString());
                    break;

                case (int)ViewModelEnum.FixtureViewModel:
                    Clipboard.SetText(FixtureGrid.fixtureDataGrid.SelectedItem.ToString());
                    break;

                case (int)ViewModelEnum.WeekCodeViewModel:
                    Clipboard.SetText(WeekCodeGrid.weekCodeDataGrid.SelectedItem.ToString());
                    break;

                default:
                    break;
            }
        }

        private void PasteCommandBinding_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            if (Clipboard.ContainsText(TextDataFormat.UnicodeText))
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
        }
    }
}