using DCMarkerEF;
using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace DCAdmin.View
{
    /// <summary>
    /// Interaction logic for FixtureControl.xaml
    /// </summary>
    public partial class FixtureControl : UserControl
    {
        public FixtureControl()
        {
            InitializeComponent();
        }

        private static void SetColumnWidthToCell(DataGrid dgrid)
        {
            var columns = dgrid.Columns;
            foreach (var column in columns)
            {
                column.Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
            }
        }

        private void fixtureDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            FixtureViewModel viewModel = (FixtureViewModel)LayoutRoot.DataContext;
            viewModel.ErrorMessage = string.Empty;

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
                                viewModel.ErrorMessage = string.Format("Error Saving to Database: {0}", error.ErrorMessage);
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
                                viewModel.ErrorMessage = string.Format("Error Saving to Database: {0}", errorMessage);
                            }
                            catch (Exception ex)
                            {
                                DCLog.Log.Error(ex, "Error Saving to Database");
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
            if (errorMessage.Contains("PRIMARY KEY"))
            {
                string s = errorMessage;
                int start = s.IndexOf("(") + 1;
                int end = s.IndexOf(")", start);
                string tmp = s.Substring(start, end - start);

                result = string.Format("Fixture Id must be unique.There already are a value of '{0}'", tmp);
            }
            else
            {
                result = errorMessage;
            }
            return result;
        }

        private void fixtureDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                fixtureDataGrid.ScrollIntoView(e.AddedItems[0]);
                FixtureViewModel fixtureVM = (FixtureViewModel)LayoutRoot.DataContext;
                fixtureVM.SelectedFixtureRow = (Fixture)e.AddedItems[0];
            }
        }
    }
}