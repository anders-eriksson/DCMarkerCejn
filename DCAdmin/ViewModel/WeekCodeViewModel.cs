using DCMarkerEF;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity.Validation;
using System.Linq;
using System.Runtime.CompilerServices;
using GlblRes = global::DCAdmin.Properties.Resources;

namespace DCAdmin
{
    public partial class WeekCodeViewModel : INotifyPropertyChanged
    {
        private WeekCode _SelectedWeekCodeRow;

        public WeekCodeViewModel()
        {
            try
            {
                _WeekCodeCollection = new ObservableCollection<WeekCode>();
                WeekCodeCollection = null;
                WeekCodeCollection = DB.Instance.LoadWeekCode();
#if DEBUG
                ErrorMessage = "Hello World!";
#endif
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public WeekCode SelectedWeekCodeRow
        {
            get
            {
                return _SelectedWeekCodeRow;
            }
            set
            {
                if (value == _SelectedWeekCodeRow) return;
                _SelectedWeekCodeRow = value;
                NotifyPropertyChanged();
            }
        }

        // public ObservableCollection<WeekCode> WeekCodeCollection { get; set; }
        private ObservableCollection<WeekCode> _WeekCodeCollection;

        public ObservableCollection<WeekCode> WeekCodeCollection
        {
            get
            {
                return _WeekCodeCollection; ;
            }
            set
            {
                _WeekCodeCollection = value;
                NotifyPropertyChanged();
            }
        }

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

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        internal void AddNewRecord()
        {
            ErrorMessage = GlblRes.Adding_a_row_is_not_allowed;
#if NOT_ALLOWED
            try
            {
                var entity = DB.Instance.AddNewWeekCodeRecord();
                if (entity != null)
                {
                    SelectedWeekCodeRow = entity;
                }
            }
            catch (Exception)
            {
                throw;
            }
#endif
        }

        internal void DeleteSelectedRecord()
        {
            ErrorMessage = GlblRes.Deleting_a_row_is_not_allowed;
#if NOT_ALLOWED
            DB.Instance.DeleteWeekCodeRecord(SelectedWeekCodeRow);
#endif
        }

        internal void SaveChanges()
        {
            try
            {
                ErrorMessage = string.Empty;
                DB.Instance.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                var error = ex.EntityValidationErrors.First().ValidationErrors.First();
                ErrorMessage = string.Format(GlblRes.Error_Saving_to_Database_0, error.ErrorMessage);
            }
            catch (Exception)
            {
                throw;
            }
        }

        internal void RefreshDatabase(bool saveChanges)
        {
            try
            {
                ErrorMessage = string.Empty;
                if (saveChanges)
                {
                    SaveChanges();
                }

                _WeekCodeCollection = new ObservableCollection<WeekCode>();
                WeekCodeCollection = null;
                WeekCodeCollection = DB.Instance.RefreshWeekCode();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}