using Configuration;
using Contracts;
using DCTcpServer;
using System;
using CommunicationService;
using DCAdam;

namespace DCMarker.Model
{
    public class AdamArticleInput : IArticleInput
    {
        private DCConfig cfg;
        private CommService _server;
        private object lockObject = new object();
        private bool isLaserMarking;
        public bool IsLaserMarking { get { return isLaserMarking; } set { isLaserMarking = value; _server.IsLaserMarking = isLaserMarking; } }

        public AdamArticleInput()
        {
            try
            {
                cfg = DCConfig.Instance;
#if DEBUG
                ICommunicationModule commModule = new AdamMock();
#else
                ICommunicationModule commModule = new Adam();
#endif
                _server = new CommService(commModule);
                _server.ArticleEvent += _server_ArticleEvent;
                _server.ItemInPlaceEvent += _server_ItemInPlaceEvent;
                _server.LaserEndEvent += _server_LaserEndEvent;
                _server.RestartEvent += _server_RestartEvent;
                _server.StartMarkingEvent += _server_StartMarkingEvent;
                _server.ErrorEvent += _server_ErrorEvent;

                Initialize();
            }
            catch (Exception ex)
            {
                DCLog.Log.Error(ex, "ADAM Module Error");
                throw;
            }
        }

        private void Initialize()
        {
            bool brc = _server.Initialize();
            if (brc)
            {
                brc = _server.Connect();
            }
            if (!brc)
            {
                DCLog.Log.Error("Can't Start ADAM Communication Service!");
                throw new Exception("Can't Start ADAM Communication Service!");
            }
        }

        public bool StartPoll(int pollinterval, int errortimeout)
        {
            bool brc = _server.StartPoll(pollinterval, errortimeout);

            return brc;
        }

        // TODO: maybe should return bool
        public bool StartPoll()
        {
            return _server.StartPoll();
        }

        private void _server_ErrorEvent(object sender, ErrorArgs e)
        {
            RaiseErrorEvent(e.Text);
        }

        private void _server_StartMarkingEvent(object sender, EventArgs e)
        {
            RaiseStartMarkingEvent();
        }

        internal void Simulate(string v)
        {
            _server.StopPoll();
            System.Threading.Thread.Sleep(500);
            _server.Simulate(v);
            _server.StartPoll(DCConfig.Instance.AdamPollInterval, DCConfig.Instance.AdamErrorTimeout);
        }

        private void _server_RestartEvent(object sender, EventArgs e)
        {
            RaiseRestartEvent();
        }

        private void _server_LaserEndEvent(object sender, EventArgs e)
        {
            RaiseLaserEndEvent();
        }

        private void _server_ItemInPlaceEvent(object sender, EventArgs e)
        {
            RaiseItemInPlaceEvent();
        }

        private void _server_ArticleEvent(object sender, ArticleDataArgs e)
        {
            RaiseArticleEvent(null, e.Data);
        }

        public void Close()
        {
            if (_server != null)
            {
                _server.ArticleEvent -= _server_ArticleEvent;
                _server.ItemInPlaceEvent -= _server_ItemInPlaceEvent;
                _server.LaserEndEvent -= _server_LaserEndEvent;
                _server.RestartEvent -= _server_RestartEvent;
                _server.StartMarkingEvent -= _server_StartMarkingEvent;
                _server.Abort();
            }
        }

        public bool SetKant(byte kantNumber)
        {
            bool result = true;

            result = _server.SendSetKant(kantNumber);

            return result;
        }

        public bool ReadyToMark(bool ready)
        {
            bool result = true;

            result = _server.SendReadyToMark(ready);

            return result;
        }

        public bool BatchNotReady(bool done)
        {
            bool result = true;

            result = _server.SendBatchNotReady(done);

            return result;
        }

        public bool Error(byte errorCode)
        {
            bool result = true;
            result = _server.SendError(errorCode);

            return result;
        }

        public int GetPort(int port)
        {
            throw new NotImplementedException();
        }

        #region Article Event

        public event EventHandler<ArticleArgs> ArticleEvent;

        internal void RaiseArticleEvent(object sender, ArticleData data)
        {
            EventHandler<ArticleArgs> handler = ArticleEvent;
            if (handler != null)
            {
                ArticleArgs args = new ArticleArgs(data);
                handler(sender, args);
            }
        }

        #endregion Article Event

        #region Laser End Event

        public delegate void LaserEndHandler();

        public event EventHandler<EventArgs> LaserEndEvent;

        internal void RaiseLaserEndEvent()
        {
            var handler = LaserEndEvent;
            if (handler != null)
            {
                handler(null, EventArgs.Empty);
            }
        }

        #endregion Laser End Event

        #region Item in place Event

        public delegate void ItemInPlaceHandler();

        public event EventHandler<EventArgs> ItemInPlaceEvent;

        internal void RaiseItemInPlaceEvent()
        {
            var handler = ItemInPlaceEvent;
            if (handler != null)
            {
                handler(null, EventArgs.Empty);
            }
        }

        #endregion Item in place Event

        #region Start Marking Event

        public delegate void StartMarkingHandler();

        public event EventHandler<EventArgs> StartMarkingEvent;

        internal void RaiseStartMarkingEvent()
        {
            var handler = StartMarkingEvent;
            if (handler != null)
            {
                handler(null, EventArgs.Empty);
            }
        }

        #endregion Start Marking Event

        #region Restart application Event

        public delegate void RestartHandler();

        public event EventHandler<EventArgs> RestartEvent;

        internal void RaiseRestartEvent()
        {
            var handler = RestartEvent;
            if (handler != null)
            {
                handler(null, EventArgs.Empty);
            }
        }

        #endregion Restart application Event

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