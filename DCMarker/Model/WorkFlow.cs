using Configuration;
using Contracts;
using DCTcpServer;
using LaserWrapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DCMarker.Model
{
    public class WorkFlow
    {
        private DCConfig cfg;
        private DB _db;
        private Server _server;
        private Laser _laser;
        private IArticleInput _articleInput;
        private IoSignals sig;

        private volatile bool IsInitialized = false;

        public WorkFlow()
        {
            cfg = DCConfig.Instance;
            cfg.WriteConfig();
            sig = new IoSignals();
            UpdateIoMasks();
            Initialize();
        }

        private void UpdateIoMasks()
        {
            sig.MASK_READYTOMARK = cfg.ReadyToMark;

            sig.MASK_MARKINGDONE = cfg.MarkingDone;
            sig.MASK_ERROR = cfg.Error;

            sig.MASK_ITEMINPLACE = cfg.ItemInPlace;
            sig.MASK_EMERGENCY = cfg.EmergencyError;
        }

        public bool Initialize()
        {
            bool result = true;
            IsInitialized = false;
            try
            {
                InitializeMachine();
                //InitializeTcpServer();
                InitializeDatabase();
                InitializeLaser();
                IsInitialized = true;
            }
            catch (Exception)
            {
                IsInitialized = false;
                throw;
            }

            return result;
        }

        public void Abort()
        {
            _laser.Release();
            _articleInput.Abort();
        }

        private void InitializeMachine()
        {
            switch (cfg.MarkingDone)
            {
                case 1:
                    _articleInput = new TcpArticleInput();
                    _articleInput.ArticleEvent += _articleInput_ArticleEvent;
                    break;

                case 2:
                    _articleInput = new ManualArticleInput();
                    _articleInput.ArticleEvent += _articleInput_ArticleEvent;
                    break;

                default:
                    throw new Exception(string.Format("Type of machine not available! Type={0}", cfg.MarkingDone));
                    break;
            }
        }

        private void _articleInput_ArticleEvent(object sender, ArticleArgs e)
        {
            RaiseErrorEvent(string.Empty);
            _laser.ResetPort(0, sig.MASK_READYTOMARK);
            List<LaserObjectData> laserObjects = _db.GetLaserDataAsObjects(e.Data.ArticleNumber);
            if (laserObjects != null)
            {
                UpdateViewModel(laserObjects);

                var brc = GetUserInput(ref laserObjects);
                if (brc)
                {
                    string layoutname = GetLayoutname(laserObjects);
                    if (!string.IsNullOrEmpty(layoutname))
                    {
                        layoutname = NormalizeLayoutName(layoutname);
                        brc = _laser.Load(layoutname);
                        if (brc)
                        {
                            brc = _laser.Update(laserObjects);
                            if (brc)
                            {
                                // we are ready to mark...
                                RaiseStatusEvent(string.Format("Waiting for start signal ({0})", layoutname));
                                _laser.ResetPort(0, sig.MASK_MARKINGDONE);
                                _laser.SetPort(0, sig.MASK_READYTOMARK);
                            }
                            else
                            {
                                RaiseErrorEvent(string.Format("Update didn't work on this article and layout! Article={0}, Layout={1}", e.Data.ArticleNumber, layoutname));
                                _laser.SetPort(0, sig.MASK_ERROR);
                            }
                        }
                        else
                        {
                            RaiseErrorEvent(string.Format("Error loading layout: {0}", layoutname));
                            _laser.SetPort(0, sig.MASK_ERROR);
                        }
                    }
                    else
                    {
                        RaiseErrorEvent(string.Format("Layout not defined for this article! Article={0}", e.Data.ArticleNumber));
                        _laser.SetPort(0, sig.MASK_ERROR);
                    }
                }
                else
                {
                    // User canceled
                    // TODO What should we do??
                }
            }
            else
            {
                // Can't find article in database.
                RaiseErrorEvent(string.Format("Article not defined in database! Article={0}", e.Data.ArticleNumber));
            }
        }

        private void UpdateViewModel(List<LaserObjectData> laserObjects)
        {
            UpdateViewModelData data = new UpdateViewModelData();
            data.ArticleNumber = GetObjectValue(laserObjects, "F1");
            data.Kant = GetObjectValue(laserObjects, "Kant");
            if (string.IsNullOrWhiteSpace(data.Kant))
            {
                data.HasKant = false;
            }
            else
            {
                data.HasKant = true;
            }

            RaiseUpdateMainViewModelEvent(data);
        }

        private string NormalizeLayoutName(string layoutname)
        {
            string result = string.Empty;

            if (layoutname.IndexOf(".xlp") < 0)
            {
                layoutname += ".xlp";
            }
            if (cfg.LayoutPath.Length > 1)
            {
                result = Path.Combine(cfg.LayoutPath, layoutname);
            }
            else
            {
                result = layoutname;
            }

            return result;
        }

        private bool GetUserInput(ref List<LaserObjectData> laserObjects)
        {
            bool result = true;

            // TODO how will we wait for TOnr to be specified!
            //      easiest is to use a Modal Dialog ...

            //// TOnr
            //string tmp = GetObjectValue(laserObjects, "enableTO");
            //bool enableTO = Convert.ToBoolean(tmp);
            //RaiseUpdateEnableTO(enableTO);

            return result;
        }

        private static string GetObjectValue(List<LaserObjectData> laserObjects, string key)
        {
            string result = null;

            try
            {
                var obj = laserObjects.SingleOrDefault(s => s.ID == key);
                if (obj != null)
                {
                    result = obj.Value;
                }
            }
            catch (Exception)
            {
                // TODO Add log!
                result = null;
            }

            return result;
        }

        private static string GetLayoutname(List<LaserObjectData> laserObjects)
        {
            return GetObjectValue(laserObjects, "Template");
        }

        private void InitializeDatabase()
        {
            _db = new DB();
        }

        private void InitializeLaser()
        {
            _laser = new Laser();
            _laser.LaserEndEvent += _laser_LaserEndEvent;
            _laser.LaserErrorEvent += _laser_LaserErrorEvent;
            _laser.ItemInPositionEvent += _laser_ItemInPositionEvent;
        }

        private void _laser_ItemInPositionEvent()
        {
            throw new NotImplementedException();
        }

        private void _laser_LaserErrorEvent(string msg)
        {
            _laser.SetPort(0, sig.MASK_ERROR);
        }

        private void _laser_LaserEndEvent()
        {
            _laser.SetPort(0, sig.MASK_MARKINGDONE);
        }

        private void InitializeTcpServer()
        {
            try
            {
                _server = new Server(cfg.TcpPort, cfg.BufferLength);
                _server.NewArticleNumberEvent += _server_NewArticleNumberEvent;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void _server_NewArticleNumberEvent(string msg)
        {
            // Get article data from LaserData table
        }

        #region Error Event

        public delegate void ErrorHandler(string msg);

        public event ErrorHandler ErrorEvent;

        internal void RaiseErrorEvent(string msg)
        {
            ErrorHandler handler = ErrorEvent;
            if (handler != null)
            {
                handler(msg);
            }
        }

        public class ErrorArgs : EventArgs
        {
            public ErrorArgs(string s)
            {
                Text = s;
            }

            public string Text { get; private set; }
        }

        #endregion Error Event

        #region Update MainViewModel Event

        public delegate void UpdateMainViewModelHandler(UpdateViewModelData data);

        public event UpdateMainViewModelHandler UpdateMainViewModelEvent;

        internal void RaiseUpdateMainViewModelEvent(UpdateViewModelData data)
        {
            UpdateMainViewModelHandler handler = UpdateMainViewModelEvent;
            if (handler != null)
            {
                handler(data);
            }
        }

        public class UpdateMainViewModelArgs : EventArgs
        {
            public UpdateMainViewModelArgs(UpdateViewModelData data)
            {
                Data = data;
            }

            public UpdateViewModelData Data { get; private set; } // readonly
        }

        #endregion Update MainViewModel Event

        #region Status Event

        public delegate void StatusHandler(string msg);

        public event StatusHandler StatusEvent;

        internal void RaiseStatusEvent(string msg)
        {
            StatusHandler handler = StatusEvent;
            if (handler != null)
            {
                handler(msg);
            }
        }

        public class StatusArgs : EventArgs
        {
            public StatusArgs(string s)
            {
                Text = s;
            }

            public string Text { get; private set; } // readonly
        }

        #endregion Status Event
    }
}