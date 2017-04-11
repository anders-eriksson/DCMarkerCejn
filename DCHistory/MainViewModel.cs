using DCHistory.Model;
using DCLog;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DCHistory
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _ErrorMessage;

        private string _FilterKey;

        private string _FilterValue;

        private FilterType _HasFilterType;

        private ObservableCollection<HistoryData> _HistoryCollection;

        private bool _IsFilterDataPicker;

        private bool _IsFilterTextbox;

        private HistoryData _SelectedRow;

        public MainViewModel()
        {
            _HistoryCollection = new ObservableCollection<HistoryData>();
            KeyCollection = DB.GetHistoryDataColumns();

            // enable Filter Textbox
            IsFilterTextbox = true;

            // disable Filter DataPicker
            IsFilterDataPicker = false;
            // disable Filter checkbox
            IsFilterBool = false;
            // Set init value for checkbox to false
            IsFilterBoolChecked = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string ErrorMessage
        {
            get
            {
                return _ErrorMessage;
            }
            set
            {
                _ErrorMessage = value;
                NotifyPropertyChanged();
            }
        }

        public string FilterKey
        {
            get
            {
                return _FilterKey;
            }
            set
            {
                _FilterKey = value;
                NotifyPropertyChanged();
            }
        }

        public string FilterValue
        {
            get
            {
                return _FilterValue;
            }
            set
            {
                _FilterValue = value;
                NotifyPropertyChanged();
            }
        }

        public FilterType HasFilterType
        {
            get
            {
                return _HasFilterType;
            }
            set
            {
                _HasFilterType = value;
                SetFilterType(_HasFilterType);
                NotifyPropertyChanged();
            }
        }

        private void SetFilterType(FilterType hasFilterType)
        {
            IsFilterBool = false;
            IsFilterDataPicker = false;
            IsFilterTextbox = false;

            if (hasFilterType == FilterType.Bool)
            {
                IsFilterBool = true;
            }
            else if (hasFilterType == FilterType.Date)
            {
                IsFilterDataPicker = true;
            }
            else if (hasFilterType == FilterType.Text)
            {
                IsFilterTextbox = true;
            }
        }

        public ObservableCollection<HistoryData> HistoryCollection
        {
            get
            {
                return _HistoryCollection;
            }
            set
            {
                if (value == _HistoryCollection) return;

                _HistoryCollection = value;
                RowCount = _HistoryCollection != null ? _HistoryCollection.Count.ToString() : string.Empty;
                NotifyPropertyChanged();
            }
        }

        private bool _IsFilterBool;

        public bool IsFilterBool
        {
            get
            {
                return _IsFilterBool;
            }
            set
            {
                _IsFilterBool = value;
                NotifyPropertyChanged();
            }
        }

        private bool? _IsFilterBoolChecked;

        public bool? IsFilterBoolChecked
        {
            get
            {
                return _IsFilterBoolChecked;
            }
            set
            {
                _IsFilterBoolChecked = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsFilterDataPicker
        {
            get
            {
                return _IsFilterDataPicker;
            }
            set
            {
                _IsFilterDataPicker = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsFilterTextbox
        {
            get
            {
                return _IsFilterTextbox;
            }
            set
            {
                _IsFilterTextbox = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<string> KeyCollection { get; set; }

        public HistoryData SelectedRow
        {
            get
            {
                return _SelectedRow;
            }
            set
            {
                if (value == _SelectedRow) return;
                _SelectedRow = value;
                NotifyPropertyChanged();
            }
        }

        private string _RowCount;

        public string RowCount
        {
            get
            {
                return _RowCount;
            }
            set
            {
                _RowCount = value;
                NotifyPropertyChanged();
            }
        }

        internal void ExecuteFilter()
        {
            ErrorMessage = string.Empty;
            HistoryCollection = null;
            GC.Collect();

            HistoryCollection = DB.GetFilteredTextHistory(FilterKey, FilterValue);
        }

        internal void ExecuteFilterDate(DateTime start, DateTime end)
        {
            HistoryCollection = null;
            GC.Collect();
            SelectedRow = null;
            HistoryCollection = DB.GetDateFilteredHistory(start, end);
        }

        internal void GetAllHistoryData()
        {
            try
            {
                HistoryCollection = null;
                GC.Collect();

                SelectedRow = null;
                HistoryCollection = DB.GetAllHistory();
            }
            catch (OutOfMemoryException)
            {
                ErrorMessage = "Out of memory! Please use a filter to make selection smaller!";
            }
            catch (SqlException ex)
            {
                Log.Fatal(ex, ex.Message);
                ErrorMessage = string.Format("Database Error: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        internal void ExecuteFilterBool()
        {
            ErrorMessage = string.Empty;
            HistoryCollection = null;
            GC.Collect();

            HistoryCollection = DB.GetFilteredBoolHistory(FilterKey, IsFilterBoolChecked.Value);
        }

        internal void ExecuteNoFilter()
        {
            ErrorMessage = string.Empty;
            HistoryCollection = null;
            GC.Collect();
            HistoryCollection = DB.GetAllHistory();
        }

        internal object FindArticleAndScrollIntoView(string searchText)
        {
            var entity = FindSerialNumber(searchText);
            if (entity == null)
            {
                ErrorMessage = "Serial number not found";
            }
            else
            {
                ErrorMessage = string.Empty;
            }
            SelectedRow = entity;

            return entity;
        }

        internal Model.HistoryData FindSerialNumber(string serialNumber)
        {
            var entity = HistoryCollection.FirstOrDefault(c => c.Snr == serialNumber);
            return entity;
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}