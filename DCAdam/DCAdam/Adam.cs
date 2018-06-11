#define TRYLOCK

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Advantech.Adam;
using Configuration;
using DCLog;
using System.Net.Sockets;
using Contracts;
using Advantech.Common;
using System.Threading;

namespace DCAdam
{
    public partial class Adam : ICommunicationModule
    {
        private string _ipAddress;
        private byte[] _byConfig;
        private System.Timers.Timer _timeoutTimer;
        private LogTelegrams _log;
        private object lockObj = new object();
        private volatile bool IsAdamInProcess;

        private int _ipPort = 502;
        private AdamSocket adamModbus;

        public Adam()
        {
            _ipAddress = DCConfig.Instance.AdamIpAddress;
            _ipPort = DCConfig.Instance.AdamIpPort;
            _log = new LogTelegrams();
        }

        public Adam(string ipAddress, ushort ipPort)
        {
            _ipAddress = ipAddress;
            _ipPort = ipPort;
            _log = new LogTelegrams();
        }

        ~Adam()
        {
            if (_timeoutTimer != null)
            {
                _timeoutTimer.Dispose();
            }
        }

        public bool Connect()
        {
            bool result = false;
            try
            {
                //result = adamModbus.Connect(_ipAddress, ProtocolType.Tcp, _ipPort);
                result = adamModbus.Connect(AdamType.Adam6000, _ipAddress, ProtocolType.Tcp);

                adamModbus.DigitalInput().GetIOConfig(out _byConfig);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Can't connect to ADAM Module");
                result = false; ;
            }

            return result;
        }

        public bool Initialize()
        {
            bool result = true;

            try
            {
                adamModbus = new AdamSocket();
                int t = DCConfig.Instance.AdamErrorTimeout;
                adamModbus.SetTimeout(t, t, t);

                _timeoutTimer = new System.Timers.Timer();
                _timeoutTimer.Elapsed += _timeoutTimer_Elapsed;
                _timeoutTimer.Interval = DCConfig.Instance.AdamPollInterval;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Error initializing ADAM module!");
                throw;
            }

            return result;
        }

        private void _timeoutTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public byte Read(ushort startAddress, ushort totalPoints)
        {
            byte result = 0;

            bool[] data;
            try
            {
                data = ReadCoils(startAddress, totalPoints);
                result = ConvertBoolArrayToByte(data);
                _log.WriteIn(result.ToString());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error reading data");
                throw;
            }

            return result;
        }

        public void ReadCommand(byte command, string artno)
        {
            throw new NotImplementedException();
        }

        public void ReadCommand(byte command, byte _currentEdge, int _totalEdges)
        {
            throw new NotImplementedException();
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
                _log.WriteOut(data.Value.ToString());
                bool[] dataArr = ConvertByteToBoolArray(data.Value);
                Log.Trace(string.Format("Data: {0} - Array: {1}", data, string.Join(",", dataArr)));
                result = WriteCoils(startAddress, dataArr);
            }
            catch (SocketException ex)
            {
                Log.Error(ex, string.Format("Error writing to ADAM module. Data: {0}", data));
                result = false;
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

            // reverse the array ?
            // Array.Reverse(result);

            return result;
        }

        private byte ConvertBoolArrayToByte(bool[] source)
        {
            byte result = 0;
            // This assumes the array never contains more than 8 elements!
            int index = 8 - source.Length;

            Array.Reverse(source);
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
#if TRYLOCK
                bool acquiredLock = false;
                try
                {
                    Monitor.TryEnter(lockObj, DCConfig.Instance.AdamErrorTimeout, ref acquiredLock);
                    if (acquiredLock)
                    {
                        CheckConnection();
                        bool brc = adamModbus.Modbus().ReadCoilStatus(startAddress, numberOfPoints, out result);
                        if (!brc)
                        {
                            result = null;
                        }
                    }
                    else
                    {
                        result = null;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Can't read from ADAM Module");
                    result = null;
                }
                finally
                {
                    if (acquiredLock)
                        Monitor.Exit(lockObj);
                }
#else
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
#endif
            }
            catch (SocketException ex)
            {
                Log.Error(ex, "Error Reading to ADAM Module");
                if (adamModbus != null)
                {
                    ErrorCode ecode = adamModbus.LastError;

                    Log.Error(string.Format("Error Message: {0})", GetErrorMessage(ecode)));
                }

                IsAdamInProcess = false;
                result = null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unknown Exception");
                IsAdamInProcess = false;
                throw;
            }

            return result;
        }

