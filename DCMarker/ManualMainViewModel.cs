using Configuration;
using Contracts;
using DCMarker.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using GlblRes = global::DCMarker.Properties.Resources;

namespace DCMarker
{
    public class ManualMainViewModel : INotifyPropertyChanged
    {
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
            BatchDone = 0;
            HasTOnr = false;
            TOnr = string.Empty;
            Status = string.Empty;
            ErrorMessage = string.Empty;
            NeedUserInput = false;
            MarkingIsInProgress = false;

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
            if (_wf != null)
            {
                _wf.SimulateItemInPlace();
            }
        }

        internal void Execute()
        {
            if (_wf != null)
            {
                _wf.Execute();
            }
        }

        private void InitializeMachine()
        {
            switch (cfg.TypeOfMachine)
            {
                case 1:
                    _wf = new WorkFlow();
                    break;

                case 2:
                    _wf = new ManualWorkFlow();
                    break;

                default:
                    throw new Exception(string.Format(GlblRes.Type_of_machine_not_available_Type0, cfg.TypeOfMachine));
                    // break;       // Leaving this as a reminder if we change throw to something else... 2017-02-10 AME
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
            }
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
            if (!string.IsNullOrWhiteSpace(ArticleNumber))
            {
                ErrorMessage = string.Empty;
                List<Article> result = _wf.GetArticle(ArticleNumber);
                if (result.Count > 0)
                {
                    bool? enableTO = result[0].EnableTO;
                    // only have to check on the first Edge/Kant
                    HasTOnr = enableTO.HasValue ? enableTO.Value : false;
                }
                else
                {
                    // the article doesn't exists show an error!!!
                    ErrorMessage = GlblRes.Article_does_not_exist_in_database;
                    HasTOnr = false;
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
                result = !string.IsNullOrWhiteSpace(ArticleNumber) && !string.IsNullOrWhiteSpace(BatchSize) && TOnr.Length == 7;
            }
            else
            {
                result = !string.IsNullOrWhiteSpace(ArticleNumber) && !string.IsNullOrWhiteSpace(BatchSize);
            }

            return result;
        }

        private void DoOkButtonCommand()
        {
            throw new NotImplementedException();
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