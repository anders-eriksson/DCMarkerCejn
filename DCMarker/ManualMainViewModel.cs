using Configuration;
using Contracts;
using DCLog;
using DCMarker.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using GlblRes = global::DCMarker.Properties.Resources;

namespace DCMarker
{
    public class ManualMainViewModel : INotifyPropertyChanged
    {
        private readonly Color BUSYCOLOR = Colors.Red;
        private readonly Color WAITINGCOLOR = Colors.LightGreen;

        private IWorkFlow _wf = null;

        private DCConfig cfg;

        public ManualMainViewModel()
        {
            ArticleNumber = string.Empty;
            Fixture = string.Empty;
            HasFixture = false;
            HasKant = false;
            Kant = string.Empty;
            HasTestItem = false;
            TestItem = string.Empty;
            HasBatchSize = false;
            BatchSize = string.Empty;
            BatchesDone = 0;
            HasTOnr = false;
            TOnr = string.Empty;
            Status = string.Empty;
            ErrorMessage = string.Empty;
            NeedUserInput = false;
            MarkingIsInProgress = false;
            IsTestItemSelected = false;
            OrderInProgress = true;
            ArticleFound = false;
            try
            {
                cfg = DCConfig.Instance;
                InitializeMachine();
                InitializeWorkflow();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        internal void Test()
        {
            //MarkingIsInProgress = !MarkingIsInProgress;

            if (_wf != null)
            {
                _wf.SimulateItemInPlace();
            }
        }

        internal void Execute()
        {
            if (_wf != null)
            {
                Log.Trace("ManuaMainViewModel: _wf.Execute");
                _wf.Execute();
            }
        }

        private void InitializeMachine()
        {
            switch (cfg.TypeOfMachine)
            {
                case 1: // PLC controlled. Kenny
                case 2: // PLC controlled with TO-number.
                    _wf = new WorkFlow();
                    break;

                case 3:
                    _wf = new ManualWorkFlow();
                    break;

                default:
                    throw new Exception(string.Format(GlblRes.Type_of_machine_not_available_Type0, cfg.TypeOfMachine));
            }
        }

        internal void ResetAllIoSignals()
        {
            if (_wf != null)
            {
                _wf.ResetAllIoSignals();
            }
        }

        private void InitializeWorkflow()
        {
            if (_wf != null)
            {
                _wf.ErrorEvent += _wf_ErrorEvent;
                _wf.UpdateMainViewModelEvent += _wf_UpdateMainViewModelEvent;
                _wf.StatusEvent += _wf_StatusEvent;
                _wf.ErrorMsgEvent += _wf_ErrorMsgEvent;
                _wf.LaserBusyEvent += _wf_LaserBusyEvent;
            }
        }

        private void _wf_LaserBusyEvent(object sender, LaserBusyEventArgs e)
        {
            MarkingIsInProgress = e.Busy;
            if (!e.Busy)
            {
                // We have got the LaserEnd event...
                BatchesDone++;

                if (!string.IsNullOrWhiteSpace(BatchSize) && BatchesDone >= Convert.ToInt32(BatchSize))
                {
                    // we are done with the order/batch.
                    ResetInputValues();
                    OrderInProgress = true;
                }
            }
        }

        private void ResetInputValues()
        {
            if (DCConfig.Instance.ResetInputValues)
            {
                ArticleNumber = string.Empty;
                HasTOnr = false;
                BatchSize = string.Empty;
                BatchesDone = 0;
                TOnr = string.Empty;
            }

            // Always reset article data so that we can't continue to mark without entering new input data!
            _wf.ResetArticleData();
        }

        private void _wf_ErrorMsgEvent(object sender, StatusArgs e)
        {
            ErrorMessage = e.Text;
        }

        private void _wf_StatusEvent(object sender, StatusArgs e)
        {
            Status = e.Text;
        }

        internal void Abort()
        {
            if (_wf != null)
            {
                _wf.Close();
            }
        }

        private void _wf_ErrorEvent(object sender, ErrorArgs e)
        {
            // Clear Status since we have an error
            Status = string.Empty;
            ErrorMessage = e.Text;
        }

        private void _wf_UpdateMainViewModelEvent(object sender, UpdateMainViewModelArgs e)
        {
            Fixture = e.Data.Fixture;
            HasFixture = string.IsNullOrWhiteSpace(Fixture) ? false : true;
            ArticleNumber = e.Data.ArticleNumber;
            Kant = e.Data.Kant;
            HasKant = string.IsNullOrWhiteSpace(Kant) ? false : true;
            HasTOnr = e.Data.HasTOnr;
            HasBatchSize = e.Data.HasBatchSize;
            NeedUserInput = e.Data.NeedUserInput;
            Status = e.Data.Status;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private ICommand _ArticleChanged;

        public ICommand ArticleChanged
        {
            get
            {
                return _ArticleChanged;
            }
            set
            {
                _ArticleChanged = value;
                NotifyPropertyChanged();
            }
        }

        private ICommand _LoadArticleCommand;

        public ICommand LoadArticleCommand
        {
            get
            {
                if (_LoadArticleCommand == null)
                {
                    _LoadArticleCommand = new RelayCommand(
                        p => this.CanLoadArticleCommandExecute(),
                        p => this.DoLoadArticleCommand());
                }
                return _LoadArticleCommand;
            }
        }

        private void DoLoadArticleCommand()
        {
            ErrorMessage = string.Empty;
            Status = string.Empty;
            if (!string.IsNullOrWhiteSpace(ArticleNumber))
            {
                List<Article> result = _wf.GetArticle(ArticleNumber);
                if (result.Count > 0)
                {
                    Fixture = result[0].FixtureId;
                    HasFixture = string.IsNullOrWhiteSpace(Fixture) ? false : true;

                    bool? enableTO = result[0].EnableTO;
                    // only have to check on the first Edge/Kant
                    HasTOnr = enableTO.HasValue ? enableTO.Value : false;
                    TOnr = string.Empty;

                    if (result.Count > 1)
                    {
                        // the article has edges/kant
                        //HasKant = true;
                        ErrorMessage = GlblRes.Edge_marking_is_not_supported_in_this_version;
                    }
                    else
                    {
                        HasKant = false;
                    }
                    ArticleFound = true;
                }
                else
                {
                    // the article doesn't exists show an error!!!
                    ErrorMessage = GlblRes.Article_does_not_exist_in_database;

                    HasTOnr = false;
                    ArticleFound = false;
                }
            }
        }

        private bool CanLoadArticleCommandExecute()
        {
            return true;
        }

        private ICommand _OkButtonCommand;

        public ICommand OkButtonCommand
        {
            get
            {
                if (_OkButtonCommand == null)
                {
                    _OkButtonCommand = new RelayCommand(
                        p => this.CanOkButtonCommandExecute(),
                        p => this.DoOkButtonCommand());
                }
                return _OkButtonCommand;
            }
        }

        private bool CanOkButtonCommandExecute()
        {
            bool result = false;
            if (HasTOnr)
            {
                result = !string.IsNullOrWhiteSpace(ArticleNumber) && !string.IsNullOrWhiteSpace(BatchSize) && TOnr.Length == DCConfig.Instance.ToNumberLength;
            }
            else
            {
                result = !string.IsNullOrWhiteSpace(ArticleNumber) && !string.IsNullOrWhiteSpace(BatchSize);
            }

            return result;
        }

        public bool ArticleFound { get; set; }

        private void DoOkButtonCommand()
        {
            int test;
            if (int.TryParse(BatchSize, out test) && ArticleFound)
            {
                ErrorMessage = string.Empty;
                Article article = new Article()
                {
                    F1 = ArticleNumber,
                    Kant = Kant,
                    FixtureId = Fixture,
                    EnableTO = HasTOnr,
                    TOnumber = TOnr,
                    Template = string.Empty,
                    IsTestItemSelected = IsTestItemSelected
                };
                BatchesDone = 0;
                //OrderInProgress = false;

                _wf.UpdateWorkflow(article);

                Status = GlblRes.Waiting_for_product;
            }
            else
            {
                ErrorMessage = "Both Article number and quantity must be entered";
            }
        }

        private bool _OrderInProgress;

        public bool OrderInProgress
        {
            get
            {
                return _OrderInProgress;
            }
            set
            {
                _OrderInProgress = value;
                NotifyPropertyChanged();
            }
        }

        private bool _MarkingIsInProgress;

        public bool MarkingIsInProgress
        {
            get
            {
                return _MarkingIsInProgress;
            }
            set
            {
                _MarkingIsInProgress = value;
                if (_MarkingIsInProgress)
                {
                    ColorStatus = BUSYCOLOR;
                }
                else
                {
                    ColorStatus = WAITINGCOLOR;
                }
                NotifyPropertyChanged();
            }
        }

        private Color _ColorStatus;

        public Color ColorStatus
        {
            get
            {
                return _ColorStatus;
            }
            set
            {
                _ColorStatus = value;
                NotifyPropertyChanged();
            }
        }

        public string ArticleNumber
        {
            get
            {
                return _articleNumber;
            }
            set
            {
                if (_articleNumber == value)
                {
                    return;
                }

                _articleNumber = value;
                DoLoadArticleCommand();
                NotifyPropertyChanged();
            }
        }

        public string BatchSize
        {
            get
            {
                return _batchSize;
            }
            set
            {
                _batchSize = value;
                NotifyPropertyChanged();
            }
        }

        private int _BatchesDone;

        public int BatchesDone
        {
            get
            {
                return _BatchesDone;
            }
            set
            {
                _BatchesDone = value;
                NotifyPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get
            {
                return _error;
            }
            set
            {
                _error = value;
                NotifyPropertyChanged();
            }
        }

        public string Fixture
        {
            get
            {
                return _fixture;
            }
            set
            {
                _fixture = value;
                NotifyPropertyChanged();
            }
        }

        public bool HasBatchSize
        {
            get
            {
                return _hasBatchSize;
            }
            set
            {
                _hasBatchSize = value;
                NotifyPropertyChanged();
            }
        }

        private bool? _IsTestItemSelected;

        public bool? IsTestItemSelected
        {
            get
            {
                return _IsTestItemSelected;
            }
            set
            {
                _IsTestItemSelected = value;
                NotifyPropertyChanged();
            }
        }

        public bool HasFixture
        {
            get
            {
                return _hasFixture;
            }
            set
            {
                _hasFixture = value;
                NotifyPropertyChanged();
            }
        }

        public bool HasKant
        {
            get
            {
                return _hasKant;
            }
            set
            {
                _hasKant = value;
                NotifyPropertyChanged();
            }
        }

        public bool HasTestItem
        {
            get
            {
                return _hasTestItem;
            }
            set
            {
                _hasTestItem = value;
                NotifyPropertyChanged();
            }
        }

        public bool HasTOnr
        {
            get
            {
                return _hasTOnr;
            }
            set
            {
                _hasTOnr = value;
                NotifyPropertyChanged();
            }
        }

        public bool NeedUserInput
        {
            get
            {
                return _needUserInput;
            }
            set
            {
                _needUserInput = value;
                NotifyPropertyChanged();
            }
        }

        public string Kant
        {
            get
            {
                return _kant;
            }
            set
            {
                _kant = value;
                NotifyPropertyChanged();
            }
        }

        public string Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                NotifyPropertyChanged();
            }
        }

        public string TestItem
        {
            get
            {
                return _testItem;
            }
            set
            {
                _testItem = value;
                NotifyPropertyChanged();
            }
        }

        public string TOnr
        {
            get
            {
                return _TOnr;
            }
            set
            {
                _TOnr = value;
                NotifyPropertyChanged();
            }
        }

        private string _articleNumber;
        private string _batchSize;
        private string _error;
        private string _fixture;
        private bool _hasBatchSize;
        private bool _hasFixture;
        private bool _hasKant;
        private bool _hasTestItem;
        private bool _hasTOnr;
        private string _kant;
        private string _status;
        private string _testItem;
        private string _TOnr;
        private bool _needUserInput;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}