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
using System.Diagnostics;
using System.Threading;
using System.Windows.Media;
using DCLog;

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
        }

        private void LaserDataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            LaserDataViewModel laserVM = (LaserDataViewModel)LayoutRoot.DataContext;
            laserVM.EditColor = Colors.Red;
        }

        //private void LaserDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        //{
        //    LaserDataViewModel laserVM = (LaserDataViewModel)LayoutRoot.DataContext;
        //    laserVM.EditColor = Colors.LimeGreen;
        //}

        private void LaserDataDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
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
                                laserVM.RaiseSaveChangesEvent();
                                laserVM.EditColor = Colors.LimeGreen;
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
                            catch (Exception ex)
                            {
                                Log.Fatal(ex, "Database Error Saving Changes");
                                throw;
                            }
                        }
                        else
                        {
                            laserVM.EditColor = Colors.LimeGreen;
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
                DoFind();
            }
        }

        private void SearchArticleNumber_LostFocus(object sender, RoutedEventArgs e)
        {
            LaserDataViewModel laserVM = (LaserDataViewModel)LayoutRoot.DataContext;
            laserVM.ErrorMessage = string.Empty;
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            DoFind();
        }

        private void DoFind()
        {
            LaserDataViewModel laserVM = (LaserDataViewModel)LayoutRoot.DataContext;
            var item = laserVM.FindArticleAndScrollIntoView(SearchArticleNumber.Text);

            //laserDataDataGrid.Focus();
            //laserDataDataGrid.ScrollToCenterOfView(item);
        }

        private void LaserDataDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private void LaserDataDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            SetColumnWidthToCell(laserDataDataGrid);
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton tmp = (ToggleButton)sender;
            LaserDataViewModel laserVM = (LaserDataViewModel)LayoutRoot.DataContext;
            laserVM.ErrorMessage = string.Empty;
            SplashScreen splash = new SplashScreen("./LoadingDatabase.png");
            splash.Show(autoClose: true, topMost: true);

            laserVM.ErrorMessage = string.Empty;
            if (tmp.IsChecked.Value)
            {
                if (laserVM.HasFilterType == FilterType.Text)
                {
                    if (!string.IsNullOrWhiteSpace(laserVM.FilterKey))
                    {
                        laserVM.ExecuteFilter();
                    }
                    else
                    {
                        tmp.IsChecked = false;
                        laserVM.ErrorMessage = "You must select a column to filter on!";
                        return;
                    }
                }
                else if (laserVM.HasFilterType == FilterType.Bool)
                {
                    laserVM.ExecuteFilterBool();
                }

                laserDataDataGrid.DataContext = null;
                laserDataDataGrid.DataContext = laserVM;
                laserDataDataGrid.Items.Refresh();
            }
            else
            {
                laserVM.ExecuteNoFilter();
                laserDataDataGrid.DataContext = null;
                laserDataDataGrid.DataContext = laserVM;
                laserDataDataGrid.Items.Refresh();
            }
        }

        private void FilterCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilterCombobox != null && FilterCombobox.SelectedItem != null)
            {
                LaserDataViewModel laserVM = (LaserDataViewModel)LayoutRoot.DataContext;
                string key = FilterCombobox.SelectedItem.ToString();

                if (key == "ExternTest" || key == "EnableTO")
                {
                    laserVM.HasFilterType = FilterType.Bool;
                }
                else
                {
                    laserVM.HasFilterType = FilterType.Text;
                }
                laserVM.FilterKey = key;
            }
        }

        /// <summary>
        /// Remove all whitespace
        /// </summary>
        /// <param name="sender">Not Used</param>
        /// <param name="e">Reference to the Datagrid Textbox</param>
        private void LaserDataDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            var editedTextbox = e.EditingElement as TextBox;
            if (editedTextbox != null)
            {
                editedTextbox.Text = editedTextbox.Text.Trim();
            }

            //LaserDataViewModel laserVM = (LaserDataViewModel)LayoutRoot.DataContext;
            //laserVM.EditColor = Colors.LimeGreen;
        }

        private string GetCurrentCellValue(TextBox txtCurCell)
        {
            return txtCurCell.Text;
        }

        private void GotoSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            LaserDataViewModel laserVM = (LaserDataViewModel)LayoutRoot.DataContext;
            laserVM.GotoSelected();
            //var sel = laserVM.SelectedLaserDataRow;
            //laserVM.SelectedLaserDataRow = null;
            //Thread.Sleep(500);
            //laserVM.SelectedLaserDataRow = sel;
        }

        private void ClearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            LaserDataViewModel laserVM = (LaserDataViewModel)LayoutRoot.DataContext;
            laserVM.FilterKey = string.Empty;
            laserVM.FilterValue = string.Empty;
        }
    }
}