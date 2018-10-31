using Configuration;
using Contracts;
using DCMarkerEF;
using LaserWrapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DCLog;
using DCMarker.Flexible;
using DCMarker.Model;

using GlblRes = global::DCMarker.Properties.Resources;

namespace DCMarker.Flexible
{
    public partial class FlexibleWorkFlow : IWorkFlow
    {
        private readonly DCConfig cfg;
        private readonly IoSignals sig;
        private string _articleNumber;
        private List<Article> _articles;
        private FlexibleItem[] _items;
        private int _currentItem;
        private int _currentEdge;
        private DB _db;
        private bool _hasEdges;
        private Laser _laser;
        private volatile bool ArticleHasToNumber = false;
        private volatile string TOnumber;
        private volatile bool IsTOnumberUpdated = false;
        private volatile bool IsTOnumberApproved = false;
        private bool _testItem;

        public bool FirstMarkingResetZ { get; set; }

        public FlexibleWorkFlow()
        {
            try
            {
                cfg = DCConfig.Instance;
                if (!File.Exists(cfg.ConfigName))
                {
                    RaiseErrorMsgEvent("Config file is not found! dcmarker.xml in program directory");
                }
                sig = IoSignals.Instance;
                //UpdateIoMasks();
                FirstMarkingResetZ = false;
                _currentItem = 0;
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
        }

        public void ResetArticleData()
        {
            if (_articles != null)
            {
                _articles.Clear();
            }
            _articleNumber = string.Empty;
            _laser.ResetDocument();
        }

        public void ResetArticleReady()
        {
            if (_laser != null)
            {
                _laser.ResetPort(0, sig.MASK_ARTICLEREADY);
            }
            else
            {
                Log.Debug("_laser == null");
            }
        }

        public void ResetCareful()
        {
            if (_laser != null)
            {
                _laser.ResetPort(0, sig.MASK_HANDLEWITHCARE);
            }
            else
            {
                Log.Debug("_laser == null");
            }
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

        public void Execute()
        {
            if (_laser != null)
            {
                Log.Trace("_laser.Execute");
                _laser.Execute();
            }
            else
            {
                Log.Debug("_laser == null");
            }
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
                throw;
            }

            return result;
        }

        public void SimulateItemInPlace(int seq)
        {
            throw new NotImplementedException("Not implemented in Flexible");
        }

        public void SimulateItemInPlace(string articlenumber)
        {
            //UpdateLayout();
            ArticleData data = new ArticleData();
            data.ArticleNumber = articlenumber;
            ArticleArgs e = new ArticleArgs(data);
            _articleInput_ArticleEvent(null, e);
        }

        private void _articleInput_ArticleEvent(object sender, ArticleArgs e)
        {
            RaiseErrorEvent(string.Empty);
            _laser.ResetPort(0, sig.MASK_READYTOMARK);
            _articleNumber = e.Data.ArticleNumber;
            RaiseStatusEvent(string.Format(GlblRes.Article_0_received, _articleNumber));

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

                RaiseErrorEvent(string.Format(GlblRes.Article_not_defined_in_database_Article0, _articleNumber));
            }
        }

#if DEBUG

        public void _laser_ItemInPositionEvent()
#else

        private void _laser_ItemInPositionEvent()
#endif
        {
#if !DEBUG
            if (FirstMarkingResetZ)
            {
                FirstMarkingResetZ = false;
                bool brc = ResetZAxis();
                if (!brc)
                {
                    RaiseErrorEvent(GlblRes.No_Connection_with_Z_axis);
                    return;
                }

                // We will return in _laser_ZeroReachedEvent
            }
            else
            {
                UpdateLayout();
                _currentItem = IncrementCurrentItem(_currentItem);
            }
#else

            UpdateLayout();
            _currentItem = IncrementCurrentItem(_currentItem);
#endif
        }

        private int IncrementCurrentItem(int currentItem)
        {
            int result = 0;

            currentItem++;
            if (currentItem > 1)
                currentItem = 0;

            result = currentItem;

            return result;
        }

