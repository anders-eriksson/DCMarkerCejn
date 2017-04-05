using DCMarkerEF;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;
using System.Data.Entity.Validation;
using System.Linq;

namespace DCAdmin
{
    public partial class QuarterCodeViewModel : INotifyPropertyChanged
    {
        private QuarterCode _SelectedQuarterCodeRow;

        public QuarterCodeViewModel()
        {
            try
            {
                _QuarterCodeCollection = new ObservableCollection<QuarterCode>();
                QuarterCodeCollection = null;
                QuarterCodeCollection = DB.Instance.LoadQuarterCode();
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

        //public ObservableCollection<QuarterCode> QuarterCodeCollection { get; set; }
        private ObservableCollection<QuarterCode> _QuarterCodeCollection;

        public ObservableCollection<QuarterCode> QuarterCodeCollection
        {
            get
            {
                return _QuarterCodeCollection;
            }
            set
            {
                _QuarterCodeCollection = value;
                NotifyPropertyChanged();
            }
        }

        public QuarterCode SelectedQuarterCodeRow
        {
            get
            {
                return _SelectedQuarterCodeRow;
            }
            set
            {
                if (value == _SelectedQuarterCodeRow) return;
                _SelectedQuarterCodeRow = value;
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
            try
            {
                var entity = DB.Instance.AddNewQuartalCodeRecord();
                if (entity != null)
                {
#if DEBUG
                    var x = QuarterCodeCollection.FirstOrDefault(e => e.QYear == entity.QYear);
#endif
                    SelectedQuarterCodeRow = entity;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        internal void DeleteSelectedRecord()
        {
            DB.Instance.DeleteQuarterCodeRecord(SelectedQuarterCodeRow);
        }

        internal void SaveChanges()
        {
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

        internal void RefreshDatabase(bool saveChanges)
        {
            try
            {
                if (saveChanges)
                {
                    SaveChanges();
                }

                _QuarterCodeCollection = new ObservableCollection<QuarterCode>();
                QuarterCodeCollection = null;
                QuarterCodeCollection = DB.Instance.RefreshQuarterCode();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}