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
using System.Data.Entity.Infrastructure;

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
            LaserDataViewModel laserVM = (LaserDataViewModel)LayoutRoot.DataContext;
            laserVM.ErrorMessage = string.Empty;

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
                                laserVM.ErrorMessage = string.Format("Error Saving to Database: {0}", error.ErrorMessage);
                            }
                            catch (DbUpdateException ex)
                            {
                                string errorMessage = string.Empty;
                                var innerException = ex.InnerException;
                                while (innerException != null)
                                {
                                    errorMessage = innerException.Message;
                                    innerException = innerException.InnerException;
                                }
                                errorMessage = ParseInnerExceptionMessage(errorMessage);
                                laserVM.ErrorMessage = string.Format("Error Saving to Database: {0}", errorMessage);
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

        private string ParseInnerExceptionMessage(string errorMessage)
        {
            string result = string.Empty;
            if (errorMessage.Contains("UNIQUE KEY"))
            {
                string s = errorMessage;
                int start = s.IndexOf("(") + 1;
                int end = s.IndexOf(")", start);
                string tmp = s.Substring(start, end - start);
                string[] arr = tmp.Split(new[] { ',' });

                result = string.Format("LaserData kräver att F1+Kant bildar ett unikt värde.\n Det finns redan en artikel som har värdet: F1 ={0}, Kant ={1}", arr[0], arr[1]);
            }
            else
            {
                result = errorMessage;
            }
            return result;
        }

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
                laserDataDataGrid.ScrollIntoView(e.AddedItems[0]);                              // Since we only can select one it will always be in position 0
                LaserDataViewModel laserVM = (LaserDataViewModel)LayoutRoot.DataContext;
                laserVM.SelectedLaserDataRow = (LaserData)e.AddedItems[0];
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