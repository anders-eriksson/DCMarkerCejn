using DCMarkerEF;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;

namespace DCAdmin
{
    public partial class WeekCodeViewModel : INotifyPropertyChanged
    {
        private WeekCode _SelectedWeekCodeRow;

        public WeekCodeViewModel()
        {
            WeekCodeCollection = DB.Instance.LoadWeekCode();
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

        public ObservableCollection<WeekCode> WeekCodeCollection { get; set; }

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
        }

        internal void DeleteSelectedRecord()
        {
            throw new NotImplementedException();
        }
    }
}