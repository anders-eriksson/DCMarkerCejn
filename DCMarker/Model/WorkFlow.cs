using Configuration;
using Contracts;
using DCMarkerEF;
using LaserWrapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WorkFlow_Res = global::DCMarker.Properties.Resources;
using DCLog;

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

        public WorkFlow()
        {
            try
            {
                cfg = DCConfig.Instance;
                if (!File.Exists(cfg.ConfigName))
                {
                    // if the config file doesn't exist then we are using default values. Write them to disk.
                    cfg.WriteConfig();
                }
                sig = new IoSignals();
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

        public void Execute()
        {
            if (_laser != null)
            {
                _laser.Execute();
            }
        }

        public List<Article> GetArticle(string articleNumber)
        {
            return _db.GetArticle(articleNumber);
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

        public void SimulateItemInPlace()
        {
            UpdateLayout();
        }

        private void _articleInput_ArticleEvent(object sender, ArticleArgs e)
        {
            RaiseErrorEvent(string.Empty);
            _laser.ResetPort(0, sig.MASK_READYTOMARK);
            _articleNumber = e.Data.ArticleNumber;
            RaiseStatusEvent(string.Format(WorkFlow_Res.Article_0_received, _articleNumber));

            _articles = _db.GetArticle(_articleNumber);

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

                RaiseErrorEvent(string.Format(WorkFlow_Res.Article_not_defined_in_database_Article0, _articleNumber));
            }
        }

        private void _laser_ItemInPositionEvent()
        {
            UpdateLayout();
        }

        private void _laser_LaserEndEvent()
        {
            _laser.SetPort(0, sig.MASK_MARKINGDONE);
            RaiseStatusEvent(WorkFlow_Res.Marking_is_done);
        }

        private void _laser_LaserErrorEvent(string msg)
        {
            _laser.SetPort(0, sig.MASK_ERROR);
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

            if (layoutname.IndexOf(WorkFlow_Res.xlp) < 0)
            {
                layoutname += WorkFlow_Res.xlp;
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
        private void UpdateLayout()
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
                            brc = _laser.Update(historyObjectData);
                            if (brc)
                            {
                                // update HistoryData table
                                var status = _db.AddHistoryDataToDB(historyData);
                                if (status != null)
                                {
                                    // we are ready to mark...
                                    RaiseStatusEvent(string.Format(WorkFlow_Res.Waiting_for_start_signal_0, layoutname));
                                    _laser.ResetPort(0, sig.MASK_MARKINGDONE);
                                    _laser.SetPort(0, sig.MASK_READYTOMARK);
                                }
                                else
                                {
                                    RaiseErrorEvent(string.Format(WorkFlow_Res.Update_didnt_work_on_this_article_and_layout_Article0_Layout1, _articleNumber, layoutname));
                                    _laser.SetPort(0, sig.MASK_ERROR);
                                }
                            }
                            else
                            {
                                RaiseErrorEvent(string.Format(WorkFlow_Res.Update_didnt_work_on_this_article_and_layout_Article0_Layout1, _articleNumber, layoutname));
                                _laser.SetPort(0, sig.MASK_ERROR);
                            }
                        }
                    }
                    else
                    {
                        RaiseErrorEvent(string.Format(WorkFlow_Res.Error_loading_layout_0, layoutname));
                        _laser.SetPort(0, sig.MASK_ERROR);
                    }
                }
                else
                {
                    RaiseErrorEvent(string.Format(WorkFlow_Res.Layout_not_defined_for_this_article_Article0, _articleNumber));
                    _laser.SetPort(0, sig.MASK_ERROR);
                }
            }
            else
            {
                RaiseErrorEvent(WorkFlow_Res.ItemInPlace_received_before_Article_Number_is_set);
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
            data.Fixture = article.FixtureId;
            data.HasFixture = string.IsNullOrWhiteSpace(data.Fixture) ? false : true;
            data.HasTOnr = article.EnableTO.HasValue ? article.EnableTO.Value : false;
            data.Template = article.Template;
            return data;
        }

        // only used by ManualWorkFlow // AME - 2017-05-12
        public void UpdateWorkflow(Article article)
        {
            throw new NotImplementedException();
        }

        // only used by ManualWorkFlow // AME - 2017-05-12
        public void ResetArticleData()
        {
            throw new NotImplementedException();
        }

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