using Configuration;
using Contracts;
using DCMarkerEF;
using LaserWrapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DCLog;
using GlblRes = global::DCMarker.Properties.Resources;

namespace DCMarker.Model
{
    public class WorkFlow : IWorkFlow
    {
        private IArticleInput _articleInput;
        private string _articleNumber;
        private List<Article> _articles;
        private int _currentEdge;
        private DB _db;
        private bool _hasEdges;
        private Laser _laser;
        private DCConfig cfg;

        private IoSignals sig;

        public bool FirstMarkingResetZ { get; set; }

        public WorkFlow()
        {
            try
            {
                cfg = DCConfig.Instance;
                if (!File.Exists(cfg.ConfigName))
                {
                    RaiseErrorMsgEvent("Config file is not found! dcmarker.xml in program directory");
                }
                sig = IoSignals.Instance;
                UpdateIoMasks();
                Initialize();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Close()
        {
            ResetAllIoSignals();
            _laser.Release();
            _articleInput.Close();
        }

#if DEBUG

        public void ArtNo(string artno)
        {
            throw new NotImplementedException();
        }

        public void StartOk()
        {
            throw new NotImplementedException();
        }

        public void Execute2()
        {
            throw new NotImplementedException();
        }

#endif

        public void Execute()
        {
            if (_laser != null)
            {
                _laser.Execute();
            }
        }

        public List<Article> GetArticle(string articleNumber)
        {
            List<Article> result;
            var maskinID = DCConfig.Instance.MaskinId;

            if (string.IsNullOrWhiteSpace(maskinID))
            {
                result = _db.GetArticle(articleNumber);
            }
            else
            {
                result = _db.GetArticle(articleNumber, maskinID);
            }

            return result;
        }

        public bool Initialize()
        {
            bool result = true;
            try
            {
                InitializeMachine();
                InitializeDatabase();
                InitializeLaser();
            }
            catch (Exception)
            {
                if (_articleInput != null)
                {
                    _articleInput.Close();
                }
                throw;
            }

            return result;
        }

        public void SimulateItemInPlace(int seq)
        {
            UpdateLayout();
        }

        public void SimulateItemInPlace(string articlenumber)
        {
            throw new NotImplementedException("Not implemented in Automatic");
        }

        private void _articleInput_ArticleEvent(object sender, ArticleArgs e)
        {
            RaiseErrorEvent(string.Empty);
            _laser.ResetPort(0, sig.MASK_READYTOMARK);
            _articleNumber = e.Data.ArticleNumber;
            RaiseStatusEvent(string.Format(GlblRes.Article_0_received, _articleNumber));

            var maskinID = DCConfig.Instance.MaskinId;
            if (string.IsNullOrWhiteSpace(maskinID))
            {
                _articles = _db.GetArticle(_articleNumber);
            }
            else
            {
                _articles = _db.GetArticle(_articleNumber, maskinID);
            }

            if (_articles != null && _articles.Count > 0)
            {
                UpdateViewModel(_articles);
            }
            else
            {
                Article empty = new Article();
                List<Article> emptyList = new List<Article>();
                emptyList.Add(empty);
                UpdateViewModel(emptyList);
                _laser.SetPort(0, sig.MASK_ERROR);
                // Can't find article in database.

                RaiseErrorEvent(string.Format(GlblRes.Article_not_defined_in_database_0, _articleNumber));
            }
        }

#if DEBUG

        public void _laser_ItemInPositionEvent()
#else

        private void _laser_ItemInPositionEvent()
#endif
        {
            UpdateLayout();
        }

        private void _laser_LaserEndEvent()
        {
            _laser.SetPort(0, sig.MASK_MARKINGDONE);
            RaiseStatusEvent(GlblRes.Marking_is_done);
        }

        private void _laser_LaserErrorEvent(string msg)
        {
            if (_laser != null)
            {
                if (!string.IsNullOrWhiteSpace(msg))
                {
                    _laser.SetPort(0, sig.MASK_ERROR);
                }
            }
            else
            {
                Log.Debug("_laser == null");
            }

            RaiseErrorEvent(msg);
        }

        private void _laser_QueryStartEvent(string msg)
        {
            RaiseStatusEvent(msg);
        }

        private void _laser_ResetIoEvent()
        {
            _laser.ResetPort(0, sig.MASK_ALL);
            RaiseErrorMsgEvent(string.Empty);
        }

        private List<LaserObjectData> ConvertToLaserObjectData(HistoryData historyData)
        {
            List<LaserObjectData> result;
            result = DB.ConvertHistoryDataToList(historyData);

            return result;
        }

        private HistoryData GetHistoryData(string _articleNumber, string kant, bool hasEdges = false)
        {
            HistoryData result = null;

            result = _db.CreateHistoryData(_articleNumber, kant, hasEdges);
            return result;
        }

        private void InitializeDatabase()
        {
            _db = new DB();
            _db.IsConnectionOK();
        }

        private void InitializeLaser()
        {
            _laser = new Laser();
            _laser.QueryStartEvent += _laser_QueryStartEvent;
            _laser.LaserEndEvent += _laser_LaserEndEvent;
            _laser.DeviceErrorEvent += _laser_LaserErrorEvent;
            _laser.ItemInPositionEvent += _laser_ItemInPositionEvent;
            _laser.ResetIoEvent += _laser_ResetIoEvent;
        }

        private void InitializeMachine()
        {
            try
            {
                _articleInput = new TcpArticleInput();
                _articleInput.ArticleEvent += _articleInput_ArticleEvent;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error initializing machine");
                throw;
            }
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

        /// <summary>
        /// Loads and updates the Layout when we have gotten an ItemInPlace signal from PLC
        /// </summary>
        public void UpdateLayout()
        {
            Article article = null;
            if (_articles != null && _articles.Count > 0)
            {
                if (_hasEdges && _currentEdge >= _articles.Count())
                {
                    _hasEdges = false;
                    _currentEdge = 1;
                }

                if (_hasEdges)
                {
                    _currentEdge++;
                    article = _articles[_currentEdge - 1];
                }
                else
                {
                    _currentEdge = 0;

                    article = _articles[_currentEdge];
                    _currentEdge++;
                }

                string layoutname = article.Template;
                if (!string.IsNullOrEmpty(layoutname))
                {
                    layoutname = NormalizeLayoutName(layoutname);
                    bool brc = _laser.Load(layoutname);
                    if (brc)
                    {
                        HistoryData historyData = GetHistoryData(_articleNumber, article.Kant, _hasEdges);
                        if (_articles.Count > 1)
                        {
                            _hasEdges = true;
                        }

                        if (historyData != null)
                        {
                            List<LaserObjectData> historyObjectData = ConvertToLaserObjectData(historyData);
                            historyData.MaskinID = DCConfig.Instance.MaskinId;
                            brc = _laser.Update(historyObjectData);
                            if (brc)
                            {
                                Log.Trace("Layout updated OK");
                                // update HistoryData table
                                var status = _db.AddHistoryDataToDB(historyData);
                                if (status != null)
                                {
                                    // we are ready to mark...
                                    RaiseStatusEvent(string.Format(GlblRes.Waiting_for_start_signal_0, layoutname));
                                    _laser.ResetPort(0, sig.MASK_MARKINGDONE);
                                    _laser.SetPort(0, sig.MASK_READYTOMARK);
                                    Log.Trace("UpdateLayout OK");
                                }
                                else
                                {
                                    RaiseErrorEvent(string.Format(GlblRes.Update_didnt_work_on_this_article_and_layout_Article0_Layout1, _articleNumber, layoutname));
                                    Log.Trace(string.Format(GlblRes.Update_didnt_work_on_this_article_and_layout_Article0_Layout1, _articleNumber, layoutname));
                                    _laser.SetPort(0, sig.MASK_ERROR);
                                }
                            }
                            else
                            {
                                RaiseErrorEvent(string.Format(GlblRes.HistoryData_Not_Created, _articleNumber, layoutname));
                                Log.Trace(string.Format(GlblRes.HistoryData_Not_Created, _articleNumber, layoutname));
                                _laser.SetPort(0, sig.MASK_ERROR);
                            }
                        }
                        else
                        {
                            RaiseErrorEvent(string.Format(GlblRes.Update_didnt_work_on_this_article_and_layout_Article0_Layout1, _articleNumber, layoutname));
                            Log.Trace(string.Format(GlblRes.Update_didnt_work_on_this_article_and_layout_Article0_Layout1, _articleNumber, layoutname));
                            _laser.SetPort(0, sig.MASK_ERROR);
                        }
                    }
                    else
                    {
                        RaiseErrorEvent(string.Format(GlblRes.Error_loading_layout_0, layoutname));
                        Log.Trace(string.Format(GlblRes.Error_loading_layout_0, layoutname));
                        _laser.SetPort(0, sig.MASK_ERROR);
                    }
                }
                else
                {
                    RaiseErrorEvent(string.Format(GlblRes.Layout_not_defined_for_this_article_Article0, _articleNumber));
                    Log.Trace(string.Format(GlblRes.Layout_not_defined_for_this_article_Article0, _articleNumber));
                    _laser.SetPort(0, sig.MASK_ERROR);
                }
            }
            else
            {
                RaiseErrorEvent(GlblRes.ItemInPlace_received_before_Article_Number_is_set);
                Log.Trace(GlblRes.ItemInPlace_received_before_Article_Number_is_set);
            }
        }

        private void UpdateViewModel(List<Article> articles)
        {
            UpdateViewModelData data = CreateUpdateViewModelData(articles);

            RaiseUpdateMainViewModelEvent(data);
        }

        private static UpdateViewModelData CreateUpdateViewModelData(List<Article> articles)
        {
            var data = new UpdateViewModelData();
            Article article = articles[0];
            data.ArticleNumber = article.F1;
            if (string.IsNullOrWhiteSpace(article.Kant))
            {
                data.HasKant = false;
                data.Kant = article.Kant;
            }
            else
            {
                data.HasKant = true;
                data.Kant = articles.Count.ToString();
            }
            data.MaskinID = article.MaskinID;
            data.Fixture = article.FixtureId;
            data.HasFixture = string.IsNullOrWhiteSpace(data.Fixture) ? false : true;
            data.HasTOnr = article.EnableTO.HasValue ? article.EnableTO.Value : false;
            data.Template = article.Template;
            return data;
        }

        #region only used by ManualWorkFlow // AME - 2017-05-12

        public void UpdateWorkflow(Article article)
        {
            throw new NotImplementedException();
        }

        public void ResetArticleData()
        {
            throw new NotImplementedException();
        }

        public void ResetArticleReady()
        {
            throw new NotImplementedException();
        }

        public bool ResetZAxis()
        {
            throw new NotImplementedException();
        }

        public void SetNextToLast()
        {
            throw new NotImplementedException();
        }

        public void ResetNextToLast()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Starts polling of an IO service
        /// Only used in connection of a hardware that don't use Events
        /// </summary>
        /// <param name="pollInterval">number of milliseconds between each poll</param>
        /// <param name="errorTimeout">number of milliseconds untile an error timeout is triggered</param>
        /// <returns>true if successful, otherwise false</returns>

        public bool StartPoll(int pollInterval, int errorTimeout)
        {
            return true;
        }

        #endregion only used by ManualWorkFlow // AME - 2017-05-12

        #region only used by NippleWorkFlow // AME - 2018-05-12

        public void LoadArticleNumber(string _articleNumber)
        {
            throw new NotImplementedException();
        }

        public void LoadUpdateLayout()
        {
            throw new NotImplementedException();
        }

        public void UpdateTOnumber(string onr)
        {
            throw new NotImplementedException();
        }

        public void UserHasApprovedTOnumber(bool state)
        {
            throw new NotImplementedException();
        }

        #region Article has TO-number Event

        public delegate void ArticleHasToNumberHandler(bool state);

        public event EventHandler<ArticleHasToNumberArgs> ArticleHasToNumberEvent;

        internal void RaiseArticleHasToNumberEvent(bool state)
        {
            var handler = ArticleHasToNumberEvent;
            if (handler != null)
            {
                var arg = new ArticleHasToNumberArgs(state);
                handler(null, arg);
            }
        }

        #endregion Article has TO-number Event

        #endregion only used by NippleWorkFlow // AME - 2018-05-12

        #region only used in FlexibleWorkFlow // AME - 2018-11-05

        public event EventHandler<ItemDoneArgs> ItemDoneEvent;

        public void ResetCareful()
        {
            throw new NotImplementedException();
        }

        public void ResetItemsDone()
        {
            throw new NotImplementedException();
        }

        public event EventHandler<UpdateItemStatusArgs> UpdateItemStatusEvent;

        public event EventHandler<SetupItemStatusArgs> SetupItemStatusEvent;

        #endregion only used in FlexibleWorkFlow // AME - 2018-11-05

        #region only used by CO208

        public event EventHandler<UpdateSerialNumberArgs> UpdateSerialNumberEvent;

        #endregion only used by CO208

        #region Laser Busy Event

        // only used by ManualWorkFlow // AME - 2017-05-12
        public event EventHandler<LaserBusyEventArgs> LaserBusyEvent;

        #endregion Laser Busy Event

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

        #region Update MainViewModel Event

        public delegate void UpdateMainViewModelHandler(UpdateViewModelData data);

        public event EventHandler<UpdateMainViewModelArgs> UpdateMainViewModelEvent;

        internal void RaiseUpdateMainViewModelEvent(UpdateViewModelData data)
        {
            var handler = UpdateMainViewModelEvent;
            if (handler != null)
            {
                var arg = new UpdateMainViewModelArgs(data);
                handler(null, arg);
            }
        }

        #endregion Update MainViewModel Event

        #region Status Event

        public delegate void StatusHandler(string msg);

        public event EventHandler<StatusArgs> StatusEvent;

        public void ResetAllIoSignals()
        {
            if (_laser != null)
            {
                _laser.ResetPort(0, sig.MASK_ALL);
                _laser.SetReady(false);
            }
        }

        internal void RaiseStatusEvent(string msg)
        {
            var handler = StatusEvent;
            if (handler != null)
            {
                var arg = new StatusArgs(msg);
                handler(null, arg);
            }
        }

        #endregion Status Event

        #region Update Error Message Event

        public delegate void ErrorMsgHandler(string msg);

        public event EventHandler<StatusArgs> ErrorMsgEvent;

        internal void RaiseErrorMsgEvent(string msg)
        {
            var handler = ErrorMsgEvent;
            if (handler != null)
            {
                var arg = new StatusArgs(msg);
                handler(null, arg);
            }
        }

        public class ErrorMsgArgs : EventArgs
        {
            public ErrorMsgArgs(string s)
            {
                Text = s;
            }

            public string Text { get; private set; } // readonly
        }

        #endregion Update Error Message Event
    }
}