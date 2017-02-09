using DCTcpServer;
using System.Windows;

namespace TestTcpServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int port = 50000;
        private int bufferLength = 12;
        private TextBoxValue TBV = new TextBoxValue();
        private Server _server;

        public MainWindow()
        {
            InitializeComponent();
            textBox.DataContext = TBV;
            InitTcpServer(port, bufferLength);
        }

        private void InitTcpServer(int port, int bufferLength)
        {
            _server = new Server();
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
    }
}