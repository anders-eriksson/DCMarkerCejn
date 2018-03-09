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

        public MainWindow()
        {
            InitializeComponent();
            int currentMask = IoFix.Add(1);
            currentMask = IoFix.Add(4);
            currentMask = IoFix.Add(2);
            currentMask = IoFix.Delete(1);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mainVM = new ManualMainViewModel();
            DataContext = mainVM;
        }
    }
}