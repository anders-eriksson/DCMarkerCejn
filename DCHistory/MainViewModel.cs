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
        private ObservableCollection<HistoryData> _HistoryCollection;

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
                NotifyPropertyChanged();
            }
        }

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

        public MainViewModel()
        {
            _HistoryCollection = new ObservableCollection<HistoryData>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        internal void GetAllHistoryData()
        {
            try
            {
                HistoryCollection.Clear();
                SelectedRow = null;
                HistoryCollection = DB.GetAllHistory();
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

        internal void GetDateFilteredHistory(DateTime start, DateTime end)
        {
            HistoryCollection.Clear();
            SelectedRow = null;
            HistoryCollection = DB.GetDateFilteredHistory(start, end);
        }

        internal Model.HistoryData FindSerialNumber(string serialNumber)
        {
            var entity = HistoryCollection.FirstOrDefault(c => c.Snr == serialNumber);
            return entity;
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

        private string _ErrorMessage;
        private HistoryData _SelectedRow;
    }
}