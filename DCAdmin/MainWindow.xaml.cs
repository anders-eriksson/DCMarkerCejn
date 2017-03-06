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
            int index = tcControl.SelectedIndex;
            DeleteSelectedRecord(index);
        }

        private void DeleteSelectedRecord(int index)
        {
            // NB   All ViewModel SaveChanges will save all the changes in all the tables!
            //      Calling in viewmodel will make it easier to handle errors...
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

        //private void laserDataDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        //{
        //    DataGrid dataGrid = sender as DataGrid;
        //    if (e.EditAction == DataGridEditAction.Commit)
        //    {
        //        ListCollectionView view = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource) as ListCollectionView;
        //        if (view.IsAddingNew || view.IsEditingItem)
        //        {
        //            this.Dispatcher.BeginInvoke(new DispatcherOperationCallback(param =>
        //            {
        //                // This callback will be called after the CollectionView
        //                // has pushed the changes back to the DataGrid.ItemSource.

        //                if (db.IsChangesPending())
        //                {
        //                    try
        //                    {
        //                        db.SaveChanges();
        //                    }
        //                    catch (System.Exception ex)
        //                    {
        //                        e.Cancel = true;
        //                        ErrorMessage.Text = GetFirstExceptionMessage(ex);
        //                    }
        //                }
        //                return null;
        //            }), DispatcherPriority.Background, new object[] { null });
        //        }
        //    }
        //}

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
            RefreshDatabase();
        }

        private void RefreshDatabase()
        {
            db.Refresh();
            InitializeViewModels();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            int index = tcControl.SelectedIndex;
            SaveChanges(index);
        }

        private void SaveChanges(int index)
        {
            switch (index)
            {
                case (int)ViewModelEnum.LaserDataViewModel:
                    {
                        laserVM.SaveChanges();
                        break;
                    }
                case (int)ViewModelEnum.WeekCodeViewModel:
                    {
                        weekVM.SaveChanges();
                        break;
                    }
                case (int)ViewModelEnum.QuarterCodeViewModel:
                    {
                        quarterVM.SaveChanges();
                        break;
                    }
                case (int)ViewModelEnum.FixtureViewModel:
                    {
                        fixtureVM.SaveChanges();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
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
    }
}