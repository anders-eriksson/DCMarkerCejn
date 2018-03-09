using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DCAdmin.ViewModel
{
    public class AddFixtureVM : INotifyPropertyChanged
    {
        public AddFixtureVM()
        {
            FixtureId = string.Empty;
        }

        private string _Fixture;

        public string FixtureId
        {
            get { return _Fixture; }
            set
            {
                _Fixture = value;
                NotifyPropertyChanged();
            }
        }

        private string _errorMessage;

        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set
            {
                _errorMessage = value;
                NotifyPropertyChanged();
            }
        }

        public bool RowExists()
        {
            return DB.Instance.ExistsFixture(FixtureId);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}