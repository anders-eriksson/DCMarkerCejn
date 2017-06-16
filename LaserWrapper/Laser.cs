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
        private Axis _laserAxis;
        private DCConfig cfg;
        private IoSignals sig;
        private int currentBits;
        private string _layoutName;

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

            _layoutName = string.Empty;
            sig = new IoSignals();
            UpdateIoMasks();
            IoFix.Init();
            NextToLast = false;

            InitLaser();
        }

        private void UpdateIoMasks()
        {
            sig.MASK_ARTICLEREADY = cfg.ArticleReady;
            sig.MASK_READYTOMARK = cfg.ReadyToMark;
            sig.MASK_NEXTTOLAST = cfg.NextToLast;
            sig.MASK_MARKINGDONE = cfg.MarkingDone;
            sig.MASK_ERROR = cfg.Error;
            sig.MASK_ALL = sig.MASK_ARTICLEREADY | sig.MASK_READYTOMARK | sig.MASK_NEXTTOLAST | sig.MASK_MARKINGDONE | sig.MASK_ERROR;

            // In
            sig.MASK_ITEMINPLACE = cfg.ItemInPlace;
            sig.MASK_EMERGENCY = cfg.EmergencyError;
            sig.MASK_RESET = cfg.ResetIo;
        }

        public void ResetDocument()
        {
            _doc = null;
        }

        public int ErrorCode { get; set; }
        public bool NextToLast { get; set; }

        public bool Execute()
        {
            Log.Debug("Laser: Execute");
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
            Log.Debug(string.Format("Loading: {0}", layout));
            bool brc = _doc.load(layout);
            if (brc)
            {
                _layoutName = layout;
            }
            return brc;
        }

        public void Release()
        {
            try
            {
                Log.Trace("Laser: Releasing Laser");
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
            string objIDs = _doc.getObjectIDs();
            string[] objArr = objIDs.Split(new char[] { ',' });
            foreach (var id in objArr)
            {
                var updateObject = objectList.Find(o => o.ID.Equals(id, StringComparison.OrdinalIgnoreCase));
                if (updateObject == null)
                {
                    // if the id is not found in the database objects list, remove 1 character and try again
                    // Stupid idea of Ola ;-)
                    var tmp = id.Substring(0, id.Length - 1);
                    updateObject = objectList.Find(o => o.ID.Equals(tmp, StringComparison.OrdinalIgnoreCase));
                }
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
                else
                {
                    Log.Error(string.Format("Laser: Layout: {0} - Can't find a field for ID: {1}", _layoutName, id));
                }
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
                RaiseDeviceErrorEvent(string.Empty);
            }

            //lock (lockObj)
            //{
            //    Monitor.Pulse(lockObj);
            //}
        }

        private void _laserSystem_sigDeviceDisconnected()
        {
            Log.Trace("Disconnected from remote device");
        }

        private void _laserSystem_sigDeviceError(string p_message)
        {
            string msg = string.Format("Laser error: {0}", p_message);
            Log.Error(msg);
            RaiseDeviceErrorEvent(msg);
        }

        private void _laserSystem_sigLaserEnd()
        {
            Log.Trace("Laser End");
            ErrorCode = 0;
            RaiseLaserEndEvent();
            RaiseLaserBusyEvent(false);
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
                _laserAxis = _laser.Axis;
                _laserAxis.sigZeroReached += _laserAxis_sigZeroReached;
                _laserAxis.sigAxisError += _laserAxis_sigAxisError;

                _laserSystem = _laser.System;
                _laserSystem.sigQueryStart += _laserSystem_sigQueryStart;
                //_laserSystem.sigLaserStart += _laserSystem_sigLaserStart;
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

        private void _laserAxis_sigAxisError(uint p_nAxis, uint p_nError)
        {
            Log.Debug(string.Format("Error: {0} has occurred on axis: {1}", p_nError, p_nAxis));
            RaiseDeviceErrorEvent(string.Format("Device Error: {0} has occurred on axis: {1}", p_nError, p_nAxis));
        }

        private void _laserAxis_sigZeroReached(uint p_nAxis)
        {
            string[] axisName = new string[] { "X", "Y", "Z", "R" };
            Log.Debug(string.Format("{0} Axis has reached Zero", p_nAxis));
            RaiseZeroReachedEvent(string.Format("{0}-Axis has been reset", axisName[p_nAxis]));
        }

        /// <summary>
        /// Event for External Start Signal!
        /// </summary>
        private void _laserSystem_sigQueryStart()
        {
            Log.Trace("Query Start");
            RaiseQueryStartEvent(GlblRes.Marking);
            ResetPort(0, sig.MASK_READYTOMARK);
            RaiseLaserBusyEvent(true);
            _doc.execute(true, true);
            if (NextToLast)
            {
                SetPort(0, sig.MASK_NEXTTOLAST);
            }
        }

        /// <summary>
        /// Event for Start marking
        /// </summary>
        private void _laserSystem_sigLaserStart()
        {
            Log.Trace("Start of marking");
            RaiseQueryStartEvent(GlblRes.Marking);
            ResetPort(0, sig.MASK_READYTOMARK);
            RaiseLaserBusyEvent(true);
            _doc.execute(true, true);
        }

        #region Digital IO

        public int GetPort(int port)
        {
            return _ioPort.getPort(port);
        }

        public bool ResetPort(int port, int mask)
        {
            if (_ioPort != null)
            {
                Log.Debug(string.Format("Reset IO Mask: {0}", mask));
                IoFix.Delete(mask);
                return _ioPort.resetPort(port, mask);
            }
            else
            {
                Log.Debug(string.Format("IO port is null! Mask: {0}", mask));
            }

            return false;
        }

        public bool SetPort(int port, int mask)
        {
            if (_ioPort != null)
            {
                Log.Debug(string.Format("Set IO Mask: {0}", mask));
                int currentMask = IoFix.Add(mask);
                Log.Trace(string.Format("Mask: {0} - CurrentMask: {1}", mask, currentMask));
                return _ioPort.setPort(port, currentMask);
            }
            else
            {
                Log.Debug(string.Format("IO port is null! Mask: {0}", mask));
            }

            return false;
        }

        public bool SetReady(bool OnOff)
        {
            if (_ioPort != null)
            {
                Log.Debug(string.Format("SetReady {0}", OnOff));
                return _ioPort.setReady(OnOff);
            }
            else
            {
                Log.Debug(string.Format("IO port is null! SetReady: {0}", OnOff));
            }
            return false;
        }

        private void _ioPort_sigInputChange(int p_nPort, int p_nBits)
        {
            Log.Debug(string.Format("Port: {0} - Bit: {1}", p_nPort, p_nBits));
            // Item In Place
            if ((p_nBits & sig.MASK_ITEMINPLACE) == sig.MASK_ITEMINPLACE)
            {
                Log.Debug("MASK_ITEMINPLACE");
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
                Log.Debug("MASK_RESET");
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

        #endregion Digital IO

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

        public event LaserErrorHandler DeviceErrorEvent;

        internal void RaiseDeviceErrorEvent(string msg)
        {
            LaserErrorHandler handler = DeviceErrorEvent;
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

        #region Laser Busy Event

        public delegate void LaserBusyHandler(bool busy);

        public event LaserBusyHandler LaserBusyEvent;

        internal void RaiseLaserBusyEvent(bool busy)
        {
            LaserBusyHandler handler = LaserBusyEvent;
            if (handler != null)
            {
                handler(busy);
            }
        }

        public class LaserBusyEventArgs : EventArgs
        {
            public LaserBusyEventArgs(bool busy)
            {
                Busy = busy;
            }

            public bool Busy { get; private set; } // readonly
        }

        #endregion Laser Busy Event

        #region Axis

        private const int X_AXIS = 0;
        private const int Y_AXIS = 1;
        private const int Z_AXIS = 2;
        private const int R_AXIS = 3;

        public bool Move(int axis, double position)
        {
            throw new NotImplementedException();
        }

        public bool Move(int axis, double xPosition, double yPosition, double zPosition, double rPosition)
        {
            throw new NotImplementedException();
        }

        public bool ResetZAxis()
        {
            bool result = true;
            result = _laserAxis.reset(Z_AXIS);          // if true then we will get an event sigZeroReached when the axis has been reset!
            return result;
        }

        #region Zero Reached Event

        public delegate void ZeroReachedHandler(string msg);

        public event ZeroReachedHandler ZeroReachedEvent;

        internal void RaiseZeroReachedEvent(string msg)
        {
            ZeroReachedHandler handler = ZeroReachedEvent;
            if (handler != null)
            {
                handler(msg);
            }
        }

        public class ZeroReachedArgs : EventArgs
        {
            public ZeroReachedArgs(string s)
            {
                Text = s;
            }

            public string Text { get; private set; } // readonly
        }

        #endregion Zero Reached Event

        #endregion Axis
    }
}