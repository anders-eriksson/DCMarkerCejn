using Configuration;
using Contracts;
using DCMarker.Model;
using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Runtime.CompilerServices;
using GlblRes = global::DCMarker.Properties.Resources;
using System.Timers;

namespace DCMarker
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class NippleMainViewModel : INotifyPropertyChanged
    {
        private IWorkFlow _wf = null;

        private DCConfig cfg;

        private System.Timers.Timer _pollTimer;
        //private bool KeepTOnr;

        // Destructor
        ~NippleMainViewModel()
        {
            if (_pollTimer != null)
            {
                _pollTimer.Dispose();
            }
        }

        public NippleMainViewModel()
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
            BatchDone = 0;
            HasTOnr = false;
            TOnr = string.Empty;
            Status = string.Empty;
            ErrorMessage = string.Empty;
            NeedUserInput = false;
            IsTestItemSelected = false;
            //KeepTOnr = false;

            try
            {
                cfg = DCConfig.Instance;
                InitializeMachine();
                InitializeWorkflow();
                //LoadLastArticleNumber();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        private int seq = 1;

        internal void Test()
        {
            if (_wf != null)
            {
                _wf.SimulateItemInPlace(seq++);
            }
        }

        internal void Execute()
        {
            seq = 1;
            //if (_wf != null)
            //{
            //    _wf.Execute();
            //}
        }

        private void InitializeMachine()
        {
            try
            {
                switch (cfg.TypeOfMachine)
                {
                    case 1:
                        _wf = new WorkFlow();
                        break;

                    case 2:
                        // TODO change this to WorkFlowWithTOnr()
                        _wf = new WorkFlow();
                        break;

                    case 3:
                        _wf = new ManualWorkFlow();
                        break;

                    case 4:
                        _wf = new NippleWorkFlow();
                        break;

                    default:
                        throw new Exception(string.Format(GlblRes.Type_of_machine_not_available_Type0, cfg.TypeOfMachine));
                        // break;       // Leaving this as a reminder if we change throw to something else... 2017-02-10 AME
                }
            }
            catch (Exception ex)
            {
                throw;
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
                _wf.ArticleHasToNumberEvent += _wf_ArticleHasToNumberEvent;
                try
                {
                    _pollTimer = new System.Timers.Timer();
                    _pollTimer.Interval = 1000;
                    _pollTimer.Elapsed += StartPoll;
                    _pollTimer.Start();
                }
                catch (Exception)
                {
                    ErrorMessage = GlblRes.Cant_start_timer_for_polling_ADAM_Module;
                    throw;
                }
            }
        }

        private void _wf_ArticleHasToNumberEvent(object sender, ArticleHasToNumberArgs e)
        {
            ArticleHasTOnumber = e.State;
        }

        private void StartPoll(object sender, ElapsedEventArgs e)
        {
            _pollTimer.Stop();
            _wf.StartPoll(DCConfig.Instance.AdamPollInterval, DCConfig.Instance.AdamErrorTimeout);
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
                SaveCurrentArticleNumber();
                _wf.Close();
            }
        }

        private void SaveCurrentArticleNumber()
        {
            Properties.Settings.Default.ArticleNumber = ArticleNumber;
            Properties.Settings.Default.Save();
        }

        private void LoadLastArticleNumber()
        {
            if (_wf != null)
            {
                ArticleNumber = Properties.Settings.Default.ArticleNumber;
                if (!string.IsNullOrWhiteSpace(_articleNumber))
                {
                    _wf.LoadArticleNumber(_articleNumber);
                }
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
            IsNewArticle = e.Data.IsNewArticleNumber;
            ArticleNumber = e.Data.ArticleNumber;
            Kant = e.Data.Kant;
            TotalKant = e.Data.TotalKant;
            IsTestItemSelected = e.Data.Provbit;
            HasKant = string.IsNullOrWhiteSpace(Kant) ? false : true;
            HasTOnr = e.Data.HasTOnr;
            HasBatchSize = e.Data.HasBatchSize;
            NeedUserInput = e.Data.NeedUserInput;
            Status = e.Data.Status;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string ArticleNumber
        {
            get
            {
                return _articleNumber;
            }
            set
            {
                if (IsNewArticle)
                {
                    //if (_articleNumber == value)
                    //{
                    //    KeepTOnr = true;
                    //}
                    //else
                    //{
                    //    KeepTOnr = false;
                    //    TOnr = string.Empty;
                    //}
                    TOnr = string.Empty;
                    _articleNumber = value;
                    NotifyPropertyChanged();
                    ActivateTO();
                    // TODO: maybe i need to wait a bit before doing this. So that WPF has updated the textbox
                    System.Threading.Thread.Sleep(100);
                    RaiseSetFocusToNumberEvent(true);
                }
            }
        }

        private bool _IsTOButtonActive;

        public bool IsTOButtonActive
        {
            get
            {
                return _IsTOButtonActive;
            }
            set
            {
                _IsTOButtonActive = value;
                NotifyPropertyChanged();
            }
        }

        private bool _IsTOnumber;

        public bool IsTOnumber
        {
            get
            {
                return _IsTOnumber;
            }
            set
            {
                _IsTOnumber = value;
                if (_IsTOnumber)
                {
                    TOnr = string.Empty;
                }
                NotifyPropertyChanged();
            }
        }

        private bool _IsNewArticle;

        public bool IsNewArticle
        {
            get
            {
                return _IsNewArticle;
            }
            set
            {
                _IsNewArticle = value;

                NotifyPropertyChanged();
                //ActivateTO();
            }
        }

        private void ActivateTO()
        {
            if (IsNewArticle)
            {
                if (string.IsNullOrWhiteSpace(TOnr))
                {
                    IsTOButtonActive = true;
                    IsTOnumber = true;
                }
                else
                {
                    IsTOButtonActive = false;
                    IsTOnumber = false;
                }
            }
            else
            {
                IsTOButtonActive = false;
                IsTOnumber = false;
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

        private int _BatchDone;

        public int BatchDone
        {
            get
            {
                return _BatchDone;
            }
            set
            {
                _BatchDone = value;
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

        private bool _HasTOnumber;

        public bool ArticleHasTOnumber
        {
            get
            {
                return _HasTOnumber;
            }
            set
            {
                _HasTOnumber = value;
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

        private string _TotalKant;

        public string TotalKant
        {
            get
            {
                return _TotalKant;
            }
            set
            {
                _TotalKant = value;
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

        #region Commands

        private ICommand _UpdateToNumberCommand;

        public ICommand UpdateToNumberCommand
        {
            get
            {
                if (_UpdateToNumberCommand == null)
                {
                    _UpdateToNumberCommand = new RelayCommand(
                        p => this.CanUpdateToNumberCommandExecute(),
                        p => this.DoUpdateToNumberCommand());
                }
                return _UpdateToNumberCommand;
            }
        }

        private void DoUpdateToNumberCommand()
        {
            if (string.IsNullOrWhiteSpace(TOnr))
            {
                ErrorMessage = GlblRes.Production_needs_a_value;
                RaiseSetFocusToNumberEvent(true);
                return;
            }
            try
            {
                if (_wf != null)
                {
                    _wf.UpdateTOnumber(TOnr);
                }
                IsNewArticle = false;
                IsTOnumber = false;
                IsTOButtonActive = false;
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                DCLog.Log.Error(ex, ex.Message);
                ErrorMessage = ex.Message;
            }
        }

        private bool CanUpdateToNumberCommandExecute()
        {
            return HasTOnr;
        }

        #endregion Commands

        #region Set Focus to TO-Number Event

        public delegate void SetFocusToNumberHandler(bool mode);

        public event EventHandler<SetFocusToNumberArgs> SetFocusToNumberEvent;

        internal void RaiseSetFocusToNumberEvent(bool mode)
        {
            var handler = SetFocusToNumberEvent;
            if (handler != null)
            {
                var arg = new SetFocusToNumberArgs(mode);
                handler(null, arg);
            }
        }

        #endregion Set Focus to TO-Number Event
    }
}