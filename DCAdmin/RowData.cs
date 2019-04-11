using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DCMarkerEF;

namespace DCAdmin
{
    public class RowData : INotifyPropertyChanged
    {
        public RowData()
        {
            MachineCode = Properties.Settings.Default.MachineCode;
            Article = string.Empty;
            Kant = string.Empty;
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
                NotifyPropertyChanged();
            }
        }

        public bool Exists(string machineCode, string article, string kant)
        {
            return DB.Instance.ExistsLaserData(machineCode, article, kant);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string _machineCode;
        private string _article;
        private string _kant;
    }
}