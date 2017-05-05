using Configuration;
using Contracts;
using DCLog;
using laserengineLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using GlblRes = global::LaserWrapper.Properties.Resources;

namespace LaserWrapper
{
    public class Laser : ILaser, IDigitalIo, Contracts.IAxis
    {
        private static object lockObj = new object();
        private LaserDoc _doc;
        private IoPort _ioPort;
        private LaserAxApp _laser;
        private laserengineLib.System _laserSystem;
        private DCConfig cfg;
        private IoSignals sig;
        private int currentBits;

        #region Configurable variables

        private string _deviceAddress;
        private int _deviceTimeout;
        private int _executeTimeout;
        private string _imagePath;
        private bool _isIoEnabled;
        private string _layoutPath;

        #endregion Configurable variables

        public Laser()
        {
            cfg = DCConfig.Instance;

            _deviceAddress = cfg.DeviceAddress;
            _deviceTimeout = cfg.DeviceTimeout;
            _imagePath = cfg.ImagePath;
            _layoutPath = cfg.LayoutPath;
            _executeTimeout = cfg.ExecuteTimeout;
            _isIoEnabled = cfg.IsIoEnabled;

            sig = new IoSignals();
            UpdateIoMasks();

            InitLaser();
        }

        private void UpdateIoMasks()
        {
            sig.MASK_READYTOMARK = cfg.ReadyToMark;

            sig.MASK_MARKINGDONE = cfg.MarkingDone;
            sig.MASK_ERROR = cfg.Error;

            sig.MASK_ITEMINPLACE = cfg.ItemInPlace;
            sig.MASK_EMERGENCY = cfg.EmergencyError;
            sig.MASK_RESET = cfg.ResetIo;
            sig.MASK_ALL = sig.MASK_ALL | sig.MASK_ERROR | sig.MASK_ITEMINPLACE | sig.MASK_READYTOMARK;
        }

        public int ErrorCode { get; set; }

        public bool Execute()
        {
            bool result = false;
            try
            {
                result = _doc.execute(true, true);
            }
            catch (NullReferenceException ex)
            {
                Log.Error(ex, "Laser: Executing but no document is loaded!");
                result = false;
            }
            catch (COMException ex)
            {
                Log.Error(ex, "Laser: Error executing document");
                result = false;
            }
            return result;
        }

        public List<string> GetDocumentsList()
        {
            List<string> result = new List<string>();
            try
            {
                Array docArr = _laserSystem.getDocumentsList();
                foreach (var doc in docArr)
                {
                    string docName = doc.ToString();
                    if (docName.EndsWith(".xlp"))
                    {
                        result.Add(docName);
                    }
                }

                result.Sort();
            }
            catch (COMException ex)
            {
                Log.Error(ex, "Laser: Error getting documents list!");
            }

            return result;
        }

        public bool Load(string layout)
        {
            _doc = new LaserDoc();
            if (layout.IndexOf(".xlp") == -1)
            {
                layout += ".xlp";
            }
            //layout = Path.Combine(_layoutPath, layout);

            return _doc.load(layout);
        }

        public void Release()
        {
            try
            {
                _laser.release();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Laser: Error releasing Laser");
            }
        }

