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
using System.Threading;

namespace DCMarker.Model
{
    public class NippleWorkFlow : IWorkFlow
    {
        private AdamArticleInput _articleInput;
        private string _articleNumber;
        private List<Article> _articles;
        private int _currentEdge;
        private int _totalEdges;
        private DB _db;
        private bool _hasEdges;
        private Laser _laser;
        private DCConfig cfg;
        private volatile bool HasError = false;
        private volatile bool ArticleHasToNumber = false;
        private volatile string TOnumber;
        private volatile bool IsTOnumberUpdated = false;
        private volatile bool IsLoadOnStartup = false;
        private volatile bool IsTOnumberApproved = false;
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

#if DEBUG

        public void ArtNo(string artno)
        {
            //ArticleData data = new ArticleData();
            //data.ArticleNumber = artno;
            //ArticleArgs e = new ArticleArgs(data);
            //_articleInput_ArticleEvent(this, e);

            _articleInput.ReadCommand((byte)CommandTypes.ArtNo, artno);
        }

        public void StartOk()
        {
            _articleInput.ReadCommand((byte)CommandTypes.OK, _currentEdge, _totalEdges);
        }

        private int edge = 1;

        public void Execute()
        {
            _articleInput.ReadCommand((byte)CommandTypes.StartMarking, edge++, _totalEdges);
        }

        public void Execute2()
        {
            _articleInput.ReadCommand((byte)CommandTypes.StartMarking2, _totalEdges, _totalEdges);
            edge = 1;
        }

#endif

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

        private bool x = true;

        public void SimulateItemInPlace(int seq)
        {
            ArticleData data = new ArticleData();
            if (x)
                data.ArticleNumber = "101156152";
            else
                //data.ArticleNumber = "101156202";
                data.ArticleNumber = "101151515";
            x = !x;
            ArticleArgs article = new ArticleArgs(data);

            _articleInput_ArticleEvent(this, article);
            ////string commandfile = string.Format("COMMANDS{0}.TXT", seq);
            //string commandfile = "COMMANDS1.TXT";
            //_articleInput.Simulate(commandfile);
            ////UpdateLayout();
        }

