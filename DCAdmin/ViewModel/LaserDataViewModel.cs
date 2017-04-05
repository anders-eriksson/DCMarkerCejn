#define TEST

using DCMarkerEF;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity.Validation;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DCAdmin.ViewModel
{
    public class LaserDataViewModel : INotifyPropertyChanged
    {
        private string _FilterKey;
        private string _FilterValue;
        private LaserData _SelectedLaserDataRow;

        public LaserDataViewModel()
        {
            try
            {
                KeyCollection = DB.Instance.GetLaserDataColumns();
#if TEST
                _LaserDataCollection = new ObservableCollection<LaserData>();
#endif
                LaserDataCollection = null;
                LaserDataCollection = DB.Instance.LoadLaserData();
#if DEBUG
                ErrorMessage = "Hello World!";
#else
                ErrorMessage = string.Empty;
#endif
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private string _ErrorMessage;

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
#if true
        private ObservableCollection<LaserData> _LaserDataCollection;

        public ObservableCollection<LaserData> LaserDataCollection
        {
            get
            {
                return _LaserDataCollection;
            }
            set
            {
                _LaserDataCollection = value;
                NotifyPropertyChanged();
            }
        }

#else
        public ObservableCollection<LaserData> LaserDataCollection { get; set; }
#endif

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
            ErrorMessage = string.Empty;
            var entity = FindSerialNumber(searchText);
            if (entity == null)
            {
                ErrorMessage = "Article number not found";
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

        internal void AddNewRecord()
        {
            ErrorMessage = string.Empty;
            try
            {
                var entity = DB.Instance.AddNewLaserDataRecord();
#if DEBUG
                var x = LaserDataCollection.FirstOrDefault(e => e.Id == entity.Id);
#endif
                if (entity != null)
                {
                    SelectedLaserDataRow = entity;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        internal void ExecuteFilter()
        {
            ErrorMessage = string.Empty;
            //_LaserDataCollection.Clear();
#if TEST
            _LaserDataCollection = new ObservableCollection<LaserData>();
#endif
            LaserDataCollection = DB.Instance.LoadLaserDataFiltered(FilterKey, FilterValue);
        }

        internal void ExecuteNoFilter()
        {
            ErrorMessage = string.Empty;
            //_LaserDataCollection.Clear();
#if TEST
            _LaserDataCollection = new ObservableCollection<LaserData>();
#endif
            LaserDataCollection = DB.Instance.LoadLaserData();
        }

        internal void SaveChanges()
        {
            ErrorMessage = string.Empty;
            try
            {
                DB.Instance.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                var error = ex.EntityValidationErrors.First().ValidationErrors.First();
                ErrorMessage = string.Format("Error Saving to Database: {0}", error.ErrorMessage);
            }
            catch (Exception)
            {
                throw;
            }
        }

        internal void DeleteSelectedRecord()
        {
            ErrorMessage = string.Empty;
            try
            {
                DB.Instance.DeleteLaserDataRecord(SelectedLaserDataRow);
            }
            catch (Exception)
            {
                ErrorMessage = "Can't delete selected row!";
            }
        }

        internal void RefreshDatabase(bool saveChanges = false)
        {
            ErrorMessage = string.Empty;
            try
            {
                if (saveChanges)
                {
                    SaveChanges();
                }

                _LaserDataCollection = new ObservableCollection<LaserData>();
                LaserDataCollection = null;
                LaserDataCollection = DB.Instance.RefreshLaserData();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}