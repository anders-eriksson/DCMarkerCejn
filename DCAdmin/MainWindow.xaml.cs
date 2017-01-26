using DCMarkerEF;
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
        private DCLasermarkContext _context;

        public MainWindow()
        {
            _context = new DCLasermarkContext();
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

        private bool IsChangesPending()
        {
            return _context.ChangeTracker.HasChanges();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.Windows.Data.CollectionViewSource laserDataViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("laserDataViewSource")));
            _context.LaserData.OrderBy(x => x.F1).Load();
            laserDataViewSource.Source = _context.LaserData.Local;

            System.Windows.Data.CollectionViewSource weekCodeViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("weekCodeViewSource")));
            _context.WeekCode.Load();
            weekCodeViewSource.Source = _context.WeekCode.Local;

            System.Windows.Data.CollectionViewSource quarterCodeViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("quarterCodeViewSource")));
            _context.QuarterCode.Load();
            quarterCodeViewSource.Source = _context.QuarterCode.Local;

            System.Windows.Data.CollectionViewSource fixtureViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("fixtureViewSource")));
            _context.Fixture.Load();
            fixtureViewSource.Source = _context.Fixture.Local;
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
            if (IsChangesPending())
            {
                var result = MessageBox.Show("Changes exists! Do you want to save them?", "Pending Changes", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }

                if (result == MessageBoxResult.Yes)
                {
                    _context.SaveChanges();
                }
            }
            _context.Dispose();
        }

        private void Window_ContentRendered(object sender, System.EventArgs e)
        {
            //CenterWindowOnScreen();
            SetColumnWidthToCell(laserDataDataGrid);
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
                        var entity = AddNewLaserDataRecord();
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

        private LaserData AddNewLaserDataRecord()
        {
            LaserData result;
            LaserData entity = new LaserData();

            result = _context.LaserData.Add(entity);

            return result;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _context.SaveChanges();
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
                        DeleteLaserDataRecord();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        private void DeleteLaserDataRecord()
        {
            var selectedItems = laserDataDataGrid.SelectedCells;
            if (selectedItems != null)
            {
                var selectedItem = selectedItems[0];
                var item = (LaserData)selectedItem.Item;
                var id = item.Id;
                var entity = _context.LaserData.Find(id);
                _context.LaserData.Remove(entity);
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
                FindArticle();
            }
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            FindArticle();
        }

        private void FindArticle()
        {
            SearchError.Text = "";
            string articleNumber = SearchArticleNumber.Text.Trim();
            var entity = _context.LaserData.FirstOrDefault<LaserData>(e => e.F1 == articleNumber);
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
    }
}