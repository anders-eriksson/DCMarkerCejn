using Configuration;
using Contracts;
using DCLog;
using DCMarker.Flexible;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using GlblRes = global::DCMarker.Properties.Resources;
using System.Collections;
using System.Diagnostics;

namespace DCMarker
{
    public partial class FlexibleMainViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        private readonly Color BUSYCOLOR = Colors.Red;
        private readonly Color WAITINGCOLOR = Colors.LightGreen;

        private IWorkFlow _wf = null;

        private DCConfig cfg;

        public FlexibleMainViewModel()
        {
            ArticleNumber = string.Empty;
            Fixture = string.Empty;
            HasFixture = false;
            HasKant = false;
            Kant = string.Empty;
            HasTestItem = false;
            TestItem = string.Empty;
            HasBatchSize = false;
            Quantity = string.Empty;
            BatchesDone = 0;
            HasTOnr = false;
            TOnr = string.Empty;
            Status = string.Empty;
            ErrorMessage = string.Empty;
            NeedUserInput = false;
            MarkingIsInProgress = false;
            IsTestItemSelected = false;
            OrderInProgress = true;
            OrderNotStarted = true;
            ArticleFound = false;
            try
            {
                cfg = DCConfig.Instance;

                InitializeMachine();
                InitializeWorkflow();
                if (DCConfig.Instance.KeepQuantity)
                {
                    Quantity = Properties.Settings.Default.Quantity;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        internal void Test()
        {
            //MarkingIsInProgress = !MarkingIsInProgress;
#if DEBUG
            if (_wf != null)
            {
                _wf._laser_ItemInPositionEvent();
                //_wf.SimulateItemInPlace(ArticleNumber);
            }
#endif
        }

#if DEBUG

        internal void Execute()
        {
            if (_wf != null)
            {
                Log.Trace("FlexibleMainViewModel: _wf.Execute");
                _wf.Execute();
            }
        }

#endif

        private void InitializeMachine()
        {
            _wf = new FlexibleWorkFlow();
            _wf.ItemDoneEvent += _wf_ItemDoneEvent;
        }

        private void _wf_ItemDoneEvent(object sender, ItemDoneArgs e)
        {
            ItemDone = e.NumberofItemsDone.ToString();
        }

        internal void ResetAllIoSignals()
        {
            if (_wf != null)
            {
                _wf.ResetAllIoSignals();
            }
        }

        internal bool ResetZAxis()
        {
            bool result = false;

            if (_wf != null)
            {
                result = _wf.ResetZAxis();
            }

            return result;
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

                if (!string.IsNullOrWhiteSpace(Quantity))
                {
                    // Check if the next marking is the second to last
                    if (BatchesDone == Convert.ToInt32(Quantity) - 2)
                    {
                        Log.Debug(string.Format("Currently marking the second to last. Batches Done:{0} - Quantity: {1}", BatchesDone, Quantity));
                        _wf.SetNextToLast();
                    }
                    if (BatchesDone >= Convert.ToInt32(Quantity))
                    {
                        // we are done with the order/batch.
                        _wf.ResetArticleReady();
                        _wf.ResetNextToLast();
                        ResetInputValues();
                        Log.Debug(GlblRes.OrderBatch_is_done);
                        OrderNotStarted = true;
                        OrderInProgress = true;
                        Status = GlblRes.Order_is_done;
                    }
                }
            }
        }

        private void ResetInputValues()
        {
            if (DCConfig.Instance.ResetInputValues)
            {
                ArticleNumber = string.Empty;
                HasFixture = false;
                HasTOnr = false;
                Quantity = string.Empty;
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
            TableSide = TableName[e.Data.CurrentItem];
            CurrentSide = e.Data.CurrentItem;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private string[] TableName = new string[2] { "A", "B" };
        private string _TableSide;

        public string TableSide
        {
            get
            {
                return _TableSide;
            }
            set
            {
                _TableSide = value;
                NotifyPropertyChanged();
            }
        }

        private string _ItemDone;

        public string ItemDone
        {
            get
            {
                return _ItemDone;
            }
            set
            {
                _ItemDone = value;
                NotifyPropertyChanged();
            }
        }

        private int _CurrentSide;

        public int CurrentSide
        {
            get
            {
                return _CurrentSide;
            }
            set
            {
                _CurrentSide = value;
                NotifyPropertyChanged();
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

        private bool _OrderNotStarted;

        public bool OrderNotStarted
        {
            get
            {
                return _OrderNotStarted;
            }
            set
            {
                _OrderNotStarted = value;
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

        public string Quantity
        {
            get
            {
                return _quantity;
            }
            set
            {
                _quantity = value;
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

                RaiseFocusEventEvent("TO-Number");
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

        private bool _IsCareful;

        public bool IsCareful
        {
            get
            {
                return _IsCareful;
            }
            set
            {
                _IsCareful = value;
                NotifyPropertyChanged();
            }
        }

        #region IDataErrorInfo

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public string Error
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string this[string propertyName]
        {
            get { var error = GetValidationError(propertyName); return error; }
        }

        private static readonly string[] ValidatedProperties = { "ArticleNumber", "TOnr" };

        /// <summary>
        /// Contains the valid state of the Validated Properties.
        /// </summary>
        private static bool[] ValidatedPropertiesState = new bool[ValidatedProperties.Length];

        public bool IsValid
        {
            get
            {
                foreach (bool state in ValidatedPropertiesState)
                {
                    if (!state)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private string GetValidationError(string propertyName)
        {
            string result = null;
            switch (propertyName)
            {
                case nameof(ArticleNumber):
                    {
                        result = ValidateArticleNumber();
                        break;
                    }
                case nameof(TOnr):
                    {
                        result = ValidateToNumber();
                        break;
                    }
                case nameof(Quantity):
                    {
                        result = ValidateQuantity();
                        break;
                    }
            };
            return result;
        }

        private string ValidateArticleNumber()
        {
            string result = null;
            ValidatedPropertiesState[GetIndex("ArticleNumber")] = false;

            if (string.IsNullOrWhiteSpace(ArticleNumber))
            {
                result = "!"; // string.Empty; // "Article Number is required";
            }
            else
            {
                result = DoLoadArticleCommand();
                if (result == null)
                {
                    ValidatedPropertiesState[GetIndex("ArticleNumber")] = true;
                }
            }

            return result;
        }

        private string ValidateQuantity()
        {
            string result = null;
            ValidatedPropertiesState[GetIndex("Quantity")] = false;
            if (string.IsNullOrWhiteSpace(_quantity))
            {
                result = "!";// string.Empty; // "Quantity is required";
            }
            else
            {
                int quantity;
                bool brc = int.TryParse(_quantity, out quantity);
                if (!brc)
                {
                    result = GlblRes.Not_a_valid_number;
                }
                else if (quantity < 1)
                {
                    result = GlblRes.Quantity_must_be_1_or_larger;
                }
                else
                {
                    ValidatedPropertiesState[GetIndex("Quantity")] = true;
                }
            }

            return result;
        }

        private static int GetIndex(string v)
        {
            return Array.IndexOf(ValidatedProperties, v);
        }

        private string ValidateToNumber()
        {
            string result = null;

            if (HasTOnr)
            {
                ValidatedPropertiesState[GetIndex("TOnr")] = false;

                if (string.IsNullOrWhiteSpace(TOnr))
                {
                    result = "!"; // string.Empty; // "TO Number is required";
                }
                else if (TOnr.Length != DCConfig.Instance.ToNumberLength)
                {
                    result = string.Format(GlblRes.TO_Number_must_be_0_characters, DCConfig.Instance.ToNumberLength);
                }
                else
                {
                    ValidatedPropertiesState[GetIndex("TOnr")] = true;
                }
            }
            else
            {
                ValidatedPropertiesState[GetIndex("TOnr")] = true;
            }

            return result;
        }

        #region Focus Event Event

        public delegate void FocusEventHandler(string msg);

        public event EventHandler<FocusEventArgs> FocusEvent;

        internal void RaiseFocusEventEvent(string msg)
        {
            var handler = FocusEvent;
            if (handler != null)
            {
                var arg = new FocusEventArgs(msg);
                handler(null, arg);
            }
        }

        #endregion Focus Event Event

        #endregion IDataErrorInfo

        private string _articleNumber;
        private string _quantity;
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