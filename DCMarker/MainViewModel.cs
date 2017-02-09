using DCMarker.Model;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace DCMarker
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private WorkFlow _wf;
        private Thread _workflowThread;

        public MainViewModel()
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
            HasTOnr = false;
            TOnr = string.Empty;
            Status = string.Empty;
            Error = string.Empty;

            _workflowThread = new Thread(ExecuteWorkflow);
            _workflowThread.Start();
        }

        private void ExecuteWorkflow()
        {
            _wf = new WorkFlow();
            _wf.ErrorEvent += _wf_ErrorEvent;
            _wf.UpdateMainViewModelEvent += _wf_UpdateMainViewModelEvent;
            _wf.StatusEvent += _wf_StatusEvent;
        }

        private void _wf_StatusEvent(string msg)
        {
            Status = msg;
        }

        internal void Abort()
        {
            _wf.Abort();
        }

        private void _wf_ErrorEvent(string msg)
        {
            Error = msg;
        }

        private void _wf_UpdateMainViewModelEvent(UpdateViewModelData data)
        {
            ArticleNumber = data.ArticleNumber;
            Kant = data.Kant;
            if (string.IsNullOrWhiteSpace(Kant))
            {
                HasKant = false;
            }
            else
            {
                HasKant = true;
            }
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
                _articleNumber = value;
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

        public string Error
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