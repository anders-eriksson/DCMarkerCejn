using DCMarkerEF;
using System;
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
        private CollectionViewSource fixtureViewSource;
        private CollectionViewSource laserDataViewSource;
        private CollectionViewSource quarterCodeViewSource;
        private CollectionViewSource weekCodeViewSource;

        public MainWindow()
        {
            db = new DB();
            InitializeComponent();

            Services.Tracker.Configure(this)//the object to track
                                           .IdentifyAs("main window")                                                                           //a string by which to identify the target object
                                           .AddProperties<Window>(w => w.Height, w => w.Width, w => w.Top, w => w.Left, w => w.WindowState)     //properties to track
                                           .RegisterPersistTrigger(nameof(SizeChanged))                                                         //when to persist data to the store
                                           .Apply();                                                                                            //apply any previously stored data
        }

        public static double MaxScreenSize
        {
            get
            {
                return System.Windows.SystemParameters.PrimaryScreenWidth;
            }
        }

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
            throw new NotImplementedException();
        }

        private void AddNewRecord(int index)
        {
            switch (index)
            {
                case 0:
                    {
                        var entity = db.AddNewLaserDataRecord();
                        if (entity != null)
                        {
                            laserDataDataGrid.SelectedItem = entity;
                            laserDataDataGrid.ScrollToCenterOfView(entity);
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
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
                case 0:
                    {
                        var selectedItems = laserDataDataGrid.SelectedCells;

                        db.DeleteLaserDataRecord(selectedItems);
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

        private void FindArticleAndScrollIntoView()
        {
            var entity = db.FindArticle(SearchArticleNumber.Text);
            ScrollToView(entity);
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            FindArticleAndScrollIntoView();
        }

        private void laserDataDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            if (e.EditAction == DataGridEditAction.Commit)
            {
                ListCollectionView view = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource) as ListCollectionView;
                if (view.IsAddingNew || view.IsEditingItem)
                {
                    this.Dispatcher.BeginInvoke(new DispatcherOperationCallback(param =>
                    {
                        // This callback will be called after the CollectionView
                        // has pushed the changes back to the DataGrid.ItemSource.

                        if (db.IsChangesPending())
                        {
                            db.SaveChanges();
                        }
                        return null;
                    }), DispatcherPriority.Background, new object[] { null });
                }
            }
        }

        private void LoadFixture()
        {
            fixtureViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("fixtureViewSource")));
            fixtureViewSource.Source = db.LoadFixture();
        }

        private void LoadLaserData()
        {
            laserDataViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("laserDataViewSource")));
            laserDataViewSource.Source = db.LoadLaserData();
        }

        private void LoadQuarterCode()
        {
            quarterCodeViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("quarterCodeViewSource")));
            quarterCodeViewSource.Source = db.LoadQuarterCode();
        }

        private void LoadWeekCode()
        {
            weekCodeViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("weekCodeViewSource")));
            weekCodeViewSource.Source = db.LoadWeekCode();
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            int index = tcControl.SelectedIndex;
            AddNewRecord(index);
        }

        private void OnTop_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshDatabase();
        }

        private void RefreshDatabase()
        {
            db.Refresh();
            LoadLaserData();
            LoadWeekCode();
            LoadQuarterCode();
            LoadFixture();
        }

        private void RefreshDataGrid(int index)
        {
            if (index == Consts.LASERDATADATAGRID)
            {
                laserDataViewSource.View.Refresh();
            }
            else if (index == Consts.WEEKCODEDATAGRID)
            {
                weekCodeViewSource.View.Refresh();
            }
            else if (index == Consts.QUARTERCODEDATAGRID)
            {
                quarterCodeViewSource.View.Refresh();
            }
            else if (index == Consts.FIXTUREDATAGRID)
            {
                fixtureViewSource.View.Refresh();
            }
            else
            {
                laserDataViewSource.View.Refresh();
                weekCodeViewSource.View.Refresh();
                quarterCodeViewSource.View.Refresh();
                fixtureViewSource.View.Refresh();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            db.SaveChanges();
            RefreshDataGrid(0); // TODO change this so it handles all the tables!
        }

        private void ScrollToView(LaserData entity)
        {
            if (entity != null)
            {
                laserDataDataGrid.SelectedItem = entity;
                laserDataDataGrid.ScrollToCenterOfView(entity);
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
                    db.SaveChanges();
                }
            }
            db.Dispose();
        }

        private void Window_ContentRendered(object sender, System.EventArgs e)
        {
            //CenterWindowOnScreen();
            SetColumnWidthToCell(laserDataDataGrid);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadLaserData();
            LoadWeekCode();

            LoadQuarterCode();

            LoadFixture();
        }
    }
}