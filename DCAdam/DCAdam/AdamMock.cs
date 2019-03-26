#define TEST

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Advantech.Adam;
using Configuration;
using DCLog;
using System.Net.Sockets;
using Contracts;
using System.IO;
using System.Diagnostics;
using Advantech.Common;
using System.Threading;

namespace DCAdam
{
    public partial class AdamMock : ICommunicationModule
    {
        private string _ipAddress;

        private int _ipPort = 502;
        private AdamSocket adamModbus;

        private LogTelegrams _log;

        private List<byte> SendCommands;
        private int _nextCommand;
        private object lockObj = new object();
        private byte _oldByte;

        private volatile bool IsAdamInProcess;

        public AdamMock()
        {
            _ipAddress = DCConfig.Instance.AdamIpAddress;
            _ipPort = DCConfig.Instance.AdamIpPort;
            SendCommands = new List<byte>();
            //InitializeSendCommands("COMMANDS.TXT");
            _nextCommand = 0;
            _log = new LogTelegrams();
        }

        private void InitializeSendCommands(string commandFile)
        {
            SendCommands = new List<byte>();
            using (StreamReader sr = new StreamReader(commandFile))
            {
                string line = string.Empty;
                do
                {
                    line = sr.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line) && line.Substring(0, 1) != "#")
                    {
                        byte cmd = Convert.ToByte(line);
                        SendCommands.Add(cmd);
                    }
                } while (!string.IsNullOrWhiteSpace(line));
            }
        }

        public AdamMock(string ipAddress, ushort ipPort)
        {
            _ipAddress = ipAddress;
            _ipPort = ipPort;
            //InitializeSendCommands("COMMANDS.TXT");
            _nextCommand = 0;
            _log = new LogTelegrams();
        }

        ~AdamMock()
        {
        }

        public void LoadCommands(string commandFile)
        {
            InitializeSendCommands(commandFile);
            _nextCommand = 0;
        }

        public bool Connect()
        {
#if TEST
            return true;
#else
            bool result = false;
            try
            {
                //result = adamModbus.Connect(_ipAddress, ProtocolType.Tcp, _ipPort);
                result = adamModbus.Connect(AdamType.Adam6000, _ipAddress, ProtocolType.Tcp);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Can't connect to ADAM module");
                result = false;
                throw;
            }

            return result;
#endif
        }

        public bool Initialize()
        {
            bool result = true;

            try
            {
                IsAdamInProcess = false;
                adamModbus = new AdamSocket();
                int t = DCConfig.Instance.AdamErrorTimeout;
                adamModbus.SetTimeout(t, t, t);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Error initializing ADAM module!");
                throw;
            }

            return result;
        }

        public void ReadCommand(byte command, string artno)
        {
            if (command == (byte)CommandTypes.ArtNo)
            {
                SendCommands = new List<byte>();
                _nextCommand = 0;

                SendCommands.Add(Constants.STX);
                SendCommands.Add(Constants.ACK);
                SendCommands.Add(command);
                SendCommands.Add(Constants.ACK);

                // params == Artno
                Encoding cp850 = Encoding.GetEncoding(850);
                byte[] arr = cp850.GetBytes(artno);
                for (int i = 0; i < arr.Length; i++)
                {
                    SendCommands.Add(arr[i]);
                    SendCommands.Add(Constants.ACK);
                }

                SendCommands.Add(Constants.ETX);
                SendCommands.Add(Constants.ACK);

                //// Start/OK
                //SendCommands.Add(Constants.STX);
                //SendCommands.Add(Constants.ACK);
                //SendCommands.Add(Constants.OkCode);
                //SendCommands.Add(Constants.ACK);
                //SendCommands.Add(49);
                //SendCommands.Add(Constants.ACK);
                //SendCommands.Add(Constants.ETX);
                //SendCommands.Add(Constants.ACK);

                //// reply on PC SetKant
                //SendCommands.Add(Constants.STX);
                //SendCommands.Add(Constants.ACK);
                //SendCommands.Add(Constants.SetKantCode);
                //SendCommands.Add(Constants.ACK);
                //SendCommands.Add(49);
                //SendCommands.Add(Constants.ACK);
                //SendCommands.Add(Constants.ETX);
                //SendCommands.Add(Constants.ACK);

                //// reply on PC BatchNotReady
                //SendCommands.Add(Constants.STX);
                //SendCommands.Add(Constants.ACK);
                //SendCommands.Add(Constants.BatchNotReadyCode);
                //SendCommands.Add(Constants.ACK);
                //SendCommands.Add(49);
                //SendCommands.Add(Constants.ACK);
                //SendCommands.Add(Constants.ETX);
                //SendCommands.Add(Constants.ACK);

                //// reply on PC ReadyToMark
                //SendCommands.Add(Constants.STX);
                //SendCommands.Add(Constants.ACK);
                //SendCommands.Add(Constants.ReadyToMarkCode);
                //SendCommands.Add(Constants.ACK);
                //SendCommands.Add(49);
                //SendCommands.Add(Constants.ACK);
                //SendCommands.Add(Constants.ETX);
                //SendCommands.Add(Constants.ACK);
            }
        }

        public void ReadCommand(byte command, byte edge, int totalEdges)
        {
            if (command == (byte)CommandTypes.OK)
            {
                SendCommands = new List<byte>();
                _nextCommand = 0;

                SendCommands.Add(Constants.STX);
                SendCommands.Add(Constants.ACK);
                SendCommands.Add(Constants.OkCode);
                SendCommands.Add(Constants.ACK);
                // aik
                SendCommands.Add(65);
                SendCommands.Add(73);
                SendCommands.Add(75);
                SendCommands.Add(Constants.ACK);
                SendCommands.Add(Constants.ETX);
                SendCommands.Add(Constants.ACK);

                // reply on PC SetKant
                SendCommands.Add(Constants.STX);
                SendCommands.Add(Constants.ACK);
                SendCommands.Add(Constants.SetKantCode);
                SendCommands.Add(Constants.ACK);
                SendCommands.Add(49);
                SendCommands.Add(Constants.ACK);
                SendCommands.Add(Constants.ETX);
                SendCommands.Add(Constants.ACK);

                // reply on PC BatchNotReady
                SendCommands.Add(Constants.STX);
                SendCommands.Add(Constants.ACK);
                SendCommands.Add(Constants.BatchNotReadyCode);
                SendCommands.Add(Constants.ACK);
                SendCommands.Add(49);
                SendCommands.Add(Constants.ACK);
                SendCommands.Add(Constants.ETX);
                SendCommands.Add(Constants.ACK);

                // reply on PC ReadyToMark
                SendCommands.Add(Constants.STX);
                SendCommands.Add(Constants.ACK);
                SendCommands.Add(Constants.ReadyToMarkCode);
                SendCommands.Add(Constants.ACK);
                SendCommands.Add(49);
                SendCommands.Add(Constants.ACK);
                SendCommands.Add(Constants.ETX);
                SendCommands.Add(Constants.ACK);
            }
            else if (command == (byte)CommandTypes.StartMarking)
            {
                SendCommands = new List<byte>();
                _nextCommand = 0;

                // Start marking command
                SendCommands.Add(Constants.STX);
                SendCommands.Add(Constants.ACK);
                SendCommands.Add(Constants.StartMarkingCode);
                SendCommands.Add(Constants.ACK);
                SendCommands.Add(49);
                SendCommands.Add(Constants.ACK);
                SendCommands.Add(Constants.ETX);
                SendCommands.Add(Constants.ACK);

                // reply on PC ReadyToMark 0
                SendCommands.Add(Constants.STX);
                SendCommands.Add(Constants.ACK);
                SendCommands.Add(Constants.ReadyToMarkCode);
                SendCommands.Add(Constants.ACK);
                SendCommands.Add(48);
                SendCommands.Add(Constants.ACK);
                SendCommands.Add(Constants.ETX);
                SendCommands.Add(Constants.ACK);

                // Marking is executed

                // reply on PC SetKant
                SendCommands.Add(Constants.STX);
                SendCommands.Add(Constants.ACK);
                SendCommands.Add(Constants.SetKantCode);
                SendCommands.Add(Constants.ACK);
                edge = (byte)(edge + (byte)48 + 1);
                SendCommands.Add(edge);
                SendCommands.Add(Constants.ACK);
                SendCommands.Add(Constants.ETX);
                SendCommands.Add(Constants.ACK);

                // reply on PC ReadyToMark
                SendCommands.Add(Constants.STX);
                SendCommands.Add(Constants.ACK);
                SendCommands.Add(Constants.ReadyToMarkCode);
                SendCommands.Add(Constants.ACK);
                SendCommands.Add(49);
                SendCommands.Add(Constants.ACK);
                SendCommands.Add(Constants.ETX);
                SendCommands.Add(Constants.ACK);
            }
            else if (command == (byte)CommandTypes.StartMarking2)
            {
                SendCommands = new List<byte>();
                _nextCommand = 0;

                SendCommands.Add(Constants.STX);
                SendCommands.Add(Constants.ACK);
                SendCommands.Add((byte)CommandTypes.StartMarking);
                SendCommands.Add(Constants.ACK);
                SendCommands.Add(49);
                SendCommands.Add(Constants.ACK);
                SendCommands.Add(Constants.ETX);
                SendCommands.Add(Constants.ACK);

                // reply on PC ReadyToMark 0
                SendCommands.Add(Constants.STX);
                SendCommands.Add(Constants.ACK);
                SendCommands.Add(Constants.ReadyToMarkCode);
                SendCommands.Add(Constants.ACK);
                SendCommands.Add(48);
                SendCommands.Add(Constants.ACK);
                SendCommands.Add(Constants.ETX);
                SendCommands.Add(Constants.ACK);

                // reply on PC BatchNotReady
                SendCommands.Add(Constants.STX);
                SendCommands.Add(Constants.ACK);
                SendCommands.Add(Constants.BatchNotReadyCode);
                SendCommands.Add(Constants.ACK);
                SendCommands.Add(48);
                SendCommands.Add(Constants.ACK);
                SendCommands.Add(Constants.ETX);
                SendCommands.Add(Constants.ACK);
            }
            else
            {
                Log.Error(string.Format("An Unvalid command! {0}", command));
                SendCommands = new List<byte>();
                _nextCommand = 0;
            }
            Log.Trace(string.Format("{0} - SendCommands: {1}", command, string.Join("|", SendCommands)));
        }

        /// <summary>
        /// If we mock this function then the rest should work perfectly?
        /// </summary>
        /// <param name="startAddress"></param>
        /// <param name="totalPoints"></param>
        /// <returns></returns>
        public byte Read(ushort startAddress, ushort totalPoints)
        {
            byte result = 0;

            try
            {
                result = ReadCoilsMock(startAddress, totalPoints);
                _log.WriteIn(_nextCommand.ToString() + "|" + result.ToString());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error reading data");
                throw;
            }

            return result;
        }

        private byte ReadCoilsMock(ushort startAddress, ushort totalPoints)
        {
            byte result = 0;
            lock (lockObj)
            {
                if (_nextCommand < SendCommands.Count)
                {
#if DEBUG
                    string stackmsg = "|";
                    string message = string.Empty;
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                    for (int i = 0; i < st.FrameCount; i++)
                    {
                        // Note that high up the call stack, there is only
                        // one stack frame.
                        System.Diagnostics.StackFrame sf = st.GetFrame(i);
                        string tmp = string.Format("{0} - {1} | ", sf.GetMethod(), sf.GetFileLineNumber());
                        stackmsg += tmp;
                    }
                    message += stackmsg;
                    Log.Trace(string.Format("Stack Trace {0}", message));
                    if (_nextCommand == 16)
                        message = "hello";
#endif
                    Log.Trace(string.Format("NextCommand: {0}", _nextCommand));
                    result = SendCommands[_nextCommand++];
                    Log.Trace(string.Format("From PLC: {0}", result));
                    _oldByte = result;
                }
                else
                {
                    SendCommands.Clear();
                    result = _oldByte;
                }
            }
            return result;
        }

        public bool[] SetValue(byte v)
        {
            bool[] result;

            result = ConvertByteToBoolArray(v);

            return result;
        }

        public bool Write(ushort startAddress, byte? data)
        {
            bool result = true;
#if !TEST
            try
            {
                bool[] dataArr = ConvertByteToBoolArray(data.Value);
                result = WriteCoils(startAddress, dataArr);
                _log.WriteOut(_nextCommand.ToString() + "|" + data.Value.ToString());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unknown Exception");
                throw;
            }
#endif
            return result;
        }

        private static bool[] ConvertByteToBoolArray(byte b)
        {
            // prepare the return result
            bool[] result = new bool[8];

            // check each bit in the byte. if 1 set to true, if 0 set to false
            for (int i = 0; i < 8; i++)
                result[i] = (b & (1 << i)) == 0 ? false : true;

            // reverse the array
            Array.Reverse(result);

            return result;
        }

        private byte ConvertBoolArrayToByte(bool[] source)
        {
            byte result = 0;
            // This assumes the array never contains more than 8 elements!
            int index = 8 - source.Length;

            // Loop through the array
            foreach (bool b in source)
            {
                // if the element is 'true' set the bit at that position
                if (b)
                    result |= (byte)(1 << (7 - index));

                index++;
            }

            return result;
        }

        private bool[] ReadCoils(ushort startAddress, ushort numberOfPoints)
        {
            bool[] result = null;
            try
            {
                bool brc = false;
                DateTime startTime = DateTime.Now;
                while (IsAdamInProcess && !CheckTimeout(startTime))
                {
                    Thread.Sleep(DCConfig.Instance.AdamPollInterval);
                }

                if (!IsAdamInProcess)
                {
                    IsAdamInProcess = true;
                    CheckConnection();
                    brc = adamModbus.Modbus().ReadCoilStatus(startAddress, numberOfPoints, out result);
                    IsAdamInProcess = false;
                }
                if (!brc)
                {
                    result = null;
                }
            }
            catch (SocketException ex)
            {
                ErrorCode ecode = adamModbus.LastError;
                Log.Error(ex, string.Format("Error Reading from ADAM Module (error: {0})", GetErrorMessage(ecode)));
                result = null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unknown Exception");
                throw;
            }

            return result;
        }

        private uint[] ReadHoldingRegisters(int startAddress, int numberOfPoints)
        {
            uint[] result = null;
            try
            {
                bool brc = false;
                DateTime startTime = DateTime.Now;
                while (IsAdamInProcess && !CheckTimeout(startTime))
                {
                    Thread.Sleep(DCConfig.Instance.AdamPollInterval);
                }

                if (!IsAdamInProcess)
                {
                    IsAdamInProcess = true;
                    CheckConnection();
                    brc = adamModbus.Modbus().ReadHoldingRegs(startAddress, numberOfPoints, out result);
                    IsAdamInProcess = false;
                }
                if (!brc)
                {
                    result = null;
                }
            }
            catch (SocketException ex)
            {
                ErrorCode ecode = adamModbus.LastError;
                Log.Error(ex, string.Format("Error Reading from ADAM Module (error: {0})", GetErrorMessage(ecode)));
                result = null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unknown Exception");
                throw;
            }

            return result;
        }

        private bool WriteCoil(ushort startAddress, bool value)
        {
            bool result = false;
            try
            {
                DateTime startTime = DateTime.Now;
                while (IsAdamInProcess && !CheckTimeout(startTime))
                {
                    Thread.Sleep(DCConfig.Instance.AdamPollInterval);
                }

                if (!IsAdamInProcess)
                {
                    IsAdamInProcess = true;
                    CheckConnection();
                    result = adamModbus.Modbus().ForceSingleCoil(startAddress, value);
                    IsAdamInProcess = false;
                }
            }
            catch (SocketException ex)
            {
                ErrorCode ecode = adamModbus.LastError;
                Log.Error(ex, string.Format("Error Writing to ADAM Module (error: {0})", GetErrorMessage(ecode)));
                result = false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unknown Exception");
                throw;
            }
            return result;
        }

        private bool WriteCoils(ushort startAddress, bool[] values)
        {
            bool result = false;
            try
            {
                DateTime startTime = DateTime.Now;
                while (IsAdamInProcess && !CheckTimeout(startTime))
                {
                    Thread.Sleep(DCConfig.Instance.AdamPollInterval);
                }

                if (!IsAdamInProcess)
                {
                    IsAdamInProcess = true;

                    bool brc = CheckConnection();

                    result = adamModbus.Modbus().ForceMultiCoils(startAddress, values);

                    IsAdamInProcess = false;
                }
            }
            catch (SocketException ex)
            {
                ErrorCode ecode = adamModbus.LastError;
                Log.Error(ex, string.Format("Error Writing to ADAM Module (error: {0})", GetErrorMessage(ecode)));
                result = false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unknown Exception");
                throw;
            }
            return result;
        }

        private bool CheckConnection()
        {
            bool result = true;
            if (!adamModbus.Connected)
            {
                result = Connect();
            }

            return result;
        }

        private bool CheckTimeout(DateTime startTime)
        {
            bool result = false;

            TimeSpan ts = DateTime.Now - startTime;
            if (ts.TotalMilliseconds > DCConfig.Instance.AdamErrorTimeout)
            {
                result = true;
            }

            return result;
        }

        private string GetErrorMessage(ErrorCode ecode)
        {
            string result = "Unknown error";

            result = ecode.ToString();

            return result;
        }
    }
}