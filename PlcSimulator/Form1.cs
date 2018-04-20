using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace PlcSimulator
{
    public partial class Form1 : Form
    {
        private TcpListener _server;
        private Int32 port;
        private IPAddress localAddr;

        private int _outbyte;
        private int _inbyte;

        private volatile int _oldData;
        private CommandData _currentCommand;

        private Queue<byte> _commands;
        private bool IsNewCommand = false;
        private volatile bool IsWaitingForAnswer;

        public Form1()
        {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            try
            {
                port = 5000;
                localAddr = IPAddress.Parse("127.0.0.1");
                _server = new TcpListener(localAddr, port);
                TcpListener server = new TcpListener(localAddr, port);

                IsWaitingForAnswer = false;
                _server.Start();
                ListenAndReceive();
                return;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void ListenAndReceive()
        {
            // Enter the listening loop.
            while (true)
            {
                Display("Waiting for a connection...");
                TcpClient client = _server.AcceptTcpClient();           // blocking
                Display("Connected!");

                NetworkStream stream = client.GetStream();

                int data = stream.ReadByte();                            // blocking
                if (IsWaitingForAnswer)
                {
                    OutgoingCommand(ref stream, data);
                }
                else
                {
                    IncommingCommand(ref stream, data);
                }
            }
        }

        private void IncommingCommand(ref NetworkStream stream, int data)
        {
            if (data == Constants.STX)
            {
                // New command
                AcknowledgeRead(ref stream, data);
                int oldData = data;

                do
                {
                    data = stream.ReadByte();
                } while (data == oldData);

                // actual command
                _currentCommand = new CommandData();
                ParseData(data);
                AcknowledgeRead(ref stream, data);
                oldData = data;
                // Param
                do
                {
                    do
                    {
                        data = stream.ReadByte();
                    } while (data == oldData);

                    if (data != Constants.ETX)
                    {
                        _currentCommand.Params.Add((byte)data);
                    }
                    AcknowledgeRead(ref stream, data);
                    oldData = data;
                } while (data != Constants.ETX);
            }
        }

        private void OutgoingCommand(ref NetworkStream stream, int data)
        {
            int olddata = 0;

            if (_outbyte != data)
            {
                // Error - What to do???
                return;
            }
            olddata = _commands.Dequeue();
            stream.WriteByte((byte)olddata);
            do
            {
                data = stream.ReadByte();
            } while (data != olddata);
            olddata = data;

            // read command
            do
            {
                data = stream.ReadByte();
            } while (olddata == data);
            olddata = data;
            stream.WriteByte(Constants.ACK);
            do
            {
                data = stream.ReadByte();
            } while (data != Constants.ACK);
            olddata = data;
        }

        private bool ParseData(int data)
        {
            bool result = true;
            //Debug.WriteLine("\tParseData");
            switch (data)
            {
                case Constants.ArtNrCode:
                    // Load article
                    _currentCommand.Type = CommandTypes.ArtNo;
                    break;

                case Constants.ProvbitCode:
                    _currentCommand.Type = CommandTypes.Provbit;
                    break;

                case Constants.OkCode:
                    _currentCommand.Type = CommandTypes.OK;

                    break;

                case Constants.ItemInPlaceCode:
                    _currentCommand.Type = CommandTypes.ItemInPlace;
                    break;

                case Constants.StartMarkingCode:
                    _currentCommand.Type = CommandTypes.StartMarking;
                    break;

                case Constants.EndMarkingCode:
                    _currentCommand.Type = CommandTypes.EndMarking;
                    break;

                case Constants.RestartCode:
                    _currentCommand.Type = CommandTypes.Restart;
                    break;

                default:
                    result = false;
                    _currentCommand.Type = CommandTypes.Undefined;
                    break;
            }
            return result;
        }

        private bool AcknowledgeRead(ref NetworkStream stream, int bytes)
        {
            bool result = false;
            byte inbyte;

            stream.WriteByte((byte)bytes);
            inbyte = (byte)stream.ReadByte();
            if (bytes == inbyte)
            {
                stream.WriteByte(Constants.ACK);
                inbyte = (byte)stream.ReadByte();
                if (inbyte == Constants.ACK)
                {
                    result = true;
                }
            }

            return result;
        }

        private void Display(string msg)
        {
            MessageTextbox.AppendText(msg);
            MessageTextbox.AppendText(Environment.NewLine);
        }

        private void ArtNoButton_Click(object sender, EventArgs e)
        {
            _outbyte = Constants.ArtNrCode;
        }

        private void StartOKButton_Click(object sender, EventArgs e)
        {
            CreateCommand(Constants.OkCode, "1");
            _outbyte = _commands.Dequeue();
        }

        private void ExternalStartButton_Click(object sender, EventArgs e)
        {
            CreateCommand(Constants.StartMarkingCode, "1");
            _outbyte = _commands.Dequeue();
        }

        private void CreateCommand(byte cmd, string param)
        {
            _commands = new Queue<byte>();
            _commands.Enqueue(2);
            //_commands.Enqueue(255);
            _commands.Enqueue(cmd);
            //_commands.Enqueue(255);
            // params
            Encoding cp850 = Encoding.GetEncoding(850);
            byte[] arr = cp850.GetBytes(param);
            for (int i = 0; i < arr.Length; i++)
            {
                _commands.Enqueue(arr[i]);
                //_commands.Enqueue(255);
            }

            IsNewCommand = true;
        }

        private static bool SendCommand(NetworkStream stream, byte cmd)
        {
            bool result = false;
            byte[] outbuf = new byte[1];
            byte[] inbuf = new byte[1];

            outbuf[0] = cmd;
            stream.Write(outbuf, 0, 1);
            stream.Read(inbuf, 0, 1);

            if (inbuf[0] == outbuf[0])
            {
                outbuf[0] = Constants.ACK;
                stream.Write(outbuf, 0, 1);
                stream.Read(inbuf, 0, 1);
                if (inbuf[0] == outbuf[0])
                {
                    result = true;
                }
            }
            return result;
        }

        private void SendParam(NetworkStream stream, string articlenumber)
        {
            bool result = true;
            byte[] outbuf = new byte[1];
            byte[] inbuf = new byte[1];

            Encoding cp850 = Encoding.GetEncoding(850);
            byte[] arr = cp850.GetBytes(articlenumber);

            int pos = 0;
            while (result && pos <= arr.Length)
            {
                outbuf[0] = arr[pos++];
                stream.Write(outbuf, 0, 1);
                stream.Read(inbuf, 0, 1);

                if (inbuf[0] == outbuf[0])
                {
                    outbuf[0] = Constants.ACK;
                    stream.Write(outbuf, 0, 1);
                    stream.Read(inbuf, 0, 1);
                    if (inbuf[0] == outbuf[0])
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                else
                {
                    result = false;
                }
            }
        }

        private bool SendParam(NetworkStream stream, byte param)
        {
            bool result = false;
            byte[] outbuf = new byte[1];
            byte[] inbuf = new byte[1];

            outbuf[0] = param;
            stream.Write(outbuf, 0, 1);
            stream.Read(inbuf, 0, 1);

            if (inbuf[0] == outbuf[0])
            {
                outbuf[0] = Constants.ACK;
                stream.Write(outbuf, 0, 1);
                stream.Read(inbuf, 0, 1);
                if (inbuf[0] == outbuf[0])
                {
                    result = true;
                }
            }
            return result;
        }
    }
}