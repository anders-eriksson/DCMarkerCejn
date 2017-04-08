//#define TEST

using DCMarkerEF;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity.Validation;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using AutoMapper;

namespace DCAdmin.ViewModel
{
    public class LaserDataViewModel : INotifyPropertyChanged
    {
        private string _FilterKey;
        private string _FilterValue;

        public LaserDataViewModel()
        {
            try
            {
                KeyCollection = DB.Instance.GetLaserDataColumns();
#if TEST
                _LaserDataCollection = new ObservableCollection<LaserData>();
#endif
                //LaserDataCollection = null;
                //LaserDataCollection = DB.Instance.LoadLaserData();
#if DEBUG
                ErrorMessage = "Hello World!";
#else
                ErrorMessage = string.Empty;
#endif
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

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

        public string FilterKey
        {
            get
            {
                return _FilterKey;
            }
            set
            {
                _FilterKey = value;
                NotifyPropertyChanged();
            }
        }

        public string FilterValue
        {
            get
            {
                return _FilterValue;
            }
            set
            {
                _FilterValue = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<string> KeyCollection { get; set; }

        public ObservableCollection<LaserData> LaserDataCollection { get; set; }

        private LaserData _SelectedLaserDataRow;

        public LaserData SelectedLaserDataRow
        {
            get
            {
                return _SelectedLaserDataRow;
            }
            set
            {
                _SelectedLaserDataRow = value;
                NotifyPropertyChanged();
#if DEBUG
                DisplaySelectedLaserDataRow();
#endif
            }
        }

        private void DisplaySelectedLaserDataRow()
        {
            if (SelectedLaserDataRow != null)
            {
                ErrorMessage = SelectedLaserDataRow.F1;
            }
            else
            {
                ErrorMessage = string.Empty;
            }
        }

        private LaserData _LastAddedRow;

        public LaserData LastAddedRow
        {
            get
            {
                return _LastAddedRow;
            }
            set
            {
                _LastAddedRow = value;
                NotifyPropertyChanged();
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

        internal object FindArticleAndScrollIntoView(string searchText)
        {
            ErrorMessage = string.Empty;
            var entity = FindSerialNumber(searchText);
            if (entity == null)
            {
                ErrorMessage = "Article number not found";
            }
            else
            {
                ErrorMessage = string.Empty;
            }
            SelectedLaserDataRow = entity;

            return entity;
        }

        internal LaserData FindSerialNumber(string articleNumber)
        {
            var entity = LaserDataCollection.FirstOrDefault(c => c.F1.StartsWith(articleNumber, StringComparison.CurrentCultureIgnoreCase));
            return entity;
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        internal void TriggerSelectedRow()
        {
            var tmp = SelectedLaserDataRow;
            SelectedLaserDataRow = null;
            SelectedLaserDataRow = tmp;
        }

        internal void AddNewRecord()
        {
            ErrorMessage = string.Empty;
            try
            {
                var entity = DB.Instance.AddNewLaserDataRecord();
#if DEBUG
                var x = LaserDataCollection.FirstOrDefault(e => e.Id == entity.Id);
#endif
                if (entity != null)
                {
                    SelectedLaserDataRow = entity;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        internal object AddRow(string machineId, string article, string kant)
        {
            LaserData d = new LaserData()
            {
                // TODO: MachineId
                //MachineId = machineId,
                F1 = article,
                Kant = kant
            };
            try
            {
                DB.Instance.AddLaserData(ref d);
                LastAddedRow = d;
                SelectedLaserDataRow = d;
            }
            catch (DbEntityValidationException ex)
            {
                var error = ex.EntityValidationErrors.First().ValidationErrors.First();
                ErrorMessage = string.Format("Error Adding to Database: {0}", error.ErrorMessage);
                d = null;
            }
            catch (Exception)
            {
                throw;
            }

            return d;
        }

        internal object AddRowFromSelected(string machineId, string article, string kant)
        {
            Mapper.Initialize(cfg => cfg.CreateMap<LaserData, LaserData>());
            LaserData newRecord = Mapper.Map<LaserData, LaserData>(SelectedLaserDataRow);
            // TODO: MachineId
            //newRecord.MachineId = machineId,
            newRecord.F1 = article;
            newRecord.Kant = string.IsNullOrWhiteSpace(kant) ? null : kant;
            newRecord.Id = 0;
            DB.Instance.AddLaserData(ref newRecord);
            SelectedLaserDataRow = newRecord;

            return newRecord;
        }

        internal void ExecuteFilter()
        {
            ErrorMessage = string.Empty;
            LaserDataCollection = DB.Instance.LoadLaserDataFiltered(FilterKey, FilterValue);
        }

        internal void ExecuteNoFilter()
        {
            ErrorMessage = string.Empty;
            LaserDataCollection = DB.Instance.LoadLaserData();
        }

        internal void SaveChanges()
        {
            ErrorMessage = string.Empty;
            try
            {
                DB.Instance.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                var error = ex.EntityValidationErrors.First().ValidationErrors.First();
                ErrorMessage = string.Format("Error Saving to Database: {0}", error.ErrorMessage);
            }
            catch (Exception)
            {
                throw;
            }
        }

        internal void DeleteSelectedRecord()
        {
            ErrorMessage = string.Empty;
            try
            {
                DB.Instance.DeleteLaserDataRecord(SelectedLaserDataRow);
            }
            catch (Exception)
            {
                ErrorMessage = "Can't delete selected row!";
            }
        }

        internal void RefreshDatabase(bool saveChanges = false)
        {
            ErrorMessage = string.Empty;
            try
            {
                if (saveChanges)
                {
                    SaveChanges();
                }

                LaserDataCollection = DB.Instance.LoadLaserData();
            }
            catch (Exception)
            {
                throw;
            }
        }

        internal void GotoSelected()
        {
            LaserData found = DB.Instance.FindLaserData(LastAddedRow);
            var equ = found.Equals(LastAddedRow);
            if (found != null && found.Equals(LastAddedRow))
            {
                SelectedLaserDataRow = found;
            }
            else
            {
                var tmp = SelectedLaserDataRow;
                SelectedLaserDataRow = null;
                SelectedLaserDataRow = tmp;
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