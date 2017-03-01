using DCMarkerEF;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DCAdmin
{
    public partial class QuarterCodeViewModel : INotifyPropertyChanged
    {
        private QuarterCode _SelectedQuarterCodeRow;

        public QuarterCodeViewModel()
        {
            //_QuarterCodeCollection = new ObservableCollection<QuarterCode>();
            QuarterCodeCollection = DB.Instance.LoadQuarterCode();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<QuarterCode> QuarterCodeCollection { get; set; }

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

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}