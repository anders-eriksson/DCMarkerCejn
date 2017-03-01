using DCAdmin.ViewModel;
using DCMarkerEF;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace DCAdmin.View
{
    /// <summary>
    /// Interaction logic for LaserDataControl.xaml
    /// </summary>
    public partial class LaserDataControl : UserControl
    {
        public LaserDataControl()
        {
            InitializeComponent();
            // LayoutRoot.DataContext = this;
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

                        if (DB.Instance.IsChangesPending())
                        {
                            DB.Instance.SaveChanges();
                        }
                        return null;
                    }), DispatcherPriority.Background, new object[] { null });
                }
            }
        }

        private void FindArticleAndScrollIntoView()
        {
            var entity = DB.Instance.FindArticle(SearchArticleNumber.Text);
            ScrollToView(entity);
        }

        private void ScrollToView(LaserData entity)
        {
            LaserDataViewModel laserVM = (LaserDataViewModel)LayoutRoot.DataContext;
            laserVM.SelectedLaserDataRow = entity;
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

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            FindArticleAndScrollIntoView();
        }

        private void laserDataDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            laserDataDataGrid.ScrollIntoView(e.AddedItems[0]);
        }

        private static void SetColumnWidthToCell(DataGrid dgrid)
        {
            var columns = dgrid.Columns;
            foreach (var column in columns)
            {
                column.Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
            }
        }

        private void laserDataDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            SetColumnWidthToCell(laserDataDataGrid);
        }
    }
}