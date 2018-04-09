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
        private byte[] _byConfig;

        private int _ipPort = 502;
        private AdamSocket adamModbus;

        private StreamReader _inputFile;

        private LogTelegrams _log;

        private List<byte> SendCommands;

        private volatile bool IsAdamInProcess;

        public AdamMock()
        {
            _ipAddress = DCConfig.Instance.AdamIpAddress;
            _ipPort = DCConfig.Instance.AdamIpPort;
            InitializeSendCommands();

            _log = new LogTelegrams();
        }

        private void InitializeSendCommands()
        {
            SendCommands = new List<byte>();
            using (StreamReader sr = new StreamReader("Commands.txt"))
            {
                string line = string.Empty;
                do
                {
                    line = sr.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        byte cmd = Convert.ToByte(line);
                        SendCommands.Add(cmd);
                    }
                } while (string.IsNullOrWhiteSpace(line));
            }
        }

        public AdamMock(string ipAddress, ushort ipPort)
        {
            _ipAddress = ipAddress;
            _ipPort = ipPort;

            _log = new LogTelegrams();
        }

        ~AdamMock()
        {
        }

        public bool Connect()
        {
            bool result = false;
            try
            {
                //result = adamModbus.Connect(_ipAddress, ProtocolType.Tcp, _ipPort);
                result = adamModbus.Connect(AdamType.Adam6000, _ipAddress, ProtocolType.Tcp);
            }
            catch (Exception)
            {
                throw;
            }

            return result;
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
                _log.WriteIn(result.ToString());
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

            // trigger timeout
            //int t = DCConfig.Instance.AdamErrorTimeout + 100;
            //Debug.WriteLine(string.Format("Wait: {0}", t));
            //Thread.Sleep(t);
            //Debug.WriteLine("Wait done");

            string line = _inputFile.ReadLine();
            while (!string.IsNullOrWhiteSpace(line) && line.Substring(0, 1) == "#")
            {
                line = _inputFile.ReadLine();
            }

            if (!string.IsNullOrWhiteSpace(line))
            {
                Debug.WriteLine(string.Format("\t\tLine: {0}", line));
                result = Convert.ToByte(line);
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
            try
            {
                bool[] dataArr = ConvertByteToBoolArray(data.Value);
                result = WriteCoils(startAddress, dataArr);
                _log.WriteOut(data.Value.ToString());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unknown Exception");
                throw;
            }

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