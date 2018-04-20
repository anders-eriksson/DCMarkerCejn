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
    public class NippleWorkFlow : IWorkFlow
    {
        private AdamArticleInput _articleInput;
        private string _articleNumber;
        private List<Article> _articles;
        private int _currentEdge;
        private DB _db;
        private bool _hasEdges;
        private Laser _laser;
        private DCConfig cfg;
        private volatile bool HasError = false;

        public bool FirstMarkingResetZ { get; set; }

        public NippleWorkFlow()
        {
            try
            {
                cfg = DCConfig.Instance;
                Initialize();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Close()
        {
            //ResetAllIoSignals();
            _laser.SetReady(false);
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
            _articleInput.Simulate("COMMANDS2.TXT");
            //UpdateLayout();
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
            UpdateLayoutKant();
            //_articleInput.IsLaserMarking = false;
        }

        private void _laser_LaserErrorEvent(string msg)
        {
            if (_laser != null)
            {
                if (!string.IsNullOrWhiteSpace(msg))
                {
                    //_laser.SetPort(0, sig.MASK_ERROR);
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
            //_laser.ResetPort(0, sig.MASK_ALL);
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
                _articleInput = new AdamArticleInput();
                _articleInput.ArticleEvent += _articleInput_ArticleEvent;
                _articleInput.ItemInPlaceEvent += _articleInput_ItemInPlaceEvent;
                _articleInput.LaserEndEvent += _articleInput_LaserEndEvent;
                _articleInput.RestartEvent += _articleInput_RestartEvent;
                _articleInput.StartMarkingEvent += _articleInput_StartMarkingEvent;
                _articleInput.ErrorEvent += _articleInput_ErrorEvent;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error initializing machine");
                throw;
            }
        }

        public bool StartPoll(int pollintervall, int errorTimeout)
        {
            return _articleInput.StartPoll(pollintervall, errorTimeout);
        }

        private void _articleInput_ErrorEvent(object sender, ErrorArgs e)
        {
            RaiseErrorEvent(e.Text);
        }

        private void _articleInput_StartMarkingEvent(object sender, EventArgs e)
        {
            _articleInput.ReadyToMark(false);
            _laser.Execute();
        }

        private void _articleInput_RestartEvent(object sender, EventArgs e)
        {
            //Log.Info("Restart of application is not implemented!");
            _laser.SetReady(false);
            _laser.SetReady(true);
        }

        private void _articleInput_LaserEndEvent(object sender, EventArgs e)
        {
            _laser.StopMarking();
        }

        private void _articleInput_ItemInPlaceEvent(object sender, EventArgs e)
        {
            UpdateLayout();
        }

        private void _articleInput_ArticleEvent(object sender, ArticleArgs e)
        {
            HasError = false;
            Log.Trace("ArticleEvent");
            RaiseErrorEvent(string.Empty);
            //_laser.ResetPort(0, sig.MASK_READYTOMARK);
            _articleNumber = e.Data.ArticleNumber;
            GetArticleDbData(e);
        }

        private void GetArticleDbData(ArticleArgs e)
        {
            RaiseStatusEvent(string.Format(GlblRes.Article_0_received, _articleNumber));

            _articles = _db.GetArticle(_articleNumber);

            if (_articles != null && _articles.Count > 0)
            {
                UpdateViewModel(_articles, e);
            }
            else
            {
                Article empty = new Article();
                List<Article> emptyList = new List<Article>();
                emptyList.Add(empty);
                UpdateViewModel(emptyList, e);
                //_laser.SetPort(0, sig.MASK_ERROR);
                _articleInput.Error((byte)Errors.ArticleNotFound);
                // Can't find article in database.

                HasError = true;
                RaiseErrorEvent(string.Format(GlblRes.Article_not_defined_in_database_0, _articleNumber));
            }
        }

        public void LoadArticleNumber(string articleNumber)
        {
            HasError = false;
            Log.Trace("ArticleEvent");
            RaiseErrorEvent(string.Empty);
            ArticleData data = new ArticleData();
            data.ArticleNumber = articleNumber;
            ArticleArgs e = new ArticleArgs(data);
            GetArticleDbData(e);
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

        /// <summary>
        /// Loads and updates the Layout when we have gotten an Start/OK signal from PLC
        /// </summary>
        private void UpdateLayout()
        {
            Log.Trace("UpdateLayout");
            Article article = null;
            if (HasError)
            {
                Log.Trace("HasError");
                return;
            }

            if (_articles != null && _articles.Count > 0)
            {
                //Log.Trace(string.Format("currentEdge: {0}", _currentEdge));
                //// TODO: check if the conditions is correct. should it really be >=
                //if (_currentEdge >= _articles.Count())
                //{
                //    //if (_hasEdges)
                //    {
                //        //  all edges has been processed
                //        _hasEdges = false;
                //        _currentEdge = 0;
                //    }
                //    _articleInput.SetKant(1);
                //    _articleInput.BatchNotReady(false);
                //    _articleInput.ReadyToMark(true);
                //    return;
                //}
                if (_hasEdges)
                {
                    Log.Trace("HasEdges");
                    article = _articles[_currentEdge];

                    ArticleData data = new ArticleData();
                    data.ArticleNumber = article.F1;
                    ArticleArgs e = new ArticleArgs(data);
                    UpdateViewModel(_articles, e);
                    _currentEdge++;
                }
                else
                {
                    Log.Trace("No Edges");
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
                        Log.Trace(string.Format("Layout loaded: {0}", layoutname));
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
                                Log.Trace("Layout updated OK");
                                // update HistoryData table
                                var status = _db.AddHistoryDataToDB(historyData);
                                if (status != null)
                                {
                                    // we are ready to mark...
                                    RaiseStatusEvent(string.Format(GlblRes.Waiting_for_start_signal_0, layoutname));
                                    if (_hasEdges)
                                    {
                                        _articleInput.SetKant(Convert.ToByte(_currentEdge));
                                    }
                                    else
                                    {
                                        _articleInput.SetKant(1);
                                    }
                                    _articleInput.BatchNotReady(true);

                                    _articleInput.ReadyToMark(true);

                                    Log.Trace("UpdateLayout OK");
                                }
                                else
                                {
                                    RaiseErrorEvent(string.Format(GlblRes.Update_didnt_work_on_this_article_and_layout_Article0_Layout1, _articleNumber, layoutname));
                                    Log.Trace(string.Format(GlblRes.Update_didnt_work_on_this_article_and_layout_Article0_Layout1, _articleNumber, layoutname));
                                    //_laser.SetPort(0, sig.MASK_ERROR);
                                }
                            }
                            else
                            {
                                RaiseErrorEvent(string.Format(GlblRes.HistoryData_Not_Created, _articleNumber, layoutname));
                                Log.Trace(string.Format(GlblRes.HistoryData_Not_Created, _articleNumber, layoutname));
                                //_laser.SetPort(0, sig.MASK_ERROR);
                            }
                        }
                        else
                        {
                            RaiseErrorEvent(string.Format(GlblRes.Update_didnt_work_on_this_article_and_layout_Article0_Layout1, _articleNumber, layoutname));
                            Log.Trace(string.Format(GlblRes.Update_didnt_work_on_this_article_and_layout_Article0_Layout1, _articleNumber, layoutname));
                            //_laser.SetPort(0, sig.MASK_ERROR);
                        }
                    }
                    else
                    {
                        RaiseErrorEvent(string.Format(GlblRes.Error_loading_layout_0, layoutname));
                        Log.Trace(string.Format(GlblRes.Error_loading_layout_0, layoutname));
                        //_laser.SetPort(0, sig.MASK_ERROR);
                        _articleInput.Error((byte)Errors.LayoutNotFound);
                    }
                }
                else
                {
                    RaiseErrorEvent(string.Format(GlblRes.Layout_not_defined_for_this_article_Article0, _articleNumber));
                    Log.Trace(string.Format(GlblRes.Layout_not_defined_for_this_article_Article0, _articleNumber));
                    //_laser.SetPort(0, sig.MASK_ERROR);
                    _articleInput.Error((byte)Errors.LayoutNotDefined);
                }
            }
            else
            {
                RaiseErrorEvent(GlblRes.ItemInPlace_received_before_Article_Number_is_set);
                Log.Trace(GlblRes.ItemInPlace_received_before_Article_Number_is_set);
            }
        }

        private void UpdateLayoutKant()
        {
            Log.Trace("UpdateLayoutKant");
            Article article = null;
            if (HasError)
            {
                Log.Trace("HasError");
                return;
            }
            if (_hasEdges)
            {
                Log.Trace("HasEdges");
                article = _articles[_currentEdge];

                ArticleData data = new ArticleData();
                data.ArticleNumber = article.F1;
                ArticleArgs e = new ArticleArgs(data);
                UpdateViewModel(_articles, e);
                _currentEdge++;
            }
            else
            {
                Log.Trace("No Edges");
                _currentEdge = 0;

                //article = _articles[_currentEdge];
                _currentEdge++;
            }

            if (_articles != null && _articles.Count > 0)
            {
                if (_currentEdge > _articles.Count)
                {
                    RaiseStatusEvent(GlblRes.Marking_is_done);
                    _articleInput.BatchNotReady(false);
                    _currentEdge = 0;
                    return;
                }

                string layoutname = article.Template;
                if (!string.IsNullOrEmpty(layoutname))
                {
                    layoutname = NormalizeLayoutName(layoutname);
                    bool brc = _laser.Load(layoutname);
                    if (brc)
                    {
                        Log.Trace(string.Format("Layout loaded: {0}", layoutname));
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
                                Log.Trace("Layout updated OK");
                                // update HistoryData table
                                var status = _db.AddHistoryDataToDB(historyData);
                                if (status != null)
                                {
                                    // we are ready to mark...
                                    RaiseStatusEvent(string.Format(GlblRes.Waiting_for_start_signal_0, layoutname));
                                    if (_hasEdges)
                                    {
                                        _articleInput.SetKant(Convert.ToByte(_currentEdge));
                                    }
                                    else
                                    {
                                        _articleInput.SetKant(1);
                                    }

                                    _articleInput.ReadyToMark(true);

                                    _articleInput.IsLaserMarking = false;
                                    Log.Trace("UpdateLayoutKant OK");
                                }
                                else
                                {
                                    RaiseErrorEvent(string.Format(GlblRes.Update_didnt_work_on_this_article_and_layout_Article0_Layout1, _articleNumber, layoutname));
                                    Log.Trace(string.Format(GlblRes.Update_didnt_work_on_this_article_and_layout_Article0_Layout1, _articleNumber, layoutname));
                                    //_laser.SetPort(0, sig.MASK_ERROR);
                                }
                            }
                            else
                            {
                                RaiseErrorEvent(string.Format(GlblRes.HistoryData_Not_Created, _articleNumber, layoutname));
                                Log.Trace(string.Format(GlblRes.HistoryData_Not_Created, _articleNumber, layoutname));
                                //_laser.SetPort(0, sig.MASK_ERROR);
                            }
                        }
                        else
                        {
                            RaiseErrorEvent(string.Format(GlblRes.Update_didnt_work_on_this_article_and_layout_Article0_Layout1, _articleNumber, layoutname));
                            Log.Trace(string.Format(GlblRes.Update_didnt_work_on_this_article_and_layout_Article0_Layout1, _articleNumber, layoutname));
                            //_laser.SetPort(0, sig.MASK_ERROR);
                        }
                    }
                    else
                    {
                        RaiseErrorEvent(string.Format(GlblRes.Error_loading_layout_0, layoutname));
                        Log.Trace(string.Format(GlblRes.Error_loading_layout_0, layoutname));
                        //_laser.SetPort(0, sig.MASK_ERROR);
                        _articleInput.Error((byte)Errors.LayoutNotFound);
                    }
                }
                else
                {
                    RaiseErrorEvent(string.Format(GlblRes.Layout_not_defined_for_this_article_Article0, _articleNumber));
                    Log.Trace(string.Format(GlblRes.Layout_not_defined_for_this_article_Article0, _articleNumber));
                    //_laser.SetPort(0, sig.MASK_ERROR);
                    _articleInput.Error((byte)Errors.LayoutNotDefined);
                }
            }
            else
            {
                RaiseErrorEvent(GlblRes.ItemInPlace_received_before_Article_Number_is_set);
                Log.Trace(GlblRes.ItemInPlace_received_before_Article_Number_is_set);
            }
        }

        private void UpdateViewModel(List<Article> articles, ArticleArgs e)
        {
            UpdateViewModelData data = CreateUpdateViewModelData(articles, e);

            RaiseUpdateMainViewModelEvent(data);
        }

        private UpdateViewModelData CreateUpdateViewModelData(List<Article> articles, ArticleArgs e)
        {
            var data = new UpdateViewModelData();
            Article article = articles[_currentEdge];
            data.TotalKant = articles.Count.ToString();
            data.Provbit = e.Data.TestItem;
            data.ArticleNumber = string.IsNullOrWhiteSpace(article.F1) ? e.Data.ArticleNumber : article.F1;
            if (string.IsNullOrWhiteSpace(article.Kant))
            {
                data.HasKant = false;
                data.Kant = article.Kant;
            }
            else
            {
                data.HasKant = true;
                //data.Kant = articles.Count.ToString();
                data.Kant = article.Kant;
            }
            data.Fixture = article.FixtureId;
            data.HasFixture = string.IsNullOrWhiteSpace(data.Fixture) ? false : true;
            data.HasTOnr = article.EnableTO.HasValue ? article.EnableTO.Value : false;
            data.Template = article.Template;
            return data;
        }

        public void ResetAllIoSignals()
        {
            if (_laser != null)
            {
                _laser.SetReady(false);
            }
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

        #endregion only used by ManualWorkFlow // AME - 2017-05-12

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