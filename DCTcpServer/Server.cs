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
    public class Server
    {
        private int _port = 50000;
        private int _maxBufferBytes = 14;
        private TcpListener _listener;
        private Thread _listeningThread;
        private TcpClient _client = null;
        private Socket _server = null;
        private byte[] _buffer;

        public byte[] BufferFromPlc
        {
            get { return _buffer; }
            internal set { _buffer = value; }
        }

        public Server(int port = 50000, int bufferLength = 14)
        {
            _port = port;
            _maxBufferBytes = bufferLength;

            var childref = new ThreadStart(Listener);
            _listeningThread = new Thread(childref)
            {
                Name = "ServerListeningThread"
            };
            _listeningThread.Start();
            Log.Debug("Listener thread started");
        }

#if XXX

        private void Listener()
        {
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            Log.Debug(string.Format("Server is started and listening on port: {0}\n", _port));
            while (true)
            {
                try
                {
                    if (_client == null || !_client.Connected)
                    {
                        _client = _listener.AcceptTcpClient();       // Blocking!
                        Log.Trace("Client connected");
                        // We have connection
                        _server = _client.Client;
                        _buffer = new byte[_maxBufferBytes];
                    }
                    var inbuffer = new byte[_maxBufferBytes];
                    var totalReceivedBytes = 0;
                    var readBytes = 0;

                    bool hasReceivedAllBytes = false;
                    do
                    {
                        readBytes = _server.Receive(_buffer, _maxBufferBytes, SocketFlags.None); // blocking!
                        Log.Debug(string.Format("Read {0} bytes from client", readBytes));
                        Log.Debug(string.Format("string.read: {0}", readBytes));
                        Log.Debug(ByteToHex(_buffer));

                        totalReceivedBytes += readBytes;
                        if (!hasReceivedAllBytes && totalReceivedBytes >= _maxBufferBytes)
                        {
                            hasReceivedAllBytes = true;
                            UpdateWF(_buffer);
                            //buffer = WriteArticleNumberToPlc(server, buffer);
                        }
                    } while (readBytes > 0);

                    if (IsAbort(_buffer, totalReceivedBytes))
                    {
                        CloseConnections();

                        _listener.Stop();
                        Log.Trace("Listener stopped");

                        return;
                    }
                    if(readBytes==0)
                    {
                        CloseConnections();
                    }
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

        private void CloseConnections()
        {
            // Client has disconnected
            _server.Close();
            _server = null;

            _client.Close();
            _client = null;
        }

#else

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

#endif

        public bool WriteArticleNumberToPlc()
        {
            bool result = false;

            try
            {
                int writeBytes = _server.Send(_buffer, _maxBufferBytes, SocketFlags.None);
                Log.Debug(string.Format("Write {0} bytes to client", writeBytes));

                //_buffer = new byte[_maxBufferBytes];

                //_server.Close();
                //_client.Close();
                //Log.Trace("Client disconnected/closed");

                result = true;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "TCP: Can't write to PLC");
                throw;
            }

            return result;
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
            int count = buffer.Count(bt => bt != 0); // find the first null
            string articleNumber = System.Text.Encoding.ASCII.GetString(buffer, 0, count);
#if DEBUGX
            RaiseNewArticleNumberEvent(string.Format("Raw:\t\t{0}", articleNumber));
            articleNumber = RemoveNonPrintableChars(articleNumber);
            RaiseNewArticleNumberEvent(string.Format("Used:\t\t{0}", articleNumber));
#else
            articleNumber = RemoveNonPrintableChars(articleNumber);
            RaiseNewArticleNumberEvent(articleNumber);
#endif
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