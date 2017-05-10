//#define TEST

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using DCLog;
using System.Text;

namespace DCTcpServer
{
    public class ServerWithTOnr
    {
        private int _port = 2000;
        private int _maxBufferBytes = 14;
        private int _articleNumberLength = 14;
        private int _toNumberLength = 7;
        private TcpListener _listener;
        private Thread listeningThread;

        public ServerWithTOnr(int port, int bufferLength, int articleNumberlength, int toNumberLength)
        {
            _port = port;
            _maxBufferBytes = bufferLength;
            _articleNumberLength = articleNumberlength;
            _toNumberLength = toNumberLength;

            var childref = new ThreadStart(Listener);
            listeningThread = new Thread(childref)
            {
                Name = "ServerListeningThread"
            };
            listeningThread.Start();
            Log.Debug("Listener thread started");
        }

        private void Listener()
        {
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            Log.Debug(string.Format("Server is started and listening on port: {0}\n", _port));
            while (true)
            {
                try
                {
                    var client = _listener.AcceptTcpClient();       // Blocking!
                    Log.Trace("Client connected");
                    // We have connection
                    //var stream = client.GetStream();
                    var server = client.Client;
                    var buffer = new byte[_maxBufferBytes];

                    var inbuffer = new byte[_maxBufferBytes];
                    var totalReceivedBytes = 0;
                    var readBytes = 0;

                    bool hasReceivedAllBytes = false;
                    do
                    {
                        //readBytes = stream.Read(buffer, totalReceivedBytes, _maxBufferBytes - totalReceivedBytes);

                        readBytes = server.Receive(buffer, _maxBufferBytes, SocketFlags.None); // blocking!
                        Log.Debug(string.Format("Read {0} bytes from client", readBytes));
                        Log.Debug(string.Format("string.read: {0}", readBytes));
                        Log.Debug(ByteToHex(buffer));

                        totalReceivedBytes += readBytes;
                        if (!hasReceivedAllBytes && totalReceivedBytes >= _maxBufferBytes)
                        {
                            hasReceivedAllBytes = true;
                            UpdateWF(buffer);

                            //stream.Write(buffer, 0, _maxBufferBytes);
                            int writeBytes = server.Send(buffer, _maxBufferBytes, SocketFlags.None);
                            Log.Debug(string.Format("Write {0} bytes to client", writeBytes));
                            buffer = new byte[_maxBufferBytes];
                        }
                    } while (readBytes > 0);

                    if (IsAbort(buffer, totalReceivedBytes))
                    {
                        _listener.Stop();
                        return;
                    }

                    //Log.Trace("Client disconnected");
                    // stream.Close();
                    client.Close();
                    Log.Trace("Client disconnected/closed");
                }
                catch (SocketException ex)
                {
                    if ((ex.SocketErrorCode == SocketError.Interrupted))
                    {
                        // a blocking listen has been cancelled
                        _listener.Stop();

                        return;
                    }
                    else if (ex.SocketErrorCode == SocketError.ConnectionAborted
                            || ex.SocketErrorCode == SocketError.ConnectionReset)
                    {
                        Log.Trace("Connection Aborted");
                    }
                    else
                    {
                        Log.Error(ex, "SocketException");
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Unknown Exception");
                    throw;
                }
            }
        }

        private string ByteToHex(byte[] buffer)
        {
            string hex = BitConverter.ToString(buffer);
            return hex;
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
            // TODO: Split buffer into articleNumber and TOnumber
            string articleNumber;
            string toNumber;
            SplitIntoStrings(buffer, out articleNumber, out toNumber);

#if DEBUGx
            RaiseNewArticleNumberEvent(string.Format("Raw:\t\t{0}", articleNumber));
            articleNumber = RemoveNonPrintableChars(articleNumber);
            RaiseNewArticleNumberEvent(string.Format("Used:\t\t{0}", articleNumber));
#else
            articleNumber = RemoveNonPrintableChars(articleNumber);
            RaiseNewArticleNumberEvent(articleNumber, toNumber);
#endif
        }

        private void SplitIntoStrings(byte[] buffer, out string articleNumber, out string toNumber)
        {
            articleNumber = string.Empty;
            toNumber = string.Empty;

            if (buffer.Length >= _articleNumberLength + _toNumberLength)
            {
                byte[] articleNumberArray = new byte[_articleNumberLength];
                Array.Copy(buffer, 0, articleNumberArray, 0, _articleNumberLength);
                int count = articleNumberArray.Count(bt => bt != 0); // find the first null
                articleNumber = System.Text.Encoding.ASCII.GetString(articleNumberArray, 0, count);

                byte[] toNumberArray = new byte[_toNumberLength];
                Array.Copy(buffer, 0, toNumberArray, 0, _toNumberLength);
                count = toNumberArray.Count(bt => bt != 0); // find the first null
                toNumber = System.Text.Encoding.ASCII.GetString(toNumberArray, 0, count);
            }
            else
            {
                int count = buffer.Count(bt => bt != 0); // find the first null
                articleNumber = System.Text.Encoding.ASCII.GetString(buffer, 0, count);
            }
        }

        private string RemoveNonPrintableChars(string articleNumber)
        {
            StringBuilder sb = new StringBuilder();
            int len = articleNumber.Length;
            for (int i = 0; i < len; i++)
            {
                if (articleNumber[i] > 31)
                {
                    sb.Append(articleNumber[i]);
                }
            }

            return sb.ToString();
        }

        #region Display Message Event

        public delegate void DisplayMsgHandler(string articleNumber, string toNumber);

        public event DisplayMsgHandler NewArticleNumberEvent;

        internal void RaiseNewArticleNumberEvent(string articleNumber, string toNumber)
        {
            DisplayMsgHandler handler = NewArticleNumberEvent;
            if (handler != null)
            {
                handler(articleNumber, toNumber);
            }
        }

        public class DisplayMsgArgs : EventArgs
        {
            public DisplayMsgArgs(string s, string t)
            {
                Text = s;
                ToNumber = t;
            }

            public string Text { get; private set; } // readonly
            public string ToNumber { get; private set; } // readonly
        }

        #endregion Display Message Event
    }
}