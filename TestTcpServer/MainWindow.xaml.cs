using DCTcpServer;
using System.Diagnostics;
using System.Windows;
using System;
using System.IO;
using DCLog;

namespace TestTcpServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int port = 2000;
        private int bufferLength = 14;
        private TextBoxValue TBV = new TextBoxValue();
        private Server _server;

        public MainWindow()
        {
            Log.Trace("Starting TestTcpServer ------------------------------------------------------------------------------------------------");
            port = Properties.Settings.Default.Port;
            bufferLength = Properties.Settings.Default.BufferSize;

            InitializeComponent();
            textBox.DataContext = TBV;
            InitTcpServer(port, bufferLength);
        }

        private void InitTcpServer(int port, int bufferLength)
        {
            Log.Trace(string.Format("InitTcpServer - Port: {0} - bufferLength: {1}", port, bufferLength));
            _server = new Server(port, bufferLength);
            _server.NewArticleNumberEvent += _server_NewArticleNumberEvent;
        }

        private void _server_NewArticleNumberEvent(string msg)
        {
            TBV.Add(msg);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _server.Abort();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            TBV = new TextBoxValue();
        }

        private void LogfileButton_Click(object sender, RoutedEventArgs e)
        {
            string logPath = GetLogPath();
            Process.Start(logPath);
        }

        private string GetLogPath()
        {
            var result = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DCLasersystem\\TestTcpServer");
            if(!Directory.Exists(result))
            {
                Directory.CreateDirectory(result);
            }

            return result;
        }
    }
}