        private void _laser_LaserEndEvent()
        {
            if (_laser != null)
            {
                FirstMarkingResetZ = false;
                _laser.SetPort(0, sig.MASK_MARKINGDONE);
                _items[_currentItem].ItemState = FlexibleItemStates.MarkingDone;
                if (_items[_currentItem].CurrentEdge >= _items[_currentItem].NumberOfEdges)
                {
                    // we are done with the item. Reset it for the next item
                    ResetItem(_currentItem);
                    RaiseItemDoneEvent(_items[_currentItem].ItemId);
                }
            }
            else
            {
                Log.Debug("_laser == null");
            }

            // TODO should this be Waiting for product? And Order done when the whole order is marked?
            //RaiseStatusEvent(GlblRes.Marking_is_done);
            RaiseStatusEvent(GlblRes.Waiting_for_product);
        }

        private void ResetItem(int currentItem)
        {
            _items[_currentItem].ItemId++;
            _items[_currentItem].CurrentEdge = 0;
            _items[_currentItem].ItemState = FlexibleItemStates.None;
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
            if (_laser != null)
            {
                _laser.ResetPort(0, sig.MASK_ALL);
            }
            else
            {
                Log.Debug("_laser == null");
            }

            RaiseErrorMsgEvent(string.Empty);
        }

        public void SetNextToLast()
        {
            // The IO signal will be set on the next External Start
            if (_laser != null)
            {
                _laser.NextToLast = true;
            }
            else
            {
                Log.Debug("_laser == null");
            }
        }

        public void ResetNextToLast()
        {
            // Reset the IO signal directly
            if (_laser != null)
            {
                _laser.NextToLast = false;
                _laser.ResetPort(0, sig.MASK_NEXTTOLAST);
            }
            else
            {
                Log.Debug("_laser == null");
            }
        }

        private List<LaserObjectData> ConvertToLaserObjectData(HistoryData historyData)
        {
            List<LaserObjectData> result;
            result = DB.ConvertHistoryDataToList(historyData);

            return result;
        }

        private HistoryData GetHistoryData(Article article, bool hasEdges)
        {
            HistoryData result = null;

            result = _db.CreateHistoryData(article, hasEdges);
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
            _laser.LaserBusyEvent += _laser_LaserBusyEvent;
            _laser.ZeroReachedEvent += _laser_ZeroReachedEvent;
        }

        private void _laser_ZeroReachedEvent(string msg)
        {
            UpdateLayout();
        }

        private void _laser_LaserBusyEvent(bool busy)
        {
            RaiseLaserBusyEvent(busy);
        }

        private void InitializeMachine()
        {
            try
            {
                CreateNewItems();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error initializing machine");
                throw;
            }
        }

        private void CreateNewItems()
        {
            _items = new FlexibleItem[2];
            _items[0] = new FlexibleItem();
            _items[1] = new FlexibleItem();
        }

