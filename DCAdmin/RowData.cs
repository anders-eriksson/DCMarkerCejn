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
            MachineId = Properties.Settings.Default.MachineId;
            Article = string.Empty;
            Kant = string.Empty;
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
                NotifyPropertyChanged();
            }
        }

        public bool Exists(string machineId, string article, string kant)
        {
            return DB.Instance.ExistsLaserData(machineId, article, kant);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string _machineId;
        private string _article;
        private string _kant;
    }
}