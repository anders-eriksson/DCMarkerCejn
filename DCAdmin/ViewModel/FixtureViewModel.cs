using DCMarkerEF;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;
using System.Data.Entity.Validation;
using System.Linq;

namespace DCAdmin
{
    public partial class FixtureViewModel : INotifyPropertyChanged
    {
        private string _ErrorMessage;
        private Fixture _SelectedFixtureRow;

        public FixtureViewModel()
        {
            FixtureCollection = DB.Instance.LoadFixture();
#if DEBUG
            ErrorMessage = "Hello World!";
#endif
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Fixture> FixtureCollection { get; set; }

        public Fixture SelectedFixtureRow
        {
            get
            {
                return _SelectedFixtureRow;
            }
            set
            {
                if (value == _SelectedFixtureRow) return;
                _SelectedFixtureRow = value;
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
                var entity = DB.Instance.AddNewFixtureRecord();
                if (entity != null)
                {
                    SelectedFixtureRow = entity;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        internal void DeleteSelectedRecord()
        {
            DB.Instance.DeleteFixtureRecord(SelectedFixtureRow);
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
    }
}