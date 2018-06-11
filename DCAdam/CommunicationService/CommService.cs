﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Contracts;
using Ardalis.GuardClauses;
using DCLog;
using System.Threading;
using System.Diagnostics;
using Configuration;
using System.Net.Sockets;

namespace CommunicationService
{
    public partial class CommService
    {
        private ICommunicationModule _comm;
        private System.Timers.Timer _pollTimer;
        private int _pollTimer_TimeSpan;
        private ArticleData _articleData;
        private volatile byte _oldData;

        /// <summary>
        /// All allowed commands
        /// </summary>
        private byte[] _allowedArray;

        /// <summary>
        /// All allowed params
        /// </summary>
        private byte[] _allowedParamArray;

#if FULLFILENAME
        /// <summary>
        /// Invalid chars for filename
        /// </summary>
        private byte[] _invalidCharsArray;
#endif

        /// <summary>
        /// Current command data
        /// </summary>
        private CommandData _currentCommand;

        private object execLock = new object();

        public bool IsWaitingForAnswer { get; private set; }
        public bool IsReadingCommand { get; private set; }

        public volatile bool IsLaserMarking;

        //public bool IsLaserMarking
        //{
        //    get { return isLaserMarking; }
        //    set
        //    {
        //        isLaserMarking = value;
        //        //if (isLaserMarking)
        //        //    StartPoll(DCConfig.Instance.AdamPollInterval, DCConfig.Instance.AdamErrorTimeout);
        //    }
        //}

        public int IsTimedout { get; set; }

        public CommService(ICommunicationModule comm)
        {
            Guard.Against.Null(comm, nameof(comm));
            _comm = comm;
            LoadAllowedValues();
        }

        private void LoadAllowedValues()
        {
            _allowedArray = new byte[] {
                  Constants.ArtNrCode,
                  Constants.OkCode,
                  Constants.StartMarkingCode,
                  Constants.RestartCode
            };

            // Only allow ETX and digits 0 - 6
            _allowedParamArray = new byte[] { 3, 48, 49, 50, 51, 52, 53, 54 };

#if FULLFILENAME
            _invalidCharsArray = new byte[] { chr("<"), chr(">"), chr(":"), chr("="), chr("\""), chr("/"), chr("\\"), chr("|"), chr("?"), chr("*") };
#endif
        }

        private byte chr(string s)
        {
            byte result = (byte)Convert.ToChar(s);

            return result;
        }

        ~CommService()
        {
            if (_pollTimer != null)
            {
                _pollTimer.Dispose();
            }
        }

        public bool Initialize()
        {
            _articleData = null;
            IsTimedout = 0;
            return _comm.Initialize();
        }

        public bool Connect()
        {
            bool result = false;
            if (_comm != null)
            {
                result = _comm.Connect();
            }

            return result;
        }

        public byte Read(ushort startAddress, ushort totalPoints)
        {
            return _comm.Read(startAddress, totalPoints);
        }