        private string NormalizeLayoutName(string layoutname)
        {
            string result = string.Empty;

            if (layoutname.IndexOf(GlblRes.xlp) < 0)
            {
                layoutname += GlblRes.xlp;
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

        public void ResetAllIoSignals()
        {
            if (_laser != null)
            {
                _laser.ResetPort(0, sig.MASK_ALL);
                _laser.SetReady(false);
                RaiseLaserBusyEvent(false);
            }
        }

        private void UpdateIoMasks()
        {
            // Out
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

        /// <summary>
        /// Loads and updates the Layout when we have gotten an ItemInPlace signal from PLC
        /// </summary>
        public void UpdateLayout()
        {
            RaiseErrorEvent(string.Empty);

            Article article = GetItemArticle(_currentItem);
            if (article != null)
            {
                var item = _items[_currentItem];
                if (item.NumberOfEdges > 1 && item.CurrentEdge > 1)
                    _hasEdges = true;
                else
                    _hasEdges = false;

                ArticleData data = new ArticleData();
                data.ArticleNumber = article.F1;
                data.IsNewArticleNumber = false;
                data.TestItem = _testItem;
                data.CurrentItem = _currentItem;
                ArticleArgs e = new ArticleArgs(data);
                UpdateViewModel(item, e);

                string layoutname = article.Template;
                if (!string.IsNullOrEmpty(layoutname))
                {
                    layoutname = NormalizeLayoutName(layoutname);
                    bool brc = _laser.Load(layoutname);
                    if (brc)
                    {
                        //HistoryData historyData = GetHistoryData(_articleNumber, article.Kant, _hasEdges);
                        HistoryData historyData = GetHistoryData(article, _hasEdges);

                        if (historyData != null)
                        {
                            List<LaserObjectData> historyObjectData = ConvertToLaserObjectData(historyData);
                            brc = _laser.Update(historyObjectData);
                            if (brc)
                            {
                                // update HistoryData table
                                HistoryData status = new HistoryData();

                                // only update HistoryData table if the user has not selected the TestItem checkbox
                                // to make this if statement easier to grasp I have reversed the IsTestItemSelected so it's false when the user has selected the checkbox.
                                if (article.IsTestItemSelected.HasValue && article.IsTestItemSelected.Value)
                                {
                                    status = _db.AddHistoryDataToDB(historyData);
                                }
                                if (status != null)
                                {
                                    // we are ready to mark...
                                    RaiseStatusEvent(string.Format(GlblRes.Waiting_for_start_signal_0, layoutname));
                                    _laser.ResetPort(0, sig.MASK_MARKINGDONE);
                                    _laser.SetPort(0, sig.MASK_READYTOMARK);
                                }
                                else
                                {
                                    RaiseErrorEvent(string.Format(GlblRes.Update_didnt_work_on_this_article_and_layout_Article0_Layout1, _articleNumber, layoutname));
                                    _laser.SetPort(0, sig.MASK_ERROR);
                                }
                            }
                            else
                            {
                                RaiseErrorEvent(string.Format(GlblRes.Update_didnt_work_on_this_article_and_layout_Article0_Layout1, _articleNumber, layoutname));
                                _laser.SetPort(0, sig.MASK_ERROR);
                            }
                        }
                    }
                    else
                    {
                        RaiseErrorEvent(string.Format(GlblRes.Error_loading_layout_0, layoutname));
                        _laser.SetPort(0, sig.MASK_ERROR);
                    }
                }
                else
                {
                    RaiseErrorEvent(string.Format(GlblRes.Layout_not_defined_for_this_article_Article0, _articleNumber));
                    _laser.SetPort(0, sig.MASK_ERROR);
                }
            }
            else
            {
                RaiseErrorEvent(GlblRes.ItemInPlace_received_before_Article_Number_is_set);
            }
        }

        private Article GetItemArticle(int currentItem)
        {
            Article result = null;

            var edge = _items[currentItem].CurrentEdge;
            _items[currentItem].CurrentEdge++;
            if (_items[currentItem].CurrentEdge > _items[currentItem].NumberOfEdges)
            {
                // FIX: Change this to something better....
                RaiseErrorEvent("Trying to mark more edges than is defined");

                result = null;
            }
            else
            {
                result = _items[currentItem].Articles[edge];
            }

            return result;
        }

        public void UpdateWorkflow(Article article)
        {
            _articleNumber = article.F1;
            _articles = _db.GetArticle(_articleNumber);
            if (_articles != null)
            {
                if (TemplateExists(_articles[0].Template))
                {
                    FinishUpdateWorkflow(article);
                }
                else
                {
                    RaiseErrorEvent(string.Format("Template was not found! {0}.xlp", _articles[0].Template));
                }
            }
            else
            {
                // Couldn't find the article! Must have been deleted by someone else...
                RaiseErrorEvent(string.Format(GlblRes.Article_Number_0_not_found_in_database, _articleNumber));
            }
        }

        private bool TemplateExists(string template)
        {
            bool result = true;
            string layout = string.Format("{0}.xlp", template);
            result = _laser.Load(layout);
            return result;
        }

        private void FinishUpdateWorkflow(Article article)
        {
            if (article.Careful.HasValue && article.Careful.Value)
            {
                _laser.SetPort(0, sig.MASK_HANDLEWITHCARE);
            }
            else
            {
                _laser.ResetPort(0, sig.MASK_HANDLEWITHCARE);
            }

            _laser.SetPort(0, sig.MASK_ARTICLEREADY);

            // reverse IsTestItemSelected to make it easier for the if statement!
            for (int i = 0; i < _articles.Count; i++)
            {
                _articles[i].TOnumber = article.TOnumber;
                _articles[i].IsTestItemSelected = !article.IsTestItemSelected;
            }
            _items[0].Articles = _articles;
            _items[0].ItemState = FlexibleItemStates.ArticleLoaded;
            _items[0].NumberOfEdges = _articles.Count;
            _items[0].CurrentEdge = 0;

            _items[1].Articles = _articles;
            _items[1].ItemState = FlexibleItemStates.ArticleLoaded;
            _items[1].NumberOfEdges = _articles.Count;
            _items[1].CurrentEdge = 0;
            _currentItem = 0;
        }

        private FlexibleItem CreateFlexibleItem()
        {
            FlexibleItem result = null;

            return result;
        }

        private void UpdateViewModel(List<Article> articles)
        {
            UpdateViewModelData data = CreateUpdateViewModelData(articles);

            RaiseUpdateMainViewModelEvent(data);
        }

        private void UpdateViewModel(FlexibleItem item, ArticleArgs e)
        {
            UpdateViewModelData data = CreateUpdateViewModelData(item, e);

            RaiseUpdateMainViewModelEvent(data);
        }

        private UpdateViewModelData CreateUpdateViewModelData(List<Article> articles)
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

        private UpdateViewModelData CreateUpdateViewModelData(FlexibleItem item, ArticleArgs e)
        {
            Log.Trace("CreateUpdateViewModelData");
            var data = new UpdateViewModelData();
            Article article = item.Articles[item.CurrentEdge - 1];
            data.TotalKant = item.Articles.Count.ToString();
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

                foreach (Article a in item.Articles)
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

            data.CurrentItem = e.Data.CurrentItem;
            Log.Trace("CreateUpdateViewModelData Done");
            return data;
        }

        public void UpdateTOnumber(string tonr)
        {
            TOnumber = tonr;
            IsTOnumberUpdated = false;
        }

        #region only used by NippleWorkFlow // AME - 2018-05-12

        public void LoadArticleNumber(string _articleNumber)
        {
            throw new NotImplementedException();
        }

        public void LoadUpdateLayout()
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

        #endregion Update Error Message Event

        #region Laser Busy Event

        public delegate void LaserBusyHandler(bool busy);

        public event EventHandler<LaserBusyEventArgs> LaserBusyEvent;

        internal void RaiseLaserBusyEvent(bool busy)
        {
            var handler = LaserBusyEvent;
            if (handler != null)
            {
                var arg = new LaserBusyEventArgs(busy);
                handler(null, arg);
            }
        }

        public bool ResetZAxis()
        {
            Log.Debug("ResetZAxis");
            bool result = _laser.ResetZAxis();

            return result;
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

        #endregion Laser Busy Event

        #region Item Done Event

        public delegate void ItemDoneHandler(int numberofItemsDone);

        public event EventHandler<ItemDoneArgs> ItemDoneEvent;

        internal void RaiseItemDoneEvent(int numberofItemsDone)
        {
            var handler = ItemDoneEvent;
            if (handler != null)
            {
                var arg = new ItemDoneArgs(numberofItemsDone);
                handler(null, arg);
            }
        }

        #endregion Item Done Event
    }
}