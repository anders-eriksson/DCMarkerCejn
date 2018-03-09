using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using GlblRes = global::MyTests.Properties.Resources;

namespace MyTests
{
    public class ManualMainViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        private readonly Color BUSYCOLOR = Colors.Red;
        private readonly Color WAITINGCOLOR = Colors.LightGreen;

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
            Quantity = string.Empty;
            BatchesDone = 0;
            HasTOnr = true;
            TOnr = string.Empty;
            Status = string.Empty;
            ErrorMessage = string.Empty;
            NeedUserInput = false;
            MarkingIsInProgress = false;
            IsTestItemSelected = false;
            //OrderInProgress = true;
            ArticleFound = false;
            try
            {
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

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

        public bool ArticleFound { get; set; }

        //private bool _OrderInProgress;

        //public bool OrderInProgress
        //{
        //    get
        //    {
        //        return _OrderInProgress;
        //    }
        //    set
        //    {
        //        _OrderInProgress = value;
        //        NotifyPropertyChanged();
        //    }
        //}

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

        #region IDataErrorInfo

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

        private static readonly string[] ValidatedProperties = { "ArticleNumber", "TOnr", "Quantity" };

        public bool IsValid
        {
            get
            {
                foreach (string property in ValidatedProperties)
                {
                    if (GetValidationError(property) != null)
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

            if (string.IsNullOrWhiteSpace(ArticleNumber))
            {
                result = "!"; // string.Empty; // "Article Number is required";
            }
            else
            {
                result = DoLoadArticleCommand();
            }

            return result;
        }

        private string DoLoadArticleCommand()
        {
            return "Hello World";
        }

        private string ValidateQuantity()
        {
            string result = null;

            if (string.IsNullOrWhiteSpace(Quantity))
            {
                result = "!";// string.Empty; // "Quantity is required";
            }
            else
            {
                int quantity;
                bool brc = int.TryParse(Quantity, out quantity);
                if (!brc)
                {
                    result = GlblRes.Not_a_valid_number;
                }
                else if (quantity < 1)
                {
                    result = GlblRes.Quantity_must_be_1_or_larger;
                }
            }

            return result;
        }

        private string ValidateToNumber()
        {
            string result = null;

            if (HasTOnr)
            {
                if (string.IsNullOrWhiteSpace(TOnr))
                {
                    result = "!"; // string.Empty; // "TO Number is required";
                }
                else if (TOnr.Length != 7)
                {
                    result = string.Format(GlblRes.TO_Number_must_be_0_characters, 7);
                }
            }
            return result;
        }

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