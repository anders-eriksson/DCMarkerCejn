using DCAdmin.ViewModel;
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
using GlblRes = global::DCAdmin.Properties.Resources;

namespace DCAdmin
{
    /// <summary>
    /// Interaction logic for AddFixture.xaml
    /// </summary>
    public partial class AddFixture : Window
    {
        public AddFixtureVM FixtureData { get; set; }

        public AddFixture()
        {
            FixtureData = new AddFixtureVM();
            DataContext = FixtureData;
            InitializeComponent();
            FixtureTextbox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            FixtureData.FixtureId = FixtureTextbox.Text.Trim();
            DialogResult = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DialogResult.HasValue && DialogResult.Value)
            {
                if (!FixtureData.RowExists())
                {
                    e.Cancel = false;
                }
                else
                {
                    // article already exists!
                    FixtureData.ErrorMessage = GlblRes.Fixture_already_exists;
                    e.Cancel = true;
                    DialogResult = false;
                }
            }
        }
    }
}