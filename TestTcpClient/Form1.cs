using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace TestTcpClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            string ipaddress = Properties.Settings.Default.IpAddress;
            int ipport = Properties.Settings.Default.IpPort;
            int byteArrLength = Properties.Settings.Default.ByteArrLength;
            using (TcpClient client = new TcpClient(ipaddress, ipport))
            {
                using (NetworkStream sw = client.GetStream())
                {
                    byte[] buffer = GetBytes(ArticleTextbox.Text.Trim(), byteArrLength);

                    sw.Write(buffer, 0, buffer.Length);

                    // wait for response
                    byte[] response = new byte[1024];

                    sw.Read(response, 0, response.Length);

                    string responceString = Encoding.ASCII.GetString(response);

                    ReceivedTextbox.Text = responceString;
                }
            }
        }

        private static byte[] GetBytes(string str, int byteArrLen = 14)
        {
            byte[] bytes = new byte[byteArrLen];
            byte[] tmp = Encoding.ASCII.GetBytes(str);
            System.Buffer.BlockCopy(tmp, 0, bytes, 0, tmp.Length);

            return bytes;
        }

        private static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }
    }
}