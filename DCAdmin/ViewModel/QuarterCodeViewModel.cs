using DCMarkerEF;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;
using System.Data.Entity.Validation;
using System.Linq;
using System.Data.Entity.Core;
using DCLog;
using System.Windows.Media;
using AutoMapper;

namespace DCAdmin
{
    public partial class QuarterCodeViewModel : INotifyPropertyChanged
    {
        private QuarterCode _SelectedQuarterCodeRow;

        public QuarterCodeViewModel()
        {
            try
            {
                _QuarterCodeCollection = new ObservableCollection<QuarterCode>();
                QuarterCodeCollection = null;
                QuarterCodeCollection = DB.Instance.LoadQuarterCode();
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

        //public ObservableCollection<QuarterCode> QuarterCodeCollection { get; set; }
        private ObservableCollection<QuarterCode> _QuarterCodeCollection;

        public ObservableCollection<QuarterCode> QuarterCodeCollection
        {
            get
            {
                return _QuarterCodeCollection;
            }
            set
            {
                _QuarterCodeCollection = value;
                NotifyPropertyChanged();
            }
        }

        public QuarterCode SelectedQuarterCodeRow
        {
            get
            {
                return _SelectedQuarterCodeRow;
            }
            set
            {
                if (value == _SelectedQuarterCodeRow) return;
                _SelectedQuarterCodeRow = value;
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
            try
            {
                ErrorMessage = string.Empty;
                var entity = DB.Instance.AddNewQuartalCodeRecord();
                if (entity != null)
                {
#if DEBUG
                    var x = QuarterCodeCollection.FirstOrDefault(e => e.QYear == entity.QYear);
#endif
                    SelectedQuarterCodeRow = entity;
                }
            }
            catch (Exception ex)
            {
                DCLog.Log.Fatal(ex, "Database Error AddNewQuartalCodeRecord!");
                throw;
            }
        }

        internal object AddRowFromSelected()
        {
            Mapper.Initialize(cfg => cfg.CreateMap<QuarterCode, QuarterCode>());
            QuarterCode newRecord = Mapper.Map<QuarterCode, QuarterCode>(SelectedQuarterCodeRow);
            // TODO: MachineId
            //newRecord.MachineId = machineId,
            newRecord.QYear = string.Empty;
            DB.Instance.AddNewQuartalCodeRecord(ref newRecord);
            SelectedQuarterCodeRow = newRecord;

            return newRecord;
        }

        internal void DeleteSelectedRecord()
        {
            ErrorMessage = string.Empty;
            DB.Instance.DeleteQuarterCodeRecord(SelectedQuarterCodeRow);
        }

        internal void TriggerSelectedRow()
        {
            var tmp = SelectedQuarterCodeRow;
            SelectedQuarterCodeRow = null;
            SelectedQuarterCodeRow = tmp;
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
                ErrorMessage = string.Format("Error Saving to Database: {0}", error.ErrorMessage);
            }
            catch (Exception ex)
            {
                DCLog.Log.Fatal(ex, "Database Error while SaveChanges!");
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

                _QuarterCodeCollection = new ObservableCollection<QuarterCode>();
                QuarterCodeCollection = null;
                QuarterCodeCollection = DB.Instance.RefreshQuarterCode();
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