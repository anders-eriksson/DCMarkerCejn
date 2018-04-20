using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace TestAdamModule
{
    internal class Program
    {
        private static TcpListener _server;

        private static void Main(string[] args)
        {
            IPAddress ipAddr = IPAddress.Parse("127.0.0.1");
            _server = new TcpListener(ipAddr, 50001);
            _server.Start();
            Console.WriteLine("The server is running at port 50001...");
            Console.WriteLine("The local End point is  :" + _server.LocalEndpoint);
            Console.WriteLine("Waiting for a connection.....");
            TcpClient client = _server.AcceptTcpClient();
            Console.WriteLine("Connected!");

            Byte[] bytes = new Byte[256];
            String data = null;

            // Get a stream object for reading and writing
            NetworkStream stream = client.GetStream();

            int i;

            // Loop to receive all the data sent by the client.
            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                // Translate data bytes to a ASCII string.
                data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                Console.WriteLine(string.Format("Received: {0}", data));

                // Process the data sent by the client.
                data = data.ToUpper();

                byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

                // Send back a response.
                stream.Write(msg, 0, msg.Length);
                listBox1.Items.Add(String.Format("Sent: {0}", data));
            }

            // Shutdown and end connection
            client.Close();
        }
    }

      catch(SocketException ex)
      {
        listBox1.Items.Add("SocketException: {0}" + ex);
      }
    }
        }

        void staticSendArtNo()
{
    bool brc = Send(Constants.STX);
    if (brc) brc = Send(Constants.ArtNo);
}

private bool Send(int cmd)
{
    bool result = true;

    return result;
}
    }
}