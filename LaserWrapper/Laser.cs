#define FLEXIBLE

using Configuration;
using Contracts;
using DCLog;
using laserengineLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using GlblRes = global::LaserWrapper.Properties.Resources;

namespace LaserWrapper
{
    public partial class Laser : ILaser, IDigitalIo, Contracts.IAxis
    {
        private static object lockObj = new object();
        private LaserDoc _doc;
        private IoPort _ioPort;
        private LaserAxApp _laser;
        private laserengineLib.System _laserSystem;
        private Axis _laserAxis;
        private DCConfig cfg;
        private IoSignals sig;
        private int currentBits=0;
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
            sig = IoSignals.Instance;
            UpdateIoMasks();
            IoFix.Init();
            NextToLast = false;

            InitLaser();
        }

        private static void InitLanguage()
        {
            string language = DCConfig.Instance.GuiLanguage;
            Log.Debug(string.Format("GUI Language: {0}", language));
            if (!string.IsNullOrWhiteSpace(language))
            {
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo(language);
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(language);
            }
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
#if false
                InitLanguage();
                RaiseQueryStartEvent(GlblRes.Marking);
#endif
                if("sv-SE"== DCConfig.Instance.GuiLanguage)
                {
                    RaiseQueryStartEvent("Märker ...");
                }
                else
                {
                    RaiseQueryStartEvent("Marking...");

                }

                result = _doc.execute(true, true);

                if (!result)
                {
                    Log.Trace("Laser: Execute failed!");
                    RaiseDeviceErrorEvent("Laser: Execute failed!");
                }
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
            Log.Trace("Laser: Execute Returns");
            return result;
        }

