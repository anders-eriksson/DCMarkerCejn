using DCMarkerEF;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DCAdmin
{
    public partial class WeekCodeViewModel : INotifyPropertyChanged
    {
        private QuarterCode _SelectedWeekCodeRow;

        public WeekCodeViewModel()
        {
            WeekCodeCollection = DB.Instance.LoadWeekCode();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public QuarterCode SelectedWeekCodeRow
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
    }
}