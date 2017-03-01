using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TestTcpServer
{
    public class TextBoxValue : INotifyPropertyChanged
    {
        private string _textboxValue;

        public string TextboxValue
        {
            get
            {
                return _textboxValue;
            }
            set
            {
                if (value != _textboxValue)
                {
                    _textboxValue = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        internal void Add(string msg)
        {
            msg = "\n" + msg;
            TextboxValue += msg;
        }
    }
}
