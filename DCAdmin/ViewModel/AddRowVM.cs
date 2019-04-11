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
            MachineCode = Properties.Settings.Default.MachineCode;
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
            return DB.Instance.ExistsLaserData(MachineCode, Article, Kant);
        }

        public string MachineCode
        {
            get
            {
                return _machineCode;
            }
            set
            {
                _machineCode = value;
                _machineCode = string.IsNullOrWhiteSpace(_machineCode) ? null : _machineCode;
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

        private string _machineCode;
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