        public bool StartPoll(int pollinterval, int errortimout)
        {
            bool result = true;

#if DEBUGx
            string stackmsg = "|";
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
            for (int i = 0; i < st.FrameCount; i++)
            {
                // Note that high up the call stack, there is only
                // one stack frame.
                System.Diagnostics.StackFrame sf = st.GetFrame(i);
                string tmp = string.Format("{0} - {1} | ", sf.GetMethod(), sf.GetFileLineNumber());
                stackmsg += tmp;
            }

            Log.Trace(string.Format("StartPoll({0},{1}) {2}", pollinterval, errortimout, stackmsg));
#else
            Log.Trace(string.Format("StartPoll({0},{1})", pollinterval, errortimout));

#endif

            _pollTimer_TimeSpan = pollinterval;
            try
            {
                _pollTimer = new System.Timers.Timer();
                _pollTimer.Stop();
                _pollTimer.Interval = pollinterval;
                _pollTimer.AutoReset = false;
                _pollTimer.Elapsed += _pollTimer_Elapsed;
                _pollTimer.Start();
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        public bool StartPoll()
        {
            bool result = false;
#if DEBUGx
            string stackmsg = "|";
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
            for (int i = 0; i < st.FrameCount; i++)
            {
                // Note that high up the call stack, there is only
                // one stack frame.
                System.Diagnostics.StackFrame sf = st.GetFrame(i);
                string tmp = string.Format("{0} - {1} | ", sf.GetMethod(), sf.GetFileLineNumber());
                stackmsg += tmp;
            }

            Log.Trace(string.Format("StartPoll() {0})", stackmsg));
#else
            //Log.Trace("StartPoll()");

#endif

            if (_pollTimer != null)
            {
                _pollTimer.Stop();
                IsLaserMarking = false;
                _pollTimer.Start();
                result = true;
            }
            return result;
        }

        public void StopPoll()
        {
            if (_pollTimer != null)
            {
                _pollTimer.Stop();
            }
        }

        public void Simulate(string v)
        {
            _comm.LoadCommands(v);
        }

        public void Abort()
        {
            if (_pollTimer != null)
            {
                _pollTimer.Stop();
            }
        }

        public bool SendSetKant(byte kantNumber)
        {
            Log.Trace(string.Format("SetKant: {0}", kantNumber));
            bool result = true;
            byte asciiCode = ToAscii(kantNumber);
            result = SendToAdam(Constants.SetKantCode, asciiCode);

            return result;
        }

        private static byte ToAscii(byte number)
        {
            return (byte)((byte)number + (byte)48);
        }

        public bool SendReadyToMark(bool mode)
        {
            Log.Trace(string.Format("ReadyToMark: {0}", mode));
            bool result = true;

            result = SendToAdam(Constants.ReadyToMarkCode, ToAscii(mode ? (byte)1 : (byte)0));

            return result;
        }

        public bool SendBatchNotReady(bool done)
        {
            Log.Trace(string.Format("BatchNotReady: {0}", done));
            bool result = true;

            result = SendToAdam(Constants.BatchNotReadyCode, ToAscii(done ? (byte)1 : (byte)0));

            return result;
        }

        public bool SendError(byte errorCode)
        {
            Log.Trace(string.Format("SendError: {0}", errorCode));
            bool result = true;
            result = SendToAdam(Constants.ErrorCode, ToAscii(errorCode));

            return result;
        }

        private bool SendToAdam(byte command, byte data)
        {
            bool result = true;

            do
            {
                bool brc = SendStartCommand();
                if (brc)
                {
                    brc = Write(Constants.DOstartAddress, command);
                    if (brc)
                    {
                        brc = AcknowledgeWrite(command);
                        if (brc)
                        {
                            brc = Write(Constants.DOstartAddress, data);
                            if (brc)
                            {
                                brc = AcknowledgeWrite(data);
                                if (brc)
                                {
                                    brc = SendEndCommand();
                                    if (!brc)
                                    {
                                        result = false;
                                    }
                                }
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
                    else
                    {
                        result = false;
                    }
                }
                else
                {
                    result = false;
                }
                if (!result)
                {
                    IsTimedout++;
                }
            } while (!result && IsTimedout <= DCConfig.Instance.AdamAllowedTimeouts);

            if (IsTimedout > DCConfig.Instance.AdamAllowedTimeouts)
            {
                Log.Error(string.Format("Communication with PLC has timed out more than {0} times! Check PLC, ADAM module and restart DCMarker!", DCConfig.Instance.AdamAllowedTimeouts));
                RaiseErrorEvent(string.Format("Communication with PLC has timed out more than {0} times! Check PLC, ADAM module and restart DCMarker!", DCConfig.Instance.AdamAllowedTimeouts));
            }

            return result;
        }

        private bool SendEndCommand()
        {
            bool result = true;
            bool brc = Write(Constants.DOstartAddress, Constants.ETX);
            if (brc)
            {
                brc = AcknowledgeWrite(Constants.ETX);
                if (!brc)
                {
                    result = false;
                }
            }

            return result;
        }

        private bool SendStartCommand()
        {
            bool result = true;

            bool brc = Write(Constants.DOstartAddress, Constants.STX);
            if (brc)
            {
                brc = AcknowledgeWrite(Constants.STX);
                if (!brc)
                {
                    result = false;
                }
            }
            return result;
        }

        public void ReadCommand(byte command, string artno)
        {
            StopPoll();
            _comm.ReadCommand(command, artno);
            StartPoll();
        }

        public void ReadCommand(byte command, int _currentEdge, int _totalEdges)
        {
            StopPoll();
            _comm.ReadCommand(command, (byte)_currentEdge, _totalEdges);
            StartPoll();
        }

        private void _pollTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            bool lockCreated = false;

            try
            {
                StopPoll();
                Monitor.TryEnter(execLock, DCConfig.Instance.AdamErrorTimeout, ref lockCreated);
                if (lockCreated)
                {
                    byte data = _comm.Read(Constants.DIstartAddress, Constants.DItotalPoints);
                    if (_oldData != data)
                    {
                        _oldData = data;
                        if (data == Constants.STX)
                        {
                            Log.Trace("StartCommand =========================");
                            // TODO: change return value to System.TimeoutException!
                            _currentCommand = new CommandData();
                            bool brc = AcknowledgeRead(data);
                            if (brc)
                            {
                                brc = GetCommand(out data);
                                if (brc)
                                {
                                    brc = AcknowledgeRead(data);
                                    if (brc)
                                    {
                                        brc = GetParam();
                                        if (brc)
                                        {
                                            ExecuteCommand(_currentCommand);
                                        }
                                        else
                                        {
                                            Log.Trace("GetParam Timedout");
                                            IsTimedout++;
                                        }
                                    }
                                    else
                                    {
                                        Log.Trace("GetAcknowledgeRead2 Timedout");
                                        IsTimedout++;
                                    }
                                }
                                else
                                {
                                    Log.Trace("GetCommand Timedout");
                                    IsTimedout++;
                                }
                            }
                            else
                            {
                                Log.Trace("AcknowledgeRead Timedout");
                                IsTimedout++;
                            }
                        }
                    }
                }
            }
            finally
            {
                // Ensure that the lock is released.
                if (lockCreated)
                {
                    IsTimedout = 0;
                    Monitor.Exit(execLock);
                    //Log.Trace("Monitor.Exit(execLock);");
                }
                else
                {
                    IsTimedout++;
                }
            }

            if (IsTimedout > DCConfig.Instance.AdamAllowedTimeouts)
            {
                string msg = string.Format("Communication with PLC has timed out more than {0} times! Check PLC, ADAM module and restart DCMarker!", DCConfig.Instance.AdamAllowedTimeouts);
                Log.Fatal(msg);
                RaiseErrorEvent(msg);
                _comm.Write(Constants.DOstartAddress, 0);
                //Abort();
                return;
            }

            //Log.Trace(string.Format("IsLaserMarking: {0} - TimerEnabled: {1}", IsLaserMarking, _pollTimer.Enabled));
            if (!IsLaserMarking)
            {
                StartPoll();
            }
        }

        public void SetArticleData(string artno)
        {
            Log.Trace("CommandTypes.ArtNo");
            _articleData = new ArticleData();
            _articleData.ArticleNumber = artno;
            RaiseArticleEvent(_articleData);
        }

        private void ExecuteCommand(CommandData _currentCommand)
        {
            string msg = string.Format("ExecuteCommand: {0}", Enum.GetName(typeof(CommandTypes), _currentCommand.Type));
            Log.Debug(msg);

            switch (_currentCommand.Type)
            {
                case CommandTypes.None:
                    break;

                case CommandTypes.ArtNo:
                    Log.Trace("CommandTypes.ArtNo");
                    _articleData = new ArticleData();
                    Encoding encoding = Encoding.GetEncoding(850);
                    _articleData.ArticleNumber = encoding.GetString(_currentCommand.Params.ToArray());
                    RaiseArticleEvent(_articleData);
                    break;

                case CommandTypes.OK:
                    Log.Trace(string.Format("OK: Param: {0} - articledata: {1} ", _currentCommand.Params[0], _articleData));
                    if (_articleData == null)
                    {
                        SendError((byte)Errors.LayoutNotDefined);
                    }
                    else if (_currentCommand.Params[0] == 49)
                    {
                        RaiseItemInPlaceEvent();
                    }
                    break;

                case CommandTypes.StartMarking:
                    if (_currentCommand.Params[0] == 49)
                    {
                        IsLaserMarking = true;
                        RaiseStartMarkingEvent();
                    }
                    break;

                case CommandTypes.EndMarking:
                    Log.Error("Command EndMarking (15) should not be called! Check again with PLC programmer!!!!");
                    //if (_currentCommand.Params[0] == 49)
                    //{
                    //    RaiseLaserEndEvent();
                    //}
                    break;

                case CommandTypes.Restart:
                    if (_currentCommand.Params[0] == 49)
                    {
                        RaiseRestartEvent();
                    }
                    break;

                case CommandTypes.Undefined:
                    Log.Error(string.Format("Undefined Command: {0}", _currentCommand.Type));
                    SendError((byte)Errors.UnknownCommand);
                    break;

                default:
                    Log.Error(string.Format("Unknown command: {0}", _currentCommand.Type));
                    SendError((byte)Errors.UnknownCommand);
                    break;
            }

            Log.Trace("ExecuteCommand Done");
        }

        private bool GetParam()
        {
            bool result = true;
            byte data;
            //Debug.WriteLine("\tGetParam");
            do
            {
                result = ReadParamUntilNewData(_oldData, out data);      // _oldData is updated
                if (data != Constants.ETX)
                {
                    _currentCommand.Params.Add(data);
                    //Debug.WriteLine(string.Format("GetParam: {0}", data));
                }
                result = AcknowledgeRead(data);       // _oldData is updated
            } while (result && data != Constants.ETX);
            Log.Trace("End Command ====================================");
            return result;
        }

        private bool ReadCommandUntilNewData(byte oldData, out byte data)
        {
            bool result = true;
            bool allowedValue = false;
            //Debug.WriteLine("\tReadUntilNewData");
            data = 0;
            DateTime startTime = DateTime.Now;
            bool timeout = false;
            Log.Trace(string.Format("ReadCommandUntilNewData oldData: {0}", oldData));

            do
            {
                Sleep(DCConfig.Instance.AdamWaitBeforeRead);
                data = _comm.Read(Constants.DIstartAddress, Constants.DItotalPoints);
                Log.Trace(string.Format("data: {0}", data));
                if (DCConfig.Instance.IsAdamErrorTimeoutActive)
                {
                    TimeSpan ts = DateTime.Now - startTime;
                    if (ts.TotalMilliseconds > DCConfig.Instance.AdamErrorTimeout)
                    {
                        timeout = true;
                    }
                }

                allowedValue = IsCommandAllowed(data);
            } while (!allowedValue && !timeout);

            _oldData = data;
            Log.Trace(string.Format("ReadCommandUntilNewData Returns: {0}", data));

            result = !timeout;
            return result;
        }

        private bool ReadParamUntilNewData(byte oldData, out byte data)
        {
            bool result = true;
            bool allowedValue = false;
            //Debug.WriteLine("\tReadUntilNewData");
            data = 0;
            DateTime startTime = DateTime.Now;
            bool timeout = false;
            Log.Trace(string.Format("ReadParamUntilNewData oldData: {0}", oldData));
            do
            {
                Sleep(DCConfig.Instance.AdamWaitBeforeRead);
                data = _comm.Read(Constants.DIstartAddress, Constants.DItotalPoints);
                Log.Trace(string.Format("data: {0}", data));
                if (DCConfig.Instance.IsAdamErrorTimeoutActive)
                {
                    TimeSpan ts = DateTime.Now - startTime;
                    if (ts.TotalMilliseconds > DCConfig.Instance.AdamErrorTimeout)
                    {
                        timeout = true;
                    }
                }
                allowedValue = IsParamAllowed(data);
            } while (!allowedValue && !timeout);

            _oldData = data;
            Log.Trace(string.Format("ReadParamUntilNewData Returns: {0}", data));

            result = !timeout;
            return result;
        }

        private bool IsCommandAllowed(byte data)
        {
            bool result = false;
            result = _allowedArray.Contains(data);

            return result;
        }

        private bool IsParamAllowed(byte data)
        {
            bool result = false;
            if (_currentCommand.Type == CommandTypes.ArtNo)
            {
                // the params are the layout name in ASCII which means that all characters are allowed that can be used as a filename...
                //
#if FULLFILENAME
                // result = IsValidFileChar(data);
#else
                // The PLC will only send digits! So I limit the data to 0 - 9
                if (data > 47 && data < 58)
                    result = true;
                else
                    result = false;
#endif
            }
            else
            {
                result = _allowedParamArray.Contains(data);
            }

            return result;
        }

#if FULLFILENAME
        private bool IsValidFileChar(byte data)
        {
            if (data < 32 || data == 127)
                return false;

            if (_invalidCharsArray.Contains(data))
                return false;

            return true;
        }
#endif

        private bool GetCommand(out byte data)
        {
            bool result = true;
            Log.Trace("GetCommand");
            data = 0;
            result = ReadCommandUntilNewData(_oldData, out data);
            if (result)
            {
                result = ParseData(data);
            }

            return result;
        }

        private bool ParseData(byte data)
        {
            bool result = true;
            //Debug.WriteLine("\tParseData");
            Log.Trace(string.Format("ParseData: {0}", data));
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
                    Log.Error(string.Format("Undefined command: {0}", data));
                    break;
            }
            Log.Trace(string.Format("End ParseData {0}", result));
            return result;
        }

        private bool AcknowledgeRead(byte? data)
        {
            bool result = true;
            DateTime startTime = DateTime.Now;
            bool timeout = false;
            Log.Trace("\tAcknowledgeRead");
            try
            {
                _comm.Write(Constants.DOstartAddress, data);
                do
                {
                    Sleep(DCConfig.Instance.AdamWaitBeforeRead);
                    data = _comm.Read(Constants.DIstartAddress, Constants.DItotalPoints);
                    Log.Trace(string.Format("data: {0}", data));
                    if (data == null)
                    {
                        return false;
                    }

                    if (DCConfig.Instance.IsAdamErrorTimeoutActive)
                    {
                        DateTime endTime = DateTime.Now;
                        TimeSpan ts = endTime - startTime;

                        if (ts.TotalMilliseconds > DCConfig.Instance.AdamErrorTimeout)
                        {
                            timeout = true;
                            //RaiseErrorEvent("Timout occurred waiting for signal from PLC");
                        }
                    }
                } while (data != Constants.ACK && !timeout);
                //Debug.WriteLine("\t\ttimeout: {0}", timeout);
                if (!timeout)
                {
                    _oldData = data.Value;
                    _comm.Write(Constants.DOstartAddress, data);
                }
                result = !timeout;
            }
            catch (SocketException ex)
            {
                Log.Error(ex, "Error communicating with ADAM Module");
                result = false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unknown Error!");
                result = false;
            }

            return result;
        }

        private bool AcknowledgeWrite(byte outData)
        {
            bool result = true;
            DateTime startTime = DateTime.Now;
            bool timeout = false;
            byte data = Constants.ACK;
            Log.Trace(string.Format("AcknowledgeWrite: {0}", outData));

            // PLC Sends outData back

            do
            {
                Sleep(DCConfig.Instance.AdamWaitBeforeRead);
                data = _comm.Read(Constants.DIstartAddress, Constants.DItotalPoints);
                Log.Trace(string.Format("Read Command from PLC: Read: {0} - Waiting for: {1}", data, outData));
                if (DCConfig.Instance.IsAdamErrorTimeoutActive)
                {
                    TimeSpan ts = DateTime.Now - startTime;
                    if (ts.TotalMilliseconds > DCConfig.Instance.AdamErrorTimeout)
                    {
                        timeout = true;
                        //RaiseErrorEvent("Timout occurred waiting for signal from PLC");
                    }
                }
            } while (data != outData && !timeout);

            if (!timeout)
            {
                // Send ACK
                _comm.Write(Constants.DOstartAddress, Constants.ACK);
                Log.Trace("Send ACK");
                // Read it back
                do
                {
                    Sleep(DCConfig.Instance.AdamWaitBeforeRead);
                    data = _comm.Read(Constants.DIstartAddress, Constants.DItotalPoints);

                    Log.Trace(string.Format("Read ACK from PLC: {0}", data));

                    if (DCConfig.Instance.IsAdamErrorTimeoutActive)
                    {
                        TimeSpan ts = DateTime.Now - startTime;

                        if (ts.TotalMilliseconds > DCConfig.Instance.AdamErrorTimeout)
                        {
                            timeout = true;
                            //RaiseErrorEvent("Timout occurred waiting for signal from PLC");
                        }
                    }
                } while (data != Constants.ACK && !timeout);
            }

            result = !timeout;
            Log.Trace(string.Format("result: {0}", result));
            return result;
        }

        public bool Write(ushort startAddress, byte data)
        {
            Sleep(DCConfig.Instance.AdamWaitBeforeWrite);
            return _comm.Write(startAddress, data);
        }

        private static void Sleep(int w)
        {
            if (w > 0)
                Thread.Sleep(w);
        }

        #region Error Event

        public delegate void ErrorHandler(string msg);

        public event EventHandler<ErrorArgs> ErrorEvent;

        internal void RaiseErrorEvent(string msg)
        {
            var handler = ErrorEvent;
            if (handler != null)
            {
                var arg = new ErrorArgs(msg);
                handler(null, arg);
            }
        }

        #endregion Error Event
    }
}