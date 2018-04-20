using System;
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
        private CommandData _currentCommand;
        private object execLock = new object();

        public bool IsWaitingForAnswer { get; private set; }
        public bool IsReadingCommand { get; private set; }

        private bool isLaserMarking;

        public bool IsLaserMarking
        {
            get { return isLaserMarking; }
            set
            {
                isLaserMarking = value;
                //if (isLaserMarking)
                //    StartPoll(DCConfig.Instance.AdamPollInterval, DCConfig.Instance.AdamErrorTimeout);
            }
        }

        public int IsTimedout { get; set; }

        public CommService(ICommunicationModule comm)
        {
            Guard.Against.Null(comm, nameof(comm));
            _comm = comm;
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
            _pollTimer_TimeSpan = pollinterval;
            try
            {
                _pollTimer = new System.Timers.Timer();
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

        public void StopPoll()
        {
            _pollTimer.Stop();
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

        private void _pollTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            bool lockCreated = false;
            try
            {
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
                            Thread.Sleep(DCConfig.Instance.AdamWaitAfterETX);
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
                return;
            }

            if (!IsLaserMarking)
            {
                _pollTimer.Start();
            }
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
                        RaiseStartMarkingEvent();
                        IsLaserMarking = true;
                    }
                    break;

                case CommandTypes.EndMarking:
                    if (_currentCommand.Params[0] == 49)
                    {
                        RaiseLaserEndEvent();
                    }
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
                result = ReadUntilNewData(_oldData, out data);      // _oldData is updated
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

        private bool ReadUntilNewData(byte oldData, out byte data)
        {
            bool result = true;
            //Debug.WriteLine("\tReadUntilNewData");
            data = 0;
            DateTime startTime = DateTime.Now;
            bool timeout = false;
            //Debug.WriteLine("\t\tReadUntilNewData oldData: {0}", oldData);

            do
            {
                data = _comm.Read(Constants.DIstartAddress, Constants.DItotalPoints);
                //Debug.WriteLine("\t\tReadUntilNewData: {0}", data);

                TimeSpan ts = DateTime.Now - startTime;
                //Debug.WriteLine("\t\tReadUntilNewData: TimeSpan: {0}", ts.TotalMilliseconds);
                if (ts.TotalMilliseconds > DCConfig.Instance.AdamErrorTimeout)
                {
                    timeout = true;
                    //RaiseErrorEvent("Timout occurred waiting for signal from PLC");
                }

                if (!timeout && data == oldData)
                {
                    Thread.Sleep(_pollTimer_TimeSpan);
                }
            } while (data == oldData && !timeout);

            _oldData = data;
            //Debug.WriteLine(string.Format("\tReadUntilNewData Returns: {0}", data));

            result = !timeout;
            return result;
        }

        private bool GetCommand(out byte data)
        {
            bool result = true;
            //Debug.WriteLine("\tGetCommand");
            data = 0;
            result = ReadUntilNewData(_oldData, out data);
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
            //Log.Trace("\tAcknowledgeRead");
            try
            {
                _comm.Write(Constants.DOstartAddress, data);
                do
                {
                    //Debug.WriteLine("\t\tAcknowledgeRead: Start time: {0}", startTime.ToLocalTime());
                    data = _comm.Read(Constants.DIstartAddress, Constants.DItotalPoints);
                    if (data == null)
                    {
                        return false;
                    }
                    DateTime endTime = DateTime.Now;
                    //Debug.WriteLine("\t\tAcknowledgeRead: End time: {0}", endTime.ToLocalTime());
                    TimeSpan ts = endTime - startTime;
                    //Debug.WriteLine("\t\tAcknowledgeRead: TimeSpan: {0}", ts.TotalMilliseconds);
                    if (ts.TotalMilliseconds > DCConfig.Instance.AdamErrorTimeout)
                    {
                        timeout = true;
                        //RaiseErrorEvent("Timout occurred waiting for signal from PLC");
                    }

                    if (!timeout && data != Constants.ACK)
                    {
                        Thread.Sleep(_pollTimer_TimeSpan);
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
                data = _comm.Read(Constants.DIstartAddress, Constants.DItotalPoints);
                Log.Trace(string.Format("Read Command from PLC: {0} - {1}", data, outData));

                TimeSpan ts = DateTime.Now - startTime;
                if (ts.TotalMilliseconds > DCConfig.Instance.AdamErrorTimeout)
                {
                    timeout = true;
                    //RaiseErrorEvent("Timout occurred waiting for signal from PLC");
                }

                if (!timeout && data != outData)
                {
                    Thread.Sleep(_pollTimer_TimeSpan);
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
                    data = _comm.Read(Constants.DIstartAddress, Constants.DItotalPoints);
                    Log.Trace(string.Format("Read ACK from PLC: {0}", data));
                    TimeSpan ts = DateTime.Now - startTime;
                    if (ts.TotalMilliseconds > DCConfig.Instance.AdamErrorTimeout)
                    {
                        timeout = true;
                        //RaiseErrorEvent("Timout occurred waiting for signal from PLC");
                    }

                    if (!timeout && data != Constants.ACK)
                    {
                        Thread.Sleep(_pollTimer_TimeSpan);
                    }
                } while (data != Constants.ACK && !timeout);
            }

            result = !timeout;
            Log.Trace(string.Format("result: {0}", result));
            return result;
        }

        public bool Write(ushort startAddress, byte data)
        {
            return _comm.Write(startAddress, data);
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