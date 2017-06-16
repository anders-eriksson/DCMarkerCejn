using Configuration;
using DCAdmin.ViewModel;
using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
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
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo(language);
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(language);
            }

            db = new DB();

            InitializeComponent();
            SaveChangesPopup.IsOpen = false;
            //ChangesSavedTextblock.Visibility = Visibility.Hidden;

            if (DCConfig.Instance.ClearClipboard)
            {
                Clipboard.Clear();
            }

            //// Init all datagrid view models
            //InitializeViewModels();

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
            InitLaserDataViewModel();
            InitQuarterCodeViewModel();
            InitWeekCodeViewModel();
            InitFixtureViewModel();
        }

        private void InitFixtureViewModel()
        {
            this.FixtureRoot.DataContext = null;
            fixtureVM = new FixtureViewModel();
            fixtureVM.RefreshDatabase(false);
            FixtureRoot.DataContext = fixtureVM;
            FixtureGrid.fixtureDataGrid.Items.Refresh();
            FixtureGrid.fixtureDataGrid.Focus();
        }

        private void InitWeekCodeViewModel()
        {
            this.WeekCodeRoot.DataContext = null;
            weekVM = new WeekCodeViewModel();
            weekVM.EventColorEvent += EventColorEvent;
            weekVM.SaveChangesEvent += SaveChangesEvent;
            weekVM.RefreshDatabase(false);
            WeekCodeRoot.DataContext = weekVM;
            WeekCodeGrid.weekCodeDataGrid.Items.Refresh();
            WeekCodeGrid.weekCodeDataGrid.Focus();
        }

        private void InitQuarterCodeViewModel()
        {
            this.QuarterCodeRoot.DataContext = null;
            quarterVM = new QuarterCodeViewModel();
            quarterVM.EventColorEvent += EventColorEvent;
            quarterVM.SaveChangesEvent += SaveChangesEvent;
            quarterVM.RefreshDatabase(false);
            QuarterCodeRoot.DataContext = quarterVM;
            QuarterCodeGrid.quarterCodeDataGrid.Items.Refresh();
            QuarterCodeGrid.quarterCodeDataGrid.Focus();
        }

        private void InitLaserDataViewModel()
        {
            this.LaserDataRoot.DataContext = null;
            laserVM = new LaserDataViewModel();
            laserVM.EventColorEvent += EventColorEvent;
            laserVM.SaveChangesEvent += SaveChangesEvent;
            laserVM.RefreshDatabase(false);
            LaserDataRoot.DataContext = laserVM;
            LaserDataGrid.laserDataDataGrid.Items.Refresh();
            LaserDataGrid.laserDataDataGrid.Focus();
        }

        private void SaveChangesEvent()
        {
            DisplaySavedPopup();
        }

        private void EventColorEvent(Color color)
        {
            EditMode.Fill = new SolidColorBrush(color);
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
            var dlg = new WPFAboutBox(this);
            dlg.ShowDialog();
        }

        private void AddNewRecord(int index)
        {
            switch (index)
            {
                case (int)ViewModelEnum.LaserDataViewModel:
                    {
                        AddRowWindow dlg = new AddRowWindow()
                        {
                            Owner = Window.GetWindow(this)
                        };
                        bool? rc = dlg.ShowDialog();

                        LaserDataGrid.laserDataDataGrid.Focus();
                        if (rc.HasValue && rc.Value)
                        {
                            object item = laserVM.AddRow(dlg.RowData.MachineId, dlg.RowData.Article, dlg.RowData.Kant);

                            laserVM.TriggerSelectedRow();
                        }
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
                        AddFixture dlg = new AddFixture()
                        {
                            Owner = Window.GetWindow(this)
                        };
                        bool? rc = dlg.ShowDialog();
                        FixtureGrid.fixtureDataGrid.Focus();
                        if (rc.HasValue && rc.Value)
                        {
                            object item = fixtureVM.AddRow(dlg.FixtureData.FixtureId);

                            fixtureVM.TriggerSelectedRow();
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        private void AddNewRecordFromSelected(int index)
        {
            switch (index)
            {
                case (int)ViewModelEnum.LaserDataViewModel:
                    {
                        AddRowWindow dlg = new AddRowWindow()
                        {
                            Owner = Window.GetWindow(this)
                        };
                        bool? rc = dlg.ShowDialog();

                        LaserDataGrid.laserDataDataGrid.Focus();
                        if (rc.HasValue && rc.Value)
                        {
                            object item = laserVM.AddRowFromSelected(dlg.RowData.MachineId, dlg.RowData.Article, dlg.RowData.Kant);

                            laserVM.TriggerSelectedRow();
                        }
                        break;
                    }
                case (int)ViewModelEnum.WeekCodeViewModel:
                    {
                        //weekVM.AddNewRecord();
                        break;
                    }
                case (int)ViewModelEnum.QuarterCodeViewModel:
                    {
                        //quarterVM.AddNewRecord();
                        quarterVM.AddRowFromSelected();
                        quarterVM.TriggerSelectedRow();
                        break;
                    }
                case (int)ViewModelEnum.FixtureViewModel:
                    {
                        AddFixture dlg = new AddFixture()
                        {
                            Owner = Window.GetWindow(this)
                        };
                        bool? rc = dlg.ShowDialog();
                        FixtureGrid.fixtureDataGrid.Focus();
                        if (rc.HasValue && rc.Value)
                        {
                            object item = fixtureVM.AddRowFromSelected(dlg.FixtureData.FixtureId);

                            fixtureVM.TriggerSelectedRow();
                        }
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
            int index = tcControl.SelectedIndex;
            DeleteSelectedRecord(index);
        }

        private void DeleteSelectedRecord(int index)
        {
            switch (index)
            {
                case (int)ViewModelEnum.LaserDataViewModel:
                    {
                        ConfirmationDeleteRowWindow dlg = new ConfirmationDeleteRowWindow()
                        {
                            Owner = Window.GetWindow(this)
                        };
                        var currentItem = laserVM.SelectedLaserDataRow;

                        // TODO: MachineId
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
                            laserVM.DeleteSelectedRecord();
                        }
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

                        LaserDataRoot.DataContext = null;
                        LaserDataRoot.DataContext = laserVM;
                        LaserDataGrid.laserDataDataGrid.Items.Refresh();

                        var item = laserVM.SelectedLaserDataRow;
                        DataGridColumn column = this.LaserDataGrid.laserDataDataGrid.Columns[1];
                        LaserDataGrid.laserDataDataGrid.ScrollIntoView(item, column);
                        LaserDataGrid.laserDataDataGrid.SelectedItem = item;
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
                        //if (IsChangesPending())
                        //{
                        //    laserVM.SaveChanges();
                        //}

                        // TODO figure out why it takes 2
                        LaserDataGrid.laserDataDataGrid.CommitEdit();
                        LaserDataGrid.laserDataDataGrid.CommitEdit();
                        break;
                    }
                case (int)ViewModelEnum.WeekCodeViewModel:
                    {
                        //if (IsChangesPending())
                        //{
                        //    weekVM.SaveChanges();
                        //}

                        // TODO figure out why it takes 2
                        WeekCodeGrid.weekCodeDataGrid.CommitEdit();
                        WeekCodeGrid.weekCodeDataGrid.CommitEdit();
                        break;
                    }
                case (int)ViewModelEnum.QuarterCodeViewModel:
                    {
                        //if (IsChangesPending())
                        //{
                        //    quarterVM.SaveChanges();
                        //}

                        // TODO figure out why it takes 2
                        QuarterCodeGrid.quarterCodeDataGrid.CommitEdit();
                        QuarterCodeGrid.quarterCodeDataGrid.CommitEdit();
                        break;
                    }
                case (int)ViewModelEnum.FixtureViewModel:
                    {
                        //if (IsChangesPending())
                        //{
                        //    fixtureVM.SaveChanges();
                        //}

                        // TODO figure out why it takes 2
                        FixtureGrid.fixtureDataGrid.CommitEdit();
                        FixtureGrid.fixtureDataGrid.CommitEdit();
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
            //ChangesSavedTextblock.Visibility = Visibility.Visible;
            var timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(1000)
            };

            timer.Tick += delegate (object sender, EventArgs e)
            {
                ((DispatcherTimer)timer).Stop();
                if (SaveChangesPopup.IsOpen) SaveChangesPopup.IsOpen = false;
                // ChangesSavedTextblock.Visibility = Visibility.Hidden;
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
            int index = tcControl.SelectedIndex;
            AddNewRecordFromSelected(index);
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

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            // Init all datagrid view models
            InitializeViewModels();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}