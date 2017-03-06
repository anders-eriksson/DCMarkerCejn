using DCAdmin.ViewModel;
using DCMarkerEF;
using System;
using System.Data.Entity.Validation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Threading;
using System.Linq;

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
                            try
                            {
                                DB.Instance.SaveChanges();
                            }
                            catch (DbEntityValidationException ex)
                            {
                                var error = ex.EntityValidationErrors.First().ValidationErrors.First();
                                LaserDataViewModel laserVM = (LaserDataViewModel)LayoutRoot.DataContext;
                                laserVM.ErrorMessage = string.Format("Error Saving to Database: {0}", error.ErrorMessage);
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                        return null;
                    }), DispatcherPriority.Background, new object[] { null });
                }
            }
        }

        //private void FindArticleAndScrollIntoView()
        //{
        //    var entity = DB.Instance.FindArticle(SearchArticleNumber.Text);
        //    if (entity != null)
        //    {
        //        ScrollToView(entity);
        //    }
        //    else
        //    {
        //        LaserDataViewModel laserVM = (LaserDataViewModel)LayoutRoot.DataContext;
        //        laserVM.ErrorMessage = "Article not found!";
        //    }
        //}

        //private void ScrollToView(LaserData entity)
        //{
        //    LaserDataViewModel laserVM = (LaserDataViewModel)LayoutRoot.DataContext;
        //    laserVM.SelectedLaserDataRow = null;
        //    laserVM.SelectedLaserDataRow = entity;
        //}

        private void SearchArticleNumber_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                e.Handled = true;
                LaserDataViewModel laserVM = (LaserDataViewModel)LayoutRoot.DataContext;
                laserVM.FindArticleAndScrollIntoView(SearchArticleNumber.Text);
            }
        }

        private void SearchArticleNumber_LostFocus(object sender, RoutedEventArgs e)
        {
            LaserDataViewModel laserVM = (LaserDataViewModel)LayoutRoot.DataContext;
            laserVM.ErrorMessage = string.Empty;
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            LaserDataViewModel laserVM = (LaserDataViewModel)LayoutRoot.DataContext;
            laserVM.FindArticleAndScrollIntoView(SearchArticleNumber.Text);
        }

        private void laserDataDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                laserDataDataGrid.ScrollIntoView(e.AddedItems[0]);
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

        private void laserDataDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            SetColumnWidthToCell(laserDataDataGrid);
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton tmp = (ToggleButton)sender;
            LaserDataViewModel laserVM = (LaserDataViewModel)LayoutRoot.DataContext;
            laserVM.ErrorMessage = string.Empty;
            if (tmp.IsChecked.Value)
            {
                if (!string.IsNullOrWhiteSpace(laserVM.FilterKey) && !string.IsNullOrWhiteSpace(laserVM.FilterValue))
                {
                    laserVM.ExecuteFilter();
                    laserDataDataGrid.Items.Refresh();
                }
                else
                {
                    tmp.IsChecked = false;
                    laserVM.ErrorMessage = "Both Filter Column and Value must be entered!";
                }
            }
            else
            {
                laserVM.ExecuteNoFilter();
                laserDataDataGrid.Items.Refresh();
            }
        }

        private void FilterCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string key = FilterCombobox.SelectedItem.ToString();
            LaserDataViewModel laserVM = (LaserDataViewModel)LayoutRoot.DataContext;
            laserVM.FilterKey = key;
        }
    }
}