        public bool Update(List<LaserObjectData> objectList)
        {
            bool result = true;
            try
            {
                string objIDs = _doc.getObjectIDs();
                string[] objArr = objIDs.Split(new char[] { ',' });
                foreach (var id in objArr)
                {
                    var updateObject = objectList.Find(o => o.ID.Equals(id, StringComparison.OrdinalIgnoreCase));
                    if (updateObject != null)
                    {
                        int objType = _doc.getObjectType(id);
                        if (objType == (int)_GraphObjectTypes.CODE_OBJ)
                        {
                            LaserCode code = _doc.getLaserCode(id);
                            code.text = updateObject.Value;
                        }
                        else if (objType == (int)_GraphObjectTypes.STRING_OBJ)
                        {
                            LaserString str = _doc.getLaserString(id);
                            str.text = updateObject.Value;
                        }
                        else if (objType == (int)_GraphObjectTypes.IMPORTED_OBJ)
                        {
                            LaserImported imp = _doc.getLaserImported(id);
                            string imgPath = Path.Combine(_imagePath, updateObject.Value);
                            imp.filename = imgPath;
                        }

                        _doc.updateDocument();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Laser: Error in Update");
                throw;
            }

            return result;
        }

        private void _laserSystem_sigDeviceConnected()
        {
            Log.Trace("Connected to remote device");
            if (_isIoEnabled)
            {
                _ioPort.checkPort(0);
                Log.Trace("checkPort(0)");
                ResetPort(0, sig.MASK_ALL);
                SetReady(true);
                Log.Trace("SetReady(true)");
                RaiseLaserErrorEvent(string.Empty);
            }
        }

        private void _laserSystem_sigDeviceDisconnected()
        {
            Log.Trace("Disconnected from remote device");
        }

        private void _laserSystem_sigDeviceError(string p_message)
        {
            string msg = string.Format("Laser error: {0}", p_message);
            Log.Error(msg);
            RaiseLaserErrorEvent(msg);
        }

        private void _laserSystem_sigLaserEnd()
        {
            Log.Trace("Laser End");
            ErrorCode = 0;
            RaiseLaserEndEvent();
        }

        private void _laserSystem_sigLaserError(int p_errCode)
        {
            ErrorCode = p_errCode;
            Log.Error(string.Format("Laser error: {0}", p_errCode));
        }

        private void InitLaser()
        {
            Log.Trace("InitLaser");
            try
            {
                _laser = new LaserAxApp();
                Log.Debug("LaserAxApp instance created");
                _laserSystem = _laser.System;
                _laserSystem.sigQueryStart += _laserSystem_sigQueryStart;
                _laserSystem.sigLaserEnd += _laserSystem_sigLaserEnd;
                _laserSystem.sigLaserError += _laserSystem_sigLaserError;
                if (_isIoEnabled)
                {
                    _ioPort = _laser.IoPort;
                    _ioPort.sigInputChange += _ioPort_sigInputChange;
                }
                if (_laserSystem.isLaserEngine())
                {
                    _laser.visible = true;
                    if (_isIoEnabled)
                    {
                        _ioPort.checkPort(0);
                        ResetPort(0, sig.MASK_ALL);

                        SetReady(true);
                    }
                }
                else
                {
                    // Remote connection
                    _laserSystem.sigDeviceConnected += _laserSystem_sigDeviceConnected;
                    _laserSystem.sigDeviceError += _laserSystem_sigDeviceError;
                    _laserSystem.sigDeviceDisconnected += _laserSystem_sigDeviceDisconnected;

                    _laserSystem.connectToDevice(_deviceAddress, _deviceTimeout);
                }
            }
            catch (COMException ex)
            {
                Log.Error(ex, "Laser: Error initializing");
                throw;
            }
        }

        private void _laserSystem_sigQueryStart()
        {
            Log.Trace("Start of marking");
            RaiseQueryStartEvent(GlblRes.Marking);
            _ioPort.resetPort(0, sig.MASK_READYTOMARK);
            _doc.execute(true, true);
        }

        public bool ArticleReady()
        {
            return SetPort(0, sig.MASK_ARTICLEREADY);
        }

        #region Digital IO

        public int GetPort(int port)
        {
            return _ioPort.getPort(port);
        }

        public bool ResetPort(int port, int mask)
        {
            Log.Trace(string.Format("ResetPort mask: {0} - {1}", mask, GetMaskName(mask)));
            return _ioPort.resetPort(port, mask);
        }

        public bool SetPort(int port, int mask)
        {
            Log.Trace(string.Format("SetPort mask: {0} - {1}", mask, GetMaskName(mask)));
            return _ioPort.setPort(port, mask);
        }

        private string GetMaskName(int mask)
        {
            string result = "Unknown Mask";
            if ((mask & sig.MASK_ARTICLEREADY) == sig.MASK_ARTICLEREADY)
            {
                result = "Article Ready";
            }
            else if ((mask & sig.MASK_ERROR) == sig.MASK_ERROR)
            {
                result = "Error";
            }
            else if ((mask & sig.MASK_MARKINGDONE) == sig.MASK_MARKINGDONE)
            {
                result = "Marking Done";
            }
            else if ((mask & sig.MASK_READYTOMARK) == sig.MASK_READYTOMARK)
            {
                result = "Ready To Mark";
            }

            return result;
        }

        public bool SetReady(bool OnOff)
        {
            return _ioPort.setReady(OnOff);
        }

        private void _ioPort_sigInputChange(int p_nPort, int p_nBits)
        {
            // Item In Place
            if ((p_nBits & sig.MASK_ITEMINPLACE) == sig.MASK_ITEMINPLACE)
            {
                // bit is set
                if ((currentBits & sig.MASK_ITEMINPLACE) != sig.MASK_ITEMINPLACE)
                {
                    currentBits |= sig.MASK_ITEMINPLACE;
                    RaiseItemInPositionEvent();
                }
            }
            else
            {
                currentBits &= ~sig.MASK_ITEMINPLACE;
            }

            // Reset IO
            if ((p_nBits & sig.MASK_RESET) == sig.MASK_RESET)
            {
                // bit is set
                if ((currentBits & sig.MASK_RESET) != sig.MASK_RESET)
                {
                    currentBits |= sig.MASK_RESET;
                    RaiseResetIoEvent();
                }
            }
            else
            {
                currentBits &= ~sig.MASK_RESET;
            }
        }

        #region LaserEnd Event

        public delegate void LaserEndHandler();

        public event LaserEndHandler LaserEndEvent;

        internal void RaiseLaserEndEvent()
        {
            LaserEndHandler handler = LaserEndEvent;
            if (handler != null)
            {
                handler();
            }
        }

        #endregion LaserEnd Event

        #region LaserError Event

        public delegate void LaserErrorHandler(string msg);

        public event LaserErrorHandler LaserErrorEvent;

        internal void RaiseLaserErrorEvent(string msg)
        {
            LaserErrorHandler handler = LaserErrorEvent;
            if (handler != null)
            {
                handler(msg);
            }
        }

        public class LaserErrorArgs : EventArgs
        {
            public LaserErrorArgs(string s)
            {
                Text = s;
            }

            public string Text { get; private set; } // readonly
        }

        #endregion LaserError Event

        #region Item in Position Event

        public delegate void ItemInPositionHandler();

        public event ItemInPositionHandler ItemInPositionEvent;

        internal void RaiseItemInPositionEvent()
        {
            ItemInPositionHandler handler = ItemInPositionEvent;
            if (handler != null)
            {
                handler();
            }
        }

        #endregion Item in Position Event

        #region LaserQueryStart Event

        public delegate void QueryStartHandler(string msg);

        public event QueryStartHandler QueryStartEvent;

        internal void RaiseQueryStartEvent(string msg)
        {
            QueryStartHandler handler = QueryStartEvent;
            if (handler != null)
            {
                handler(msg);
            }
        }

        public class QueryStartArgs : EventArgs
        {
            public QueryStartArgs(string s)
            {
                Text = s;
            }

            public string Text { get; private set; } // readonly
        }

        #endregion LaserQueryStart Event

        #region Reset IO Signals Event

        public delegate void ResetIoHandler();

        public event ResetIoHandler ResetIoEvent;

        internal void RaiseResetIoEvent()
        {
            ResetIoHandler handler = ResetIoEvent;
            if (handler != null)
            {
                handler();
            }
        }

        #endregion Reset IO Signals Event

        #endregion Digital IO

        #region Axis

        public bool Move(int axis, double position)
        {
            throw new NotImplementedException();
        }

        public bool Move(int axis, double xPosition, double yPosition, double zPosition, double rPosition)
        {
            throw new NotImplementedException();
        }

        #endregion Axis
    }
}