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

namespace DCAdmin
{
    /// <summary>
    /// Interaction logic for ConfirmationDeleteRow.xaml
    /// </summary>
    public partial class ConfirmationDeleteRowWindow : Window
    {
        private string _maskinID;
        private string _article;
        private string _kant;

        public ConfirmationDeleteRowWindow()
        {
            InitializeComponent();
        }

        public void InitValues(string maskinID, string article, string kant)
        {
            _maskinID = maskinID;
            _article = article;
            _kant = kant;

            MaskinIDTextblock.Text = maskinID;
            ArticleTextblock.Text = article;
            KantTextblock.Text = kant;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}