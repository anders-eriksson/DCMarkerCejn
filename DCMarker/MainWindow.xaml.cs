﻿using DCMarker.Model;
using System.Threading;
using System.Windows;

namespace DCMarker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel mainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            mainViewModel = new MainViewModel();
            DataContext = mainViewModel;

#if DEBUGx
            InitViewModel();
#endif
        }

        private void InitViewModel()
        {
            mainViewModel.ArticleNumber = "123456789012";
            mainViewModel.Kant = "1";
            mainViewModel.HasKant = true;
            mainViewModel.Fixture = "999888777555";
            mainViewModel.HasFixture = true;
            mainViewModel.Status = "OffLine";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mainViewModel.Abort();
        }
    }
}