        public bool StopMarking()
        {
            bool result = true;

            Log.Debug("Laser: StopMarking");
            if (_laserSystem != null)
            {
                result = _laserSystem.stopLaser();
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

#if false
        public bool UpdateToNumber(string tonumber)
        {
            bool result = false;

            LaserObjectData dta = new LaserObjectData();
            dta.ID = "T1";
            dta.Value = tonumber;
            result = Update(new List<LaserObjectData>() { dta });

            return result;
        }
#endif

        public bool Update(List<LaserObjectData> objectList)
        {
            bool result = true;
            LogObjectList(objectList);
            string objIDs = GetObjectIDs();
            Log.Trace(string.Format("objIDs: {0}", objIDs));
            string[] objArr = objIDs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var id in objArr)
            {
                Log.Trace(string.Format("ID: {0}", id));
                var updateObject = objectList.Find(o => o.ID.Equals(id, StringComparison.OrdinalIgnoreCase));
                if (updateObject == null)
                {
                    Log.Trace("updateObject is null");
                    // if the id is not found in the database objects list, remove 1 character and try again
                    // Stupid idea of Ola ;-)
                    var tmp = id.Substring(0, id.Length - 1);
                    updateObject = objectList.Find(o => o.ID.Equals(tmp, StringComparison.OrdinalIgnoreCase));
                }
                if (updateObject != null)
                {
                    Log.Trace(string.Format("Object: {0} - Value: {1}", updateObject.ID, updateObject.Value));

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
                    Log.Trace(string.Format("Laser: Updated ID:{0}", id));
                    _doc.updateDocument();
                }
                else
                {
                    Log.Info(string.Format("Laser: Layout: {0} - Can't find a field for ID: {1}", _layoutName, id));
                }
            }

            Log.Trace(string.Format("Laser: Update {0} returns {1}", _layoutName, result));
            return result;
        }

        private void LogObjectList(List<LaserObjectData> objectList)
        {
            string msg = string.Empty;

            Log.Trace("objectList:");
            foreach (LaserObjectData o in objectList)
            {
                Log.Trace(string.Format("\tID: {0} - Value: {1}", o.ID, o.Value));
            }
        }

        private string GetObjectIDs()
        {
            string result = string.Empty;

            try
            {
                result = _doc.getObjectIDs();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Can't get ObjectIDs");
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
            string msg = string.Format("Laser Device Error: {0}", p_message);
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
            Log.Error(string.Format("Laser Error: {0}", p_errCode));
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

#if !FLEXIBLE
                // sigQueryStart will only trigger when an External Start signal is recieved. Thus we must use sigLaserStart when debugging...
                _laserSystem.sigQueryStart += _laserSystem_sigQueryStart;
#endif

#if DEBUG
                _laserSystem.sigLaserStart += _laserSystem_sigLaserStart;
#endif
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
                    Log.Trace(string.Format("Remote Connection: {0}", _deviceAddress));
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

        public void SaveDoc()
        {
            if (_doc != null)
            {
                string fname = _doc.filename;
                if (!string.IsNullOrWhiteSpace(fname))
                {
                    fname = Path.Combine(@"C:\temp", Path.GetFileName(fname));
                    _doc.saveAs(fname);
                }
            }
        }

#if !FLEXIBLE
        /// <summary>
        /// Event for External Start Signal!
        /// </summary>
        private void _laserSystem_sigQueryStart()
        {
            Log.Trace("Query Start");
            InitLanguage();
            RaiseQueryStartEvent(GlblRes.Marking);
            ResetPort(0, sig.MASK_READYTOMARK);
            RaiseLaserBusyEvent(true);
            _doc.execute(true, true);
            if (NextToLast)
            {
                SetPort(0, sig.MASK_NEXTTOLAST);
            }
        }
#endif

#if DEBUG || FLEXIBLE

        /// <summary>
        /// Event for Start marking
        /// NB! This is called everytime _doc.execute() is executed!
        /// Which is why it can't exist in the Release version!!!
        /// NB! Flexible doesn't use External Start, it uses a IO to signal start so we use this function 
        ///     to start the marking even in Release mode. We then need to call _doc.execute
        /// </summary>
        public void _laserSystem_sigLaserStart()
        {
            Log.Trace("Start of marking");
#if false
                InitLanguage();
                RaiseQueryStartEvent(GlblRes.Marking);
#endif
            if ("sv-SE" == DCConfig.Instance.GuiLanguage)
            {
                RaiseQueryStartEvent("Märker ...");
            }
            else
            {
                RaiseQueryStartEvent("Marking...");

            }
            ResetPort(0, sig.MASK_READYTOMARK);
            RaiseLaserBusyEvent(true);
        }

#endif

#region Digital IO

        public int GetPort(int port)
        {
            return _ioPort.getPort(port);
        }

        public bool ResetPort(int port, int mask)
        {
            if (_ioPort != null)
            {
                Log.Debug(string.Format("Reset IO Mask: {0} - {1}", GetMaskName(mask), mask));
                IoFix.Delete(mask);
                return _ioPort.resetPort(port, mask);
            }
            else
            {
                Log.Debug(string.Format("IO port is null! Mask: {0} - {1}", GetMaskName(mask), mask));
            }

            return false;
        }

        public bool SetPort(int port, int mask)
        {
            if (_ioPort != null)
            {
                Log.Debug(string.Format("Set IO Mask: {0} - {1}", GetMaskName(mask), mask));
                int currentMask = IoFix.Add(mask);
                Log.Trace(string.Format("Mask: {0} - CurrentMask: {1}", mask, currentMask));
                return _ioPort.setPort(port, currentMask);
            }
            else
            {
                Log.Debug(string.Format("IO port is null! Mask: {0} - {1}", GetMaskName(mask), mask));
            }

            return false;
        }

        private string GetMaskName(int mask)
        {
            string result = string.Empty;

            bool brc = sig.NameDict.TryGetValue(mask, out result);
            if (!brc)
                Log.Debug(string.Format("Mask Name Not Found: {0}", mask));

            return result;
            throw new NotImplementedException();
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