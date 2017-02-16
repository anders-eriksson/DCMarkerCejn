using Configuration;
using Contracts;
using DCMarkerEF;
using LaserWrapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WorkFlow_Res = global::DCMarker.Properties.Resources;

namespace DCMarker.Model
{
    public class WorkFlow : IWorkFlow
    {
        private IArticleInput _articleInput;
        private DB _db;
        private Laser _laser;

        private List<Article> _articles;

        private DCConfig cfg;

        private IoSignals sig;

        private string _articleNumber;
        private int _currentEdge;
        private bool _hasEdges;

        public WorkFlow()
        {
            cfg = DCConfig.Instance;
            cfg.WriteConfig();
            sig = new IoSignals();
            UpdateIoMasks();
            Initialize();
        }

        public void Close()
        {
            _laser.Release();
            _articleInput.Close();
        }

        public bool Initialize()
        {
            bool result = true;
            //IsInitialized = false;
            try
            {
                InitializeMachine();
                //InitializeTcpServer();
                InitializeDatabase();
                InitializeLaser();
                //IsInitialized = true;
            }
            catch (Exception)
            {
                //IsInitialized = false;

                throw;
            }

            return result;
        }

        public void SimulateItemInPlace()
        {
            UpdateLayout();
        }

        public void Execute()
        {
            _laser.Execute();
        }

        private void _articleInput_ArticleEvent(object sender, ArticleArgs e)
        {
            RaiseErrorEvent(string.Empty);
            _laser.ResetPort(0, sig.MASK_READYTOMARK);
            _articleNumber = e.Data.ArticleNumber;
            RaiseStatusEvent(string.Format("Article {0} received", _articleNumber));

            _articles = _db.GetArticle(_articleNumber);

            if (_articles != null && _articles.Count > 0)
            {
                UpdateViewModel(_articles);
            }
            else
            {
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

        private void InitializeDatabase()
        {
            _db = new DB();
        }

        private void InitializeLaser()
        {
            _laser = new Laser();
            _laser.QueryStartEvent += _laser_QueryStartEvent;
            _laser.LaserEndEvent += _laser_LaserEndEvent;
            _laser.LaserErrorEvent += _laser_LaserErrorEvent;
            _laser.ItemInPositionEvent += _laser_ItemInPositionEvent;
        }

        private void _laser_QueryStartEvent(string msg)
        {
            RaiseStatusEvent(msg);
        }

        private void InitializeMachine()
        {
            _articleInput = new TcpArticleInput();
            _articleInput.ArticleEvent += _articleInput_ArticleEvent;
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
                                _db.AddHistoryDataToDB(historyData);
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
                RaiseErrorEvent("ItemInPlace received before Article Number is set!");
            }
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

        private void UpdateViewModel(List<Article> articles)
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

            RaiseUpdateMainViewModelEvent(data);
        }

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

        internal void RaiseStatusEvent(string msg)
        {
            var handler = StatusEvent;
            if (handler != null)
            {
                var arg = new StatusArgs(msg);
                handler(null, arg);
            }
        }

        public void ResetAllIoSignals()
        {
            if (_laser != null)
            {
                _laser.ResetPort(0, sig.MASK_ALL);
            }
        }

        #endregion Status Event
    }
}