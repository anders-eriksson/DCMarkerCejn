using DCMarkerEF;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity.Validation;
using System.Linq;
using System.Runtime.CompilerServices;
using GlblRes = global::DCAdmin.Properties.Resources;
using DCLog;
using System.Data.Entity.Core;
using System.Windows.Media;

namespace DCAdmin
{
    public partial class WeekCodeViewModel : INotifyPropertyChanged
    {
        private WeekCode _SelectedWeekCodeRow;

        public WeekCodeViewModel()
        {
            try
            {
                _WeekCodeCollection = new ObservableCollection<WeekCode>();
                WeekCodeCollection = null;
                WeekCodeCollection = DB.Instance.LoadWeekCode();
#if DEBUG
                ErrorMessage = "Hello World!";
#endif
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public WeekCode SelectedWeekCodeRow
        {
            get
            {
                return _SelectedWeekCodeRow;
            }
            set
            {
                if (value == _SelectedWeekCodeRow) return;
                _SelectedWeekCodeRow = value;
                NotifyPropertyChanged();
            }
        }

        // public ObservableCollection<WeekCode> WeekCodeCollection { get; set; }
        private ObservableCollection<WeekCode> _WeekCodeCollection;

        public ObservableCollection<WeekCode> WeekCodeCollection
        {
            get
            {
                return _WeekCodeCollection; ;
            }
            set
            {
                _WeekCodeCollection = value;
                NotifyPropertyChanged();
            }
        }

        private string _ErrorMessage;

        public string ErrorMessage
        {
            get
            {
                return _ErrorMessage;
            }
            set
            {
                _ErrorMessage = value;
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

        internal void AddNewRecord()
        {
            ErrorMessage = GlblRes.Adding_a_row_is_not_allowed;
#if NOT_ALLOWED
            try
            {
                var entity = DB.Instance.AddNewWeekCodeRecord();
                if (entity != null)
                {
                    SelectedWeekCodeRow = entity;
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Database Error AddNewWeekCodeRecord!");
                throw;
            }
#endif
        }

        internal void TriggerSelectedRow()
        {
            var tmp = SelectedWeekCodeRow;
            SelectedWeekCodeRow = null;
            SelectedWeekCodeRow = tmp;
        }

        internal void DeleteSelectedRecord()
        {
            ErrorMessage = GlblRes.Deleting_a_row_is_not_allowed;
#if NOT_ALLOWED
            DB.Instance.DeleteWeekCodeRecord(SelectedWeekCodeRow);
#endif
        }

        internal void SaveChanges()
        {
            try
            {
                ErrorMessage = string.Empty;
                DB.Instance.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                var error = ex.EntityValidationErrors.First().ValidationErrors.First();
                ErrorMessage = string.Format(GlblRes.Error_Saving_to_Database_0, error.ErrorMessage);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Database Error while SaveChanges!");
                throw;
            }
        }

        internal void RefreshDatabase(bool saveChanges)
        {
            try
            {
                ErrorMessage = string.Empty;
                if (saveChanges)
                {
                    SaveChanges();
                }

                _WeekCodeCollection = new ObservableCollection<WeekCode>();
                WeekCodeCollection = null;
                WeekCodeCollection = DB.Instance.RefreshWeekCode();
            }
            catch (EntityException ex)
            {
                Log.Fatal(ex, "Database Error while Refreshing!");
                ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Database Error while Refreshing!");
                ErrorMessage = ex.Message;
                throw;
            }
        }

        private Color _editColor;

        public Color EditColor
        {
            get
            {
                return _editColor;
            }
            internal set
            {
                _editColor = value;
                NotifyPropertyChanged();
                RaiseEventColorEvent(_editColor);
            }
        }

        #region Signal Edit Color Event

        public delegate void EventColorHandler(Color color);

        public event EventColorHandler EventColorEvent;

        internal void RaiseEventColorEvent(Color color)
        {
            EventColorHandler handler = EventColorEvent;
            if (handler != null)
            {
                handler(color);
            }
        }

        public class EventColorArgs : EventArgs
        {
            public EventColorArgs(Color c)
            {
                Color = c;
            }

            public Color Color { get; private set; } // readonly
        }

        #endregion Signal Edit Color Event

        #region Saved Changes Event

        public delegate void SaveChangesHandler();

        public event SaveChangesHandler SaveChangesEvent;

        internal void RaiseSaveChangesEvent()
        {
            SaveChangesHandler handler = SaveChangesEvent;
            if (handler != null)
            {
                handler();
            }
        }

        #endregion Saved Changes Event
    }
}