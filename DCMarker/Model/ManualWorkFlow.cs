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
    public class ManualWorkFlow : IWorkFlow
    {
        //    private IArticleInput _articleInput;
        //    private DBx _db;
        //    private Laser _laser;
        //    private List<LaserObjectData> _laserObjects;
        //    private Server _server;
        //    private DCConfig cfg;
        //    private volatile bool IsInitialized = false;
        //    private IoSignals sig;
        //    private string _articleNumber;

        //    public ManualWorkFlow()
        //    {
        //        cfg = DCConfig.Instance;
        //        cfg.WriteConfig();
        //        sig = new IoSignals();
        //        UpdateIoMasks();
        //        Initialize();
        //    }

        //    public void Close()
        //    {
        //        _laser.Release();
        //        _articleInput.Close();
        //    }

        //    public bool Initialize()
        //    {
        //        bool result = true;
        //        IsInitialized = false;
        //        try
        //        {
        //            InitializeMachine();
        //            //InitializeTcpServer();
        //            InitializeDatabase();
        //            InitializeLaser();
        //            IsInitialized = true;
        //        }
        //        catch (Exception)
        //        {
        //            IsInitialized = false;
        //            throw;
        //        }

        //        return result;
        //    }

        //    private static string GetLayoutname(List<LaserObjectData> laserObjects)
        //    {
        //        return GetObjectValue(laserObjects, "Template");
        //    }

        //    private static string GetObjectValue(List<LaserObjectData> laserObjects, string key)
        //    {
        //        string result = null;

        //        try
        //        {
        //            var obj = laserObjects.SingleOrDefault(s => s.ID == key);
        //            if (obj != null)
        //            {
        //                result = obj.Value;
        //            }
        //        }
        //        catch (Exception)
        //        {
        //            // TODO Add log!
        //            result = null;
        //        }

        //        return result;
        //    }

        //    private void _articleInput_ArticleEvent(object sender, ArticleArgs e)
        //    {
        //        RaiseErrorEvent(string.Empty);
        //        _laser.ResetPort(0, sig.MASK_READYTOMARK);
        //        _articleNumber = e.Data.ArticleNumber;
        //        _laserObjects = _db.GetLaserDataAsObjects(_articleNumber);
        //        if (_laserObjects != null)
        //        {
        //            UpdateViewModel(_laserObjects);
        //        }
        //        else
        //        {
        //            // Can't find article in database.
        //            RaiseErrorEvent(string.Format("Article not defined in database! Article={0}", _articleNumber));
        //        }
        //    }

        //    private void _laser_ItemInPositionEvent()
        //    {
        //        UpdateLayout();
        //    }

        //    private void _laser_LaserEndEvent()
        //    {
        //        _laser.SetPort(0, sig.MASK_MARKINGDONE);
        //    }

        //    private void _laser_LaserErrorEvent(string msg)
        //    {
        //        _laser.SetPort(0, sig.MASK_ERROR);
        //    }

        //    private void _server_NewArticleNumberEvent(string msg)
        //    {
        //        // Get article data from LaserData table
        //    }

        //    private bool GetUserInput(ref List<LaserObjectData> laserObjects)
        //    {
        //        bool result = true;

        //        // TODO how will we wait for TOnr to be specified!
        //        //      easiest is to use a Modal Dialog ...

        //        //// TOnr
        //        //string tmp = GetObjectValue(laserObjects, "enableTO");
        //        //bool enableTO = Convert.ToBoolean(tmp);
        //        //RaiseUpdateEnableTO(enableTO);

        //        return result;
        //    }

        //    private void InitializeDatabase()
        //    {
        //        _db = new DBx();
        //    }

        //    private void InitializeLaser()
        //    {
        //        _laser = new Laser();
        //        _laser.LaserEndEvent += _laser_LaserEndEvent;
        //        _laser.LaserErrorEvent += _laser_LaserErrorEvent;
        //        _laser.ItemInPositionEvent += _laser_ItemInPositionEvent;
        //    }

        //    private void InitializeMachine()
        //    {
        //        _articleInput = new ManualArticleInput();
        //        _articleInput.ArticleEvent += _articleInput_ArticleEvent;
        //    }

        //    //private void InitializeTcpServer()
        //    //{
        //    //    try
        //    //    {
        //    //        _server = new Server(cfg.TcpPort, cfg.BufferLength);
        //    //        _server.NewArticleNumberEvent += _server_NewArticleNumberEvent;
        //    //    }
        //    //    catch (Exception ex)
        //    //    {
        //    //        throw;
        //    //    }
        //    //}

        //    private string NormalizeLayoutName(string layoutname)
        //    {
        //        string result = string.Empty;

        //        if (layoutname.IndexOf(".xlp") < 0)
        //        {
        //            layoutname += ".xlp";
        //        }
        //        if (cfg.LayoutPath.Length > 1)
        //        {
        //            result = Path.Combine(cfg.LayoutPath, layoutname);
        //        }
        //        else
        //        {
        //            result = layoutname;
        //        }

        //        return result;
        //    }

        //    private void UpdateIoMasks()
        //    {
        //        sig.MASK_READYTOMARK = cfg.ReadyToMark;

        //        sig.MASK_MARKINGDONE = cfg.MarkingDone;
        //        sig.MASK_ERROR = cfg.Error;

        //        sig.MASK_ITEMINPLACE = cfg.ItemInPlace;
        //        sig.MASK_EMERGENCY = cfg.EmergencyError;
        //    }

        //    private void UpdateLayout()
        //    {
        //        string layoutname = GetLayoutname(_laserObjects);
        //        if (!string.IsNullOrEmpty(layoutname))
        //        {
        //            layoutname = NormalizeLayoutName(layoutname);
        //            bool brc = _laser.Load(layoutname);
        //            if (brc)
        //            {
        //                brc = _laser.Update(_laserObjects);
        //                if (brc)
        //                {
        //                    // we are ready to mark...
        //                    RaiseStatusEvent(string.Format("Waiting for start signal ({0})", layoutname));
        //                    _laser.ResetPort(0, sig.MASK_MARKINGDONE);
        //                    _laser.SetPort(0, sig.MASK_READYTOMARK);
        //                }
        //                else
        //                {
        //                    RaiseErrorEvent(string.Format("Update didn't work on this article and layout! Article={0}, Layout={1}", _articleNumber, layoutname));
        //                    _laser.SetPort(0, sig.MASK_ERROR);
        //                }
        //            }
        //            else
        //            {
        //                RaiseErrorEvent(string.Format("Error loading layout: {0}", layoutname));
        //                _laser.SetPort(0, sig.MASK_ERROR);
        //            }
        //        }
        //        else
        //        {
        //            RaiseErrorEvent(string.Format("Layout not defined for this article! Article={0}", _articleNumber));
        //            _laser.SetPort(0, sig.MASK_ERROR);
        //        }
        //    }

        //    private void UpdateViewModel(List<LaserObjectData> laserObjects)
        //    {
        //        var data = new UpdateViewModelData
        //        {
        //            ArticleNumber = GetObjectValue(laserObjects, "F1"),
        //            Kant = GetObjectValue(laserObjects, "Kant")
        //        };
        //        if (string.IsNullOrWhiteSpace(data.Kant))
        //        {
        //            data.HasKant = false;
        //        }
        //        else
        //        {
        //            data.HasKant = true;
        //        }
        //        var tmp = GetObjectValue(laserObjects, "TOnr");
        //        data.HasTOnr = Convert.ToBoolean(tmp);

        //        RaiseUpdateMainViewModelEvent(data);
        //    }

        //    #region Error Event

        //    public delegate void ErrorHandler(string msg);

        //    public event EventHandler<ErrorArgs> ErrorEvent;

        //    internal void RaiseErrorEvent(string msg)
        //    {
        //        var handler = ErrorEvent;
        //        if (handler != null)
        //        {
        //            var arg = new ErrorArgs(msg);
        //            handler(null, arg);
        //        }
        //    }

        //    #endregion Error Event

        //    #region Update MainViewModel Event

        //    public delegate void UpdateMainViewModelHandler(UpdateViewModelData data);

        //    public event EventHandler<UpdateMainViewModelArgs> UpdateMainViewModelEvent;

        //    internal void RaiseUpdateMainViewModelEvent(UpdateViewModelData data)
        //    {
        //        var handler = UpdateMainViewModelEvent;
        //        if (handler != null)
        //        {
        //            var arg = new UpdateMainViewModelArgs(data);
        //            handler(null, arg);
        //        }
        //    }

        //    #endregion Update MainViewModel Event

        //    #region Status Event

        //    public delegate void StatusHandler(string msg);

        //    public event EventHandler<StatusArgs> StatusEvent;

        //    internal void RaiseStatusEvent(string msg)
        //    {
        //        var handler = StatusEvent;
        //        if (handler != null)
        //        {
        //            var arg = new StatusArgs(msg);
        //            handler(null, arg);
        //        }
        //    }

        //    public void TestFunction()
        //    {
        //        throw new NotImplementedException();
        //    }

        //    #endregion Status Event
        //
        public event EventHandler<ErrorArgs> ErrorEvent;

        public event EventHandler<StatusArgs> StatusEvent;

        public event EventHandler<UpdateMainViewModelArgs> UpdateMainViewModelEvent;

        public void Close()
        {
            throw new NotImplementedException();
        }

        public bool Initialize()
        {
            throw new NotImplementedException();
        }

        public void TestFunction()
        {
            throw new NotImplementedException();
        }
    }
}