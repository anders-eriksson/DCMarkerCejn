using Configuration;
using Contracts;
using DCLog;
using laserengineLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace LaserWrapper
{
    public class Laser : ILaser, IDigitalIo, Contracts.IAxis
    {
        private static object lockObj = new object();
        private LaserDoc _doc;
        private IoPort _ioPort;
        private LaserAxApp _laser;
        private laserengineLib.System _laserSystem;

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
            DCConfig cfg = DCConfig.Instance;

            _deviceAddress = cfg.DeviceAddress;
            _deviceTimeout = cfg.DeviceTimeout;
            _imagePath = cfg.ImagePath;
            _layoutPath = cfg.LayoutPath;
            _executeTimeout = cfg.ExecuteTimeout;
            _isIoEnabled = cfg.IsIoEnabled;

            InitLaser();
        }

        public int ErrorCode { get; set; }

        public bool Execute()
        {
            bool result;
            result = _doc.execute(true, true);
            if (result)
            {
                try
                {
                    lock (lockObj)
                    {
                        Monitor.Wait(lockObj, _executeTimeout);
                        if (ErrorCode > 0)
                        {
                            result = false;
                        }
                    }
                }
                catch (ThreadInterruptedException ex)
                {
                    Log.Error(ex, "Laser: Execute was interrupted"); ;
                    result = false;
                }
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

        public bool Update(List<LaserObjects> objectList)
        {
            bool result = true;
            string objIDs = _doc.getObjectIDs();
            string[] objArr = objIDs.Split(new char[] { ',' });
            foreach (var id in objArr)
            {
                var updateObject = objectList.Find(o => o.ID == id);
                if (updateObject != null)
                {
                    LaserObject obj = _doc.getLaserObject(id);
                    if (obj.getType() == (int)_GraphObjectTypes.CODE_OBJ)
                    {
                        LaserCode code = _doc.getLaserCode(id);
                        code.text = updateObject.Value;
                    }
                    else if (obj.getType() == (int)_GraphObjectTypes.STRING_OBJ)
                    {
                        LaserString str = _doc.getLaserString(id);
                        str.text = updateObject.Value;
                    }
                    else if (obj.getType() == (int)_GraphObjectTypes.IMPORTED_OBJ)
                    {
                        LaserImported imp = _doc.getLaserImported(id);
                        string imgPath = Path.Combine(_imagePath, updateObject.Value);
                        imp.filename = imgPath;
                    }

                    _doc.updateDocument();
                }
            }

            return result;
        }

        private void _ioPort_sigInputChange(int p_nPort, int p_nBits)
        {
            throw new NotImplementedException();
        }

        private void _laserSystem_sigDeviceConnected()
        {
            if (_isIoEnabled)
            {
                _ioPort.checkPort(0);
            }

            lock (lockObj)
            {
                Monitor.Pulse(lockObj);
            }
        }

        private void _laserSystem_sigDeviceDisconnected()
        {
            Log.Trace("sigDeviceDisconnected");
        }

        private void _laserSystem_sigDeviceError(string p_message)
        {
            lock (lockObj)
            {
                Monitor.Pulse(lockObj);
            }
        }

        private void _laserSystem_sigLaserEnd()
        {
            ErrorCode = 0;
            lock (lockObj)
            {
                Monitor.Pulse(lockObj);
            }
        }

        private void _laserSystem_sigLaserError(int p_errCode)
        {
            ErrorCode = p_errCode;
            lock (lockObj)
            {
                Monitor.Pulse(lockObj);
            }
        }

        private void InitLaser()
        {
            try
            {
                _laser = new LaserAxApp();
                _laserSystem = _laser.System;
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
                    }
                }
                else
                {
                    // Remote connection
                    _laserSystem.sigDeviceConnected += _laserSystem_sigDeviceConnected;
                    _laserSystem.sigDeviceError += _laserSystem_sigDeviceError;
                    _laserSystem.sigDeviceDisconnected += _laserSystem_sigDeviceDisconnected;

                    _laserSystem.connectToDevice(_deviceAddress, _deviceTimeout);
                    lock (lockObj)
                    {
                        Monitor.Wait(lockObj);
                    }
                }
            }
            catch (COMException ex)
            {
                Log.Error(ex, "Laser: Error initializing");
                throw;
            }
        }

        #region Digital IO

        public int GetPort(int port)
        {
            throw new NotImplementedException();
        }

        public bool ResetPort(int port, int mask)
        {
            return _ioPort.resetPort(port, mask);
        }

        public bool SetPort(int port, int mask)
        {
            return _ioPort.setPort(port, mask);
        }

        public bool SetReady(bool OnOff)
        {
            throw new NotImplementedException();
        }

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