        private uint[] ReadHoldingRegisters(int startAddress, int numberOfPoints)
        {
            uint[] result = null;
            try
            {
#if TRYLOCK
                bool acquiredLock = false;
                try
                {
                    Monitor.TryEnter(lockObj, DCConfig.Instance.AdamErrorTimeout, ref acquiredLock);
                    if (acquiredLock)
                    {
                        CheckConnection();
                        bool brc = adamModbus.Modbus().ReadHoldingRegs(startAddress, numberOfPoints, out result);
                        if (!brc)
                        {
                            result = null;
                        }
                    }
                    else
                    {
                        result = null;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Can't read from ADAM Module");
                }
                finally
                {
                    if (acquiredLock)
                        Monitor.Exit(lockObj);
                }
#else
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
#endif
            }
            catch (SocketException ex)
            {
                Log.Error(ex, "Error Reading to ADAM Module");
                if (adamModbus != null)
                {
                    ErrorCode ecode = adamModbus.LastError;

                    Log.Error(string.Format("Error Message: {0})", GetErrorMessage(ecode)));
                }
                IsAdamInProcess = false;
                result = null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unknown Exception");
                IsAdamInProcess = false;
                throw;
            }

            return result;
        }

        private bool WriteCoil(ushort startAddress, bool value)
        {
            bool result = false;
            try
            {
#if TRYLOCK
                bool acquiredLock = false;
                try
                {
                    Monitor.TryEnter(lockObj, DCConfig.Instance.AdamErrorTimeout, ref acquiredLock);
                    if (acquiredLock)
                    {
                        CheckConnection();
                        result = adamModbus.Modbus().ForceSingleCoil(startAddress, value);
                        Log.Debug(string.Format("ForceSingleCoil result = {0}", result));
                    }
                    else
                    {
                        result = false;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Can't write to ADAM Module");
                }
                finally
                {
                    if (acquiredLock)
                        Monitor.Exit(lockObj);
                }
#else
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
#endif
            }
            catch (SocketException ex)
            {
                Log.Error(ex, "Error Writing to ADAM Module");
                if (adamModbus != null)
                {
                    ErrorCode ecode = adamModbus.LastError;

                    Log.Error(string.Format("Error Message: {0})", GetErrorMessage(ecode)));
                }
                IsAdamInProcess = false;
                result = false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unknown Exception");
                IsAdamInProcess = false;
                throw;
            }
            return result;
        }

        private bool WriteCoils(ushort startAddress, bool[] values)
        {
            bool result = false;
            //try
            //{
#if TRYLOCK
            bool acquiredLock = false;
            try
            {
                Monitor.TryEnter(lockObj, DCConfig.Instance.AdamErrorTimeout, ref acquiredLock);
                if (acquiredLock)
                {
                    CheckConnection();
                    result = adamModbus.Modbus().ForceMultiCoils(startAddress, values);
                    Log.Debug(string.Format("ForceMultiCoil result = {0}", result));
                }
                else
                {
                    result = false;
                }
            }
            catch (SocketException ex)
            {
                Log.Error(ex, "Error Writing to ADAM Module");
                if (adamModbus != null)
                {
                    ErrorCode ecode = adamModbus.LastError;

                    Log.Error(string.Format("Error Message: {0})", GetErrorMessage(ecode)));
                }

                IsAdamInProcess = false;
                result = false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unknown Exception");
                IsAdamInProcess = false;
                throw;
            }
            finally
            {
                if (acquiredLock)
                    Monitor.Exit(lockObj);
            }
#else
                DateTime startTime = DateTime.Now;
                while (IsAdamInProcess && !CheckTimeout(startTime))
                {
                    Thread.Sleep(DCConfig.Instance.AdamPollInterval);
                }

                if (!IsAdamInProcess)
                {
                    IsAdamInProcess = true;
                    CheckConnection();
                    result = adamModbus.Modbus().ForceMultiCoils(startAddress, values);
                    IsAdamInProcess = false;
                }
#endif
            //}
            //catch (SocketException ex)
            //{
            //    Log.Error(ex, "Error Writing to ADAM Module");
            //    if (adamModbus != null)
            //    {
            //        ErrorCode ecode = adamModbus.LastError;

            //        Log.Error(string.Format("Error Message: {0})", GetErrorMessage(ecode)));
            //    }

            //    IsAdamInProcess = false;
            //    result = false;
            //}
            //catch (Exception ex)
            //{
            //    Log.Error(ex, "Unknown Exception");
            //    IsAdamInProcess = false;
            //    throw;
            //}
            return result;
        }

        private bool DCForceMultiCoils(ushort startAddress, bool[] values)
        {
            bool result = true;
            Array.Reverse(values); ;

            foreach (bool b in values)
            {
                WriteCoil(startAddress, b);
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

        public void LoadCommands(string commandFile)
        {
            throw new NotImplementedException();
        }
    }
}