using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace DCAdmin.View
{
    /// <summary>
    /// Interaction logic for WeekCodeControl.xaml
    /// </summary>
    public partial class WeekCodeControl : UserControl
    {
        public WeekCodeControl()
        {
            InitializeComponent();
        }

        private void weekCodeDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
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
    }
}