using DCMarkerEF;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DCAdmin
{
    public partial class FixtureViewModel : INotifyPropertyChanged
    {
        private Fixture _SelectedFixtureRow;

        public FixtureViewModel()
        {
            FixtureCollection = DB.Instance.LoadFixture();
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

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}