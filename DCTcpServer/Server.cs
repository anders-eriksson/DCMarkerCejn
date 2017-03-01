using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace DCTcpServer
{
    public class Server
    {
        private int _port = 50000;
        private int _bufferLength = 12;
        private TcpListener _listener;
        private Thread listeningThread;

        public Server(int port = 50000, int bufferLength = 12)
        {
            _port = port;
            _bufferLength = bufferLength;

            var childref = new ThreadStart(Listener);
            listeningThread = new Thread(childref)
            {
                Name = "ServerListeningThread"
            };
            listeningThread.Start();
        }

        private void Listener()
        {
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            while (true)
            {
                try
                {
                    var client = _listener.AcceptTcpClient();       // Blocking!

                    // We have connection...
                    var stream = client.GetStream();
                    var buffer = new byte[_bufferLength];
                    var receivedLength = 0;

                    while (receivedLength < _bufferLength)
                    {
                        var nextLength = stream.Read(buffer, receivedLength, _bufferLength - receivedLength);
                        if (IsAbort(buffer, receivedLength))
                        {
                            _listener.Stop();
                            return;
                        }
                        if (nextLength == 0 || nextLength == _bufferLength)
                        {
                            // Update Workflow
                            UpdateWF(buffer);

                            stream.Write(buffer, 0, _bufferLength);

                            stream.Close();
                            client.Close();
                        }
                        receivedLength += nextLength;
                    }
                }
                catch (SocketException e)
                {
                    if ((e.SocketErrorCode == SocketError.Interrupted))
                    {
                        // a blocking listen has been cancelled
                        _listener.Stop();
                        return;
                    }
                }
            }
        }

        private static bool IsAbort(byte[] buffer, int receivedLength)
        {
            var msg = System.Text.Encoding.ASCII.GetString(buffer, 0, receivedLength);
            return msg.Trim() == "ABORT" ? true : false;
        }

        public void Abort()
        {
            using (TcpClient client = new TcpClient("127.0.0.1", _port))
            {
                var stream = client.GetStream();
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes("ABORT");
                stream.Write(buffer, 0, buffer.Length);
            }
            //listeningThread.Abort();
        }

        private void UpdateWF(byte[] buffer)
        {
            int count = buffer.Count(bt => bt != 0); // find the first null
            string articleNumber = System.Text.Encoding.ASCII.GetString(buffer, 0, count);
            RaiseNewArticleNumberEvent(articleNumber);
        }

        #region Display Message Event

        public delegate void DisplayMsgHandler(string msg);

        public event DisplayMsgHandler NewArticleNumberEvent;

        internal void RaiseNewArticleNumberEvent(string msg)
        {
            DisplayMsgHandler handler = NewArticleNumberEvent;
            if (handler != null)
            {
                handler(msg);
            }
        }

        public class DisplayMsgArgs : EventArgs
        {
            public DisplayMsgArgs(string s)
            {
                Text = s;
            }

            public string Text { get; private set; } // readonly
        }

        #endregion Display Message Event
    }
}
