using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace ValidationToolkit
{
    public abstract class ViewModel : ValidationErrorContainer, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Called internally to notify the view that that a property has changed.
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
