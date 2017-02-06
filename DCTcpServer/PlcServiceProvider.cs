using System;
using TcpLib;

namespace DCTcpServer
{
    public class PlcServiceProvider : TcpServiceProvider
    {
        private int bufferLen = 12;

        public PlcServiceProvider()
        {
        }

        public PlcServiceProvider(int buflen)
        {
            bufferLen = buflen;
        }

        public override object Clone()
        {
            return new PlcServiceProvider();
        }

        public override void OnAcceptConnection(ConnectionState state)
        {
            throw new NotImplementedException();
        }

        public override void OnDropConnection(ConnectionState state)
        {
            throw new NotImplementedException();
        }

        public override void OnReceiveData(ConnectionState state)
        {
            var buffer = new byte[bufferLen];
            var receivedLength = 0;
            while (receivedLength < bufferLen)
            {
                var nextLength = state.Read(buffer, receivedLength, bufferLen - receivedLength);
                if (nextLength == 0)
                {
                    // Update Workflow
                    UpdateWF(buffer);

                    state.Write(buffer, 0, bufferLen);
                }
                receivedLength += nextLength;
            }
            // end the communication ??
            state.EndConnection();
        }

        private void UpdateWF(byte[] buffer)
        {
            throw new NotImplementedException();
        }
    }
}