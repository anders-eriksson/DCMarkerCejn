using DCMarkerEF;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DCAdmin.ViewModel
{
    public class LaserDataViewModel : INotifyPropertyChanged
    {
        private string _ErrorMessage;
        private string _FilterKey;
        private string _FilterValue;
        private LaserData _SelectedLaserDataRow;

        public LaserDataViewModel()
        {
            KeyCollection = DB.Instance.GetLaserDataColumns();
            LaserDataCollection = DB.Instance.LoadLaserData();
            ErrorMessage = "Hello World!";
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

        public ObservableCollection<string> KeyCollection { get; set; }
        public ObservableCollection<LaserData> LaserDataCollection { get; set; }

        public LaserData SelectedLaserDataRow
        {
            get
            {
                return _SelectedLaserDataRow;
            }
            set
            {
                //if (value == _SelectedLaserDataRow) return;
                _SelectedLaserDataRow = value;
                NotifyPropertyChanged();
            }
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
            SelectedLaserDataRow = entity;

            return entity;
        }

        internal LaserData FindSerialNumber(string articleNumber)
        {
            var entity = LaserDataCollection.FirstOrDefault(c => c.F1.StartsWith(articleNumber));
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