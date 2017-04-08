using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DCAdmin.Control
{
    public class DataGridTrimTextColumn : DataGridTextColumn
    {
        public DataGridTrimTextColumn()
        {
            //LostFocus += TrimOnLostFocus;
        }

        private void TrimOnLostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            var trimTextBox = sender as TextBox;
            if (trimTextBox != null)
                trimTextBox.Text = trimTextBox.Text.Trim();
        }
    }
}