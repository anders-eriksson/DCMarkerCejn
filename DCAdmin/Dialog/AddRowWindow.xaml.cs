using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DCAdmin.ViewModel;
using GlblRes = global::DCAdmin.Properties.Resources;

namespace DCAdmin
{
    /// <summary>
    /// Interaction logic for AddRow.xaml
    /// </summary>
    public partial class AddRowWindow : Window
    {
        public AddRowVM RowData { get; set; }

        public AddRowWindow()
        {
            RowData = new AddRowVM();
            DataContext = RowData;

            InitializeComponent();

            ArticleTextbox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            RowData.MachineCode = MachineCodeTextbox.Text.Trim();
            RowData.Article = ArticleTextbox.Text.Trim();
            RowData.Kant = KantTextbox.Text.Trim();
            DialogResult = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DialogResult.HasValue && DialogResult.Value)
            {
                if (!RowData.RowExists())
                {
                    // Save values for next time
                    Properties.Settings.Default.MachineCode = RowData.MachineCode;
                    Properties.Settings.Default.Article = RowData.Article;
                    Properties.Settings.Default.Kant = RowData.Kant;
                    e.Cancel = false;
                }
                else
                {
                    // article already exists!
                    RowData.ErrorMessage = GlblRes.Article_already_exists;
                    e.Cancel = true;
                    DialogResult = false;
                }
            }
        }
    }
}