        public void SimulateItemInPlace(string articlenumber)
        {
            throw new NotImplementedException("Not implemented in Nipple");
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

            StartPoll();
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

        public bool StartPoll()
        {
            return _articleInput.StartPoll();
        }

        private void _articleInput_ErrorEvent(object sender, ErrorArgs e)
        {
            RaiseErrorEvent(e.Text);
        }

        private void _articleInput_StartMarkingEvent(object sender, EventArgs e)
        {
            _articleInput.ReadyToMark(false);
            bool berror = _laser.Execute();
            if (!berror)
            {
            }
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

        public void _articleInput_ArticleEvent(object sender, ArticleArgs e)
        {
            HasError = false;
            IsLoadOnStartup = false;
            IsTOnumberApproved = false;
            IsTOnumberUpdated = false;
            Log.Trace("ArticleEvent");
            _currentEdge = 0;
            RaiseErrorEvent(string.Empty);
            //_laser.ResetPort(0, sig.MASK_READYTOMARK);
            _articleNumber = e.Data.ArticleNumber;
            e.Data.IsNewArticleNumber = true;
            ArticleHasToNumber = false;
            GetArticleDbData(e);
        }

        private void GetArticleDbData(ArticleArgs e)
        {
            Log.Trace("GetArticleDbData");
            RaiseStatusEvent(string.Format(GlblRes.Article_0_received, e.Data.ArticleNumber));

            _articles = _db.GetArticle(e.Data.ArticleNumber);
            LogArticles(_articles);
            if (_articles != null && _articles.Count > 0)
            {
                Log.Trace("_articles != null && _articles.Count > 0");
                _totalEdges = _articles.Count;
                UpdateViewModel(_articles, e);
            }
            else
            {
                Log.Error(string.Format("Article not found: {0}", e.Data.ArticleNumber));
                Article empty = new Article();
                List<Article> emptyList = new List<Article>();
                emptyList.Add(empty);
                UpdateViewModel(emptyList, e);
                _articleInput.Error((byte)Errors.ArticleNotFound);

                HasError = true;
                RaiseErrorEvent(string.Format(GlblRes.Article_not_defined_in_database_0, e.Data.ArticleNumber));
            }
        }

        private static void LogArticles(List<Article> articles)
        {
            foreach (Article a in articles)
            {
                Log.Trace(string.Format("Article: {0} - Kant: {1}", a.F1, a.Kant));
            }
        }

        public void LoadArticleNumber(string articleNumber)
        {
            _articleInput.SetArticleData(articleNumber);
            //HasError = false;
            //IsLoadOnStartup = true;
            //IsTOnumberApproved = false;
            //IsTOnumberUpdated = false;
            //Log.Trace("ArticleEvent");
            //_currentEdge = 0;
            //RaiseErrorEvent(string.Empty);
            //_articleNumber = articleNumber;
            //ArticleData data = new ArticleData();
            //data.ArticleNumber = articleNumber;
            //data.IsNewArticleNumber = true;
            //var e = new ArticleArgs(data);
            //ArticleHasToNumber = false;
            //GetArticleDbData(e);
        }

        public void LoadUpdateLayout()
        {
            if (!HasError)
            {
                UpdateLayout();
                StartPoll(DCConfig.Instance.AdamPollInterval, DCConfig.Instance.AdamErrorTimeout);
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

        /// <summary>
        /// Loads and updates the Layout when we have gotten an Start/OK signal from PLC
        /// </summary>
        public void UpdateLayout()
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
                if (_hasEdges)
                {
                    Log.Trace("HasEdges");
                    article = _articles[_currentEdge];

                    if (ArticleHasToNumber)
                    {
                        if (!IsTOnumberUpdated)
                        {
                            RaiseStatusEvent(GlblRes.Waiting_for_TOnumber);

                            WaitForToNumber();
                            article.TOnumber = TOnumber;
                            IsTOnumberUpdated = true;
                        }
                    }
                    else
                    {
                        article.TOnumber = string.Empty;
                    }
                    ArticleData data = new ArticleData();
                    data.ArticleNumber = article.F1;
                    data.IsNewArticleNumber = false;
                    ArticleArgs e = new ArticleArgs(data);
                    UpdateViewModel(_articles, e);
                    _currentEdge++;
                }
                else
                {
                    Log.Trace("No Edges");
                    _currentEdge = 0;

                    article = _articles[_currentEdge];
                    if (ArticleHasToNumber)
                    {
                        if (!IsTOnumberUpdated)
                        {
                            RaiseStatusEvent(GlblRes.Waiting_for_TOnumber);

                            WaitForToNumber();
                            article.TOnumber = TOnumber;
                            IsTOnumberUpdated = true;
                        }
                    }
                    else
                    {
                        article.TOnumber = string.Empty;
                    }
                    Log.Trace(string.Format("article.TOnummer: {0}", article.TOnumber));

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
                            //if (article.EnableTO.HasValue && article.EnableTO.Value)
                            //{
                            LaserObjectData dta = new LaserObjectData();
                            dta.ID = "TO";
                            dta.Value = article.TOnumber;
                            historyObjectData.Add(dta);
                            //}
                            brc = _laser.Update(historyObjectData);
#if DEBUG
                            _laser.SaveDoc();
#endif
                            if (brc)
                            {
                                Log.Trace("Layout updated OK");
                                // update HistoryData table
                                var status = _db.AddHistoryDataToDB(historyData);
                                if (status != null)
                                {
                                    // we are ready to mark...
                                    RaiseStatusEvent(string.Format(GlblRes.Waiting_for_start_signal_0, layoutname));
                                    //if (!IsLoadOnStartup)
                                    {
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
                                    }
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

        private void WaitForToNumber()
        {
            try
            {
                Log.Trace("WaitForTOnumber");
                while (!IsTOnumberApproved || string.IsNullOrWhiteSpace(TOnumber))
                {
                    Thread.Sleep(1);
                }
                Log.Trace("WaitForTOnumber Done");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unknown Error");
                throw;
            }
        }

        public void UserHasApprovedTOnumber(bool state)
        {
            IsTOnumberApproved = true;
        }

        private void UpdateLayoutKant()
        {
            // The first time we get here we have already marked one!!
            //      which means that _currentEdge is 1

            Log.Trace(string.Format("UpdateLayoutKant - currentEdge: {0}", _currentEdge));
            Article article = null;
            if (HasError)
            {
                Log.Trace("HasError");
                return;
            }
            if (_hasEdges)
            {
                Log.Trace(string.Format("HasEdges - _articles.Count: {0} - currentEdge: {1}", _articles.Count, _currentEdge));

                if (_currentEdge < _articles.Count)
                {
                    article = _articles[_currentEdge];
                    if (ArticleHasToNumber)
                    {
                        article.TOnumber = TOnumber;
                    }
                    else
                    {
                        article.TOnumber = string.Empty;
                    }
                    Log.Trace(string.Format("article.TOnummer: {0}", article.TOnumber));
                    Log.Trace(string.Format("article.kant: {0}", article.Kant));
                    ArticleData data = new ArticleData();
                    data.ArticleNumber = string.Empty;  //article.F1;
                    data.IsNewArticleNumber = false;
                    ArticleArgs e = new ArticleArgs(data);
                    UpdateViewModel(_articles, e);
                    _currentEdge++;
                }
                else
                {
                    _currentEdge++;
                }
            }
            else
            {
                Log.Trace("No Edges");
                //_currentEdge = 0;
                if (_currentEdge < _articles.Count)
                {
                    article = _articles[_currentEdge];
                    if (ArticleHasToNumber)
                    {
                        article.TOnumber = TOnumber;
                    }
                    else
                    {
                        article.TOnumber = string.Empty;
                    }

                    //if (ArticleHasToNumber && !IsTOnumberUpdated)
                    if (ArticleHasToNumber)
                    {
                        Log.Trace("ArticleHasToNumber");
                        article.TOnumber = TOnumber;
                    }
                    else
                    {
                        Log.Trace("ELSE ");
                        try
                        {
                            article.TOnumber = string.Empty;
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "updating article.TOnumber");
                            throw;
                        }
                    }
                    //article = _articles[_currentEdge];
                    _currentEdge++;
                }
                else
                {
                    _currentEdge++;
                }
            }
            Log.Trace("Handled Edges!");
            if (_articles != null && _articles.Count > 0)
            {
                Log.Trace("_articles != null && _articles.Count > 0");
                if (_currentEdge > _articles.Count)
                {
                    Log.Trace("_currentEdge > _articles.Count");
                    RaiseStatusEvent(GlblRes.Marking_is_done);
                    _articleInput.BatchNotReady(false);
                    Log.Trace("Marking is done! BatchNotReady==false");
                    _currentEdge = 0;
                    return;
                }
                else
                {
                    Log.Trace(string.Format("Template: {0} - currentEdge: {1}", article.Template, _currentEdge));
                }

                string layoutname = article.Template;
                if (!string.IsNullOrEmpty(layoutname))
                {
                    Log.Trace(string.Format("Layout: {0}", layoutname));
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
                            // Add TO-number
                            LaserObjectData dta = new LaserObjectData();
                            dta.ID = "TO";
                            dta.Value = article.TOnumber;
                            historyObjectData.Add(dta);

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
                                        _articleInput.SetKant(Convert.ToByte(article.Kant));
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

        public void UpdateTOnumber(string tonr)
        {
            TOnumber = tonr;
            IsTOnumberUpdated = false;
        }

        private void UpdateViewModel(List<Article> articles, ArticleArgs e)
        {
            Log.Trace("UpdateViewModel");
            UpdateViewModelData data = CreateUpdateViewModelData(articles, e);

            RaiseUpdateMainViewModelEvent(data);
        }

        private UpdateViewModelData CreateUpdateViewModelData(List<Article> articles, ArticleArgs e)
        {
            Log.Trace("CreateUpdateViewModelData");
            var data = new UpdateViewModelData();
            Article article = articles[_currentEdge];
            data.TotalKant = articles.Count.ToString();
            data.Provbit = e.Data.TestItem;
            data.ArticleNumber = string.IsNullOrWhiteSpace(article.F1) ? e.Data.ArticleNumber : article.F1;
            data.IsNewArticleNumber = e.Data.IsNewArticleNumber;
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

            // TO-number should only be entered once when the article is loaded!
            if (!ArticleHasToNumber)
            {
                data.HasTOnr = false;

                foreach (Article a in articles)
                {
                    if (a.EnableTO.HasValue)
                    {
                        data.HasTOnr = a.EnableTO.Value;
                    }
                }
                if (data.HasTOnr)
                {
                    ArticleHasToNumber = true;
                    RaiseArticleHasToNumberEvent(true);
                }
                else
                {
                    ArticleHasToNumber = false;
                    RaiseArticleHasToNumberEvent(false);
                }
            }

            data.Template = article.Template;
            data.HasTOnr = ArticleHasToNumber;

            Log.Trace("CreateUpdateViewModelData Done");
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
    }
}