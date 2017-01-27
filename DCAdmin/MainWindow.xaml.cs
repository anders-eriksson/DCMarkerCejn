using DCMarkerEF;
using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DCAdmin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DB db;
        private const int LASERDATADATAGRID = 0;
        private const int WEEKCODEDATAGRID = 1;
        private const int QUARTERCODEDATAGRID = 2;
        private const int FIXTUREDATAGRID = 3;

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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.Windows.Data.CollectionViewSource laserDataViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("laserDataViewSource")));
            laserDataViewSource.Source = db.LoadLaserData();

            System.Windows.Data.CollectionViewSource weekCodeViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("weekCodeViewSource")));
            weekCodeViewSource.Source = db.LoadWeekCode();

            System.Windows.Data.CollectionViewSource quarterCodeViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("quarterCodeViewSource")));
            quarterCodeViewSource.Source = db.LoadQuarterCode();

            System.Windows.Data.CollectionViewSource fixtureViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("fixtureViewSource")));
            fixtureViewSource.Source = db.LoadFixture();
        }

        private void CenterWindowOnScreen()
        {
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth / 2) - (windowWidth / 2);
            this.Top = (screenHeight / 2) - (windowHeight / 2);
        }

        private static void SetColumnWidthToCell(DataGrid dgrid)
        {
            var columns = dgrid.Columns;
            foreach (var column in columns)
            {
                column.Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
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
                    db.SaveChanges();
                }
            }
            db.Dispose();
        }

        private void Window_ContentRendered(object sender, System.EventArgs e)
        {
            //CenterWindowOnScreen();
            //SetColumnWidthToCell(laserDataDataGrid);
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            int index = tcControl.SelectedIndex;
            AddNewRecord(index);
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

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            db.SaveChanges();
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

        private void SearchArticleNumber_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                e.Handled = true;
                FindArticleAndScrollIntoView();
            }
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            FindArticleAndScrollIntoView();
        }

        private void FindArticleAndScrollIntoView()
        {
            var entity = db.FindArticle(SearchArticleNumber.Text);
            ScrollToView(entity);
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

        private void SearchArticleNumber_LostFocus(object sender, RoutedEventArgs e)
        {
            SearchError.Text = "";
        }

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnTop_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            int index = tcControl.SelectedIndex;
            RefreshDataGrid(index);
        }

        private void RefreshDataGrid(int index)
        {
            if (index == LASERDATADATAGRID)
            {
                laserDataDataGrid.Items.Refresh();
            }
            else if (index == WEEKCODEDATAGRID)
            {
                weekCodeDataGrid.Items.Refresh();
            }
            else if (index == QUARTERCODEDATAGRID)
            {
                quarterCodeDataGrid.Items.Refresh();
            }
            else if (index == FIXTUREDATAGRID)
            {
                fixtureDataGrid.Items.Refresh();
            }
            else
            {
                laserDataDataGrid.Items.Refresh();
                weekCodeDataGrid.Items.Refresh();
                quarterCodeDataGrid.Items.Refresh();
                fixtureDataGrid.Items.Refresh();
            }
        }
    }
}