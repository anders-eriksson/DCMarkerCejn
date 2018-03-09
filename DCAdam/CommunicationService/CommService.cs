using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Contracts;
using Ardalis.GuardClauses;

namespace CommunicationService
{
    public class CommService
    {
        private ICommunicationModule _comm;
        private System.Timers.Timer _pullTimer;
        private System.Timers.Timer _queueTimer;
        private CommandTypes _currentCommandType;
        private Command _command;
        private CommunicationStates _state;
        private CommandQueue _queue;

        public bool IsWaitingForAnswer { get; private set; }
        public bool IsReadingCommand { get; private set; }

        public CommService(ICommunicationModule comm)
        {
            Guard.Against.Null(comm, nameof(comm));
            _comm = comm;
        }

        public bool Initialize()
        {
            _currentCommandType = CommandTypes.None;
            return _comm.Initialize();
        }

        public bool Connect()
        {
            return _comm.Connect();
        }

        public byte Read(ushort startAddress, ushort totalPoints)
        {
            return _comm.Read(startAddress, totalPoints);
        }

        public bool StartPoll(int timespan, int timeout)
        {
            bool result = true;

            try
            {
                _pullTimer = new System.Timers.Timer();
                _pullTimer.Interval = timespan;
                _pullTimer.Elapsed += _pollTimer_Elapsed;
                _pullTimer.Start();
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        public bool StartQueue(int timespan, int timeout)
        {
            bool result = true;

            try
            {
                _queueTimer = new System.Timers.Timer();
                _queueTimer.Interval = timespan;
                _queueTimer.Elapsed += _queueTimer_Elapsed;
                _queueTimer.Start();
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        private void _queueTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _queueTimer.Stop();
            if (!_queue.IsEmpty())
            {
                Command command = _queue.Dequeue();
                if (command != null)
                {
                    command.Run();
                }
            }
            _queueTimer.Start();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _pollTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            byte data = _comm.Read(Constants.DIstartAddress, Constants.DItotalPoints);

            if (IsWaitingForAnswer)
            {
                if (data == Constants.ACK)
                {
                    _comm.Write(Constants.DOstartAddress, Constants.ACK);
                    IsWaitingForAnswer = false;
                    if (_state == CommunicationStates.Done)
                    {
                        ExecuteCommand(_command);
                    }
                }
                else
                {
                    // TODO: check that the data is the same as the one we sent!

                    _comm.Write(Constants.DOstartAddress, Constants.ACK);
                    IsWaitingForAnswer = true;
                }
            }
            else
            {
                if (_currentCommandType == CommandTypes.None)
                {
                    switch (data)
                    {
                        case Constants.STX:
                            // New command
                            _comm.Write(Constants.DOstartAddress, Constants.STX);
                            IsReadingCommand = true;
                            IsWaitingForAnswer = true;
                            _state = CommunicationStates.Creating;
                            break;

                        case Constants.ETX:
                            // End of command
                            _comm.Write(Constants.DOstartAddress, Constants.ETX);
                            IsReadingCommand = false;
                            IsWaitingForAnswer = true;
                            _state = CommunicationStates.Done;
                            break;

                        case Constants.ArtNrCode:
                            // Load article
                            _currentCommandType = CommandTypes.ArtNo;
                            _comm.Write(Constants.DOstartAddress, data);
                            IsWaitingForAnswer = true;
                            _command = new ArtNoCommand();
                            break;

                        case Constants.ProvbitCode:
                            _currentCommandType = CommandTypes.Provbit;
                            _comm.Write(Constants.DOstartAddress, data);
                            IsWaitingForAnswer = true;
                            _command = new ProvbitCommand();
                            break;

                        case Constants.OkCode:
                            _currentCommandType = CommandTypes.OK;
                            _comm.Write(Constants.DOstartAddress, data);
                            IsWaitingForAnswer = true;
                            _command = new OkCommand();
                            break;

                        case Constants.ItemInPlaceCode:
                            _currentCommandType = CommandTypes.ItemInPlace;
                            _comm.Write(Constants.DOstartAddress, data);
                            IsWaitingForAnswer = true;
                            _command = new ItemInPlaceCommand();
                            break;

                        case Constants.StartMarkingCode:
                            _currentCommandType = CommandTypes.StartMarking;
                            _comm.Write(Constants.DOstartAddress, data);
                            IsWaitingForAnswer = true;
                            _command = new StartMarkingCommand();
                            break;

                        case Constants.EndMarkingCode:
                            _currentCommandType = CommandTypes.EndMarking;
                            _comm.Write(Constants.DOstartAddress, data);
                            IsWaitingForAnswer = true;
                            _command = new EndMarkingCommand();
                            break;

                        case Constants.RestartCode:
                            _currentCommandType = CommandTypes.Restart;
                            _comm.Write(Constants.DOstartAddress, data);
                            IsWaitingForAnswer = true;
                            _command = new RestartApplicationCommand();
                            break;

                        default:
                            break;
                    }
                }
                // It's a parameter!
                else if (_currentCommandType == CommandTypes.ArtNo)
                {
                    char ch = Convert.ToChar(data);
                    ArtNoCommand cmd = (ArtNoCommand)_command;
                    cmd.Parameter += ch;
                    IsWaitingForAnswer = true;
                }
                else if (_currentCommandType == CommandTypes.Provbit)
                {
                    bool result = Convert.ToBoolean(data);
                    ProvbitCommand cmd = (ProvbitCommand)_command;
                    cmd.Parameter = result;
                    IsWaitingForAnswer = true;
                }
                else if (_currentCommandType == CommandTypes.OK)
                {
                    bool result = Convert.ToBoolean(data);
                    OkCommand cmd = (OkCommand)_command;
                    cmd.Parameter = result;
                    IsWaitingForAnswer = true;
                }
                else if (_currentCommandType == CommandTypes.ItemInPlace)
                {
                    bool result = Convert.ToBoolean(data);
                    ItemInPlaceCommand cmd = (ItemInPlaceCommand)_command;
                    cmd.Parameter = result;
                    IsWaitingForAnswer = true;
                }
                else if (_currentCommandType == CommandTypes.StartMarking)
                {
                    bool result = Convert.ToBoolean(data);
                    StartMarkingCommand cmd = (StartMarkingCommand)_command;
                    cmd.Parameter = result;
                    IsWaitingForAnswer = true;
                }
                else if (_currentCommandType == CommandTypes.EndMarking)
                {
                    bool result = Convert.ToBoolean(data);
                    EndMarkingCommand cmd = (EndMarkingCommand)_command;
                    cmd.Parameter = result;
                    IsWaitingForAnswer = true;
                }
                else if (_currentCommandType == CommandTypes.Restart)
                {
                    bool result = Convert.ToBoolean(data);
                    RestartApplicationCommand cmd = (RestartApplicationCommand)_command;
                    cmd.Parameter = result;
                    IsWaitingForAnswer = true;
                }
            }
        }

        private void ExecuteCommand(Command command)
        {
            _queue.Enqueue(command);
        }

        public bool Write(ushort startAddress, byte data)
        {
            return _comm.Write(startAddress, data);
        }
    }
}