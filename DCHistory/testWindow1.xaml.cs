using System.Windows;

namespace DCHistory
{
    /// <summary>
    /// Interaction logic for testWindow1.xaml
    /// </summary>
    public partial class testWindow1 : Window
    {
        public testWindow1()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.Windows.Data.CollectionViewSource dCHistoryContextViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("dCHistoryContextViewSource")));
            // Load data by setting the CollectionViewSource.Source property:
            // dCHistoryContextViewSource.Source = [generic data source]
        }
    }
}