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
            try
            {
                _FixtureCollection = new ObservableCollection<Fixture>();
                FixtureCollection = null;
                FixtureCollection = DB.Instance.LoadFixture();
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

        //public ObservableCollection<Fixture> FixtureCollection { get; set; }
        private ObservableCollection<Fixture> _FixtureCollection;

        public ObservableCollection<Fixture> FixtureCollection
        {
            get
            {
                return _FixtureCollection;
            }
            set
            {
                _FixtureCollection = value;
                NotifyPropertyChanged();
            }
        }

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
                ErrorMessage = string.Empty;
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
            ErrorMessage = string.Empty;
            DB.Instance.DeleteFixtureRecord(SelectedFixtureRow);
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
                ErrorMessage = string.Empty;
                if (saveChanges)
                {
                    SaveChanges();
                }

                _FixtureCollection = new ObservableCollection<Fixture>();
                FixtureCollection = null;
                FixtureCollection = DB.Instance.RefreshFixture();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}