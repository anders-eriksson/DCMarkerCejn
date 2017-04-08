using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCMarkerEF;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DCAdmin.ViewModel
{
    public class AddRowVM : INotifyPropertyChanged
    {
        public AddRowVM()
        {
            ErrorMessage = string.Empty;
            MachineId = Properties.Settings.Default.MachineId;
            Article = string.Empty;
            Kant = string.Empty;
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
            return DB.Instance.ExistsLaserData(MachineId, Article, Kant);
        }

        public string MachineId
        {
            get
            {
                return _machineId;
            }
            set
            {
                _machineId = value;
                NotifyPropertyChanged();
            }
        }

        public string Article
        {
            get
            {
                return _article;
            }
            set
            {
                _article = value;
                NotifyPropertyChanged();
            }
        }

        public string Kant
        {
            get
            {
                return _kant;
            }
            set
            {
                _kant = value;
                _kant = string.IsNullOrWhiteSpace(_kant) ? null : _kant;
                NotifyPropertyChanged();
            }
        }

        private string _machineId;
        private string _article;
        private string _kant;

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