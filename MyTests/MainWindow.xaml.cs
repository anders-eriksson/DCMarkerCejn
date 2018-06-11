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
using System.Windows.Navigation;
using System.Windows.Shapes;
using LaserWrapper;

namespace MyTests
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ManualMainViewModel mainVM;
        private byte[] _allowedArray;

        public MainWindow()
        {
            InitializeComponent();
            int currentMask = IoFix.Add(1);
            currentMask = IoFix.Add(4);
            currentMask = IoFix.Add(2);
            currentMask = IoFix.Delete(1);
            _allowedArray = new byte[] { 10, 12, 14, 16 };

            bool brc = IsDataAllowed(10);
            brc = IsDataAllowed(12);
            brc = IsDataAllowed(14);
            brc = IsDataAllowed(15);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mainVM = new ManualMainViewModel();
            DataContext = mainVM;
        }

        private bool IsDataAllowed(byte data)
        {
            bool result = false;
            result = _allowedArray.Contains(data);

            return result;
        }
    }
}