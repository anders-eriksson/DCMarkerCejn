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
            MaskinID = Properties.Settings.Default.MaskinID;
            Article = string.Empty;
            Kant = string.Empty;
        }

        public string MaskinID
        {
            get
            {
                return _maskinID;
            }
            set
            {
                _maskinID = value;
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

        public bool Exists(string maskinID, string article, string kant)
        {
            return DB.Instance.ExistsLaserData(maskinID, article, kant);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string _maskinID;
        private string _article;
        private string _kant;
    }
}