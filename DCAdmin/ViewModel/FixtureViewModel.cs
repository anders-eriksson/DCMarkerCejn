using DCMarkerEF;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;
using System.Data.Entity.Validation;
using System.Linq;
using DCLog;
using System.Data.Entity.Core;
using System.Windows.Media;
using AutoMapper;

namespace DCAdmin
{
    public partial class FixtureViewModel : INotifyPropertyChanged
    {
        private string _ErrorMessage;
        private Fixture _SelectedFixtureRow;

        public FixtureViewModel()
        {
            try
            {
                _FixtureCollection = new ObservableCollection<Fixture>();
                FixtureCollection = null;
                FixtureCollection = DB.Instance.LoadFixture();
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

        private ObservableCollection<Fixture> _FixtureCollection;

        public ObservableCollection<Fixture> FixtureCollection
        {
            get
            {
                return _FixtureCollection;
            }
            set
            {
                _FixtureCollection = value;
                NotifyPropertyChanged();
            }
        }

        public Fixture SelectedFixtureRow
        {
            get
            {
                return _SelectedFixtureRow;
            }
            set
            {
                if (value == _SelectedFixtureRow) return;
                _SelectedFixtureRow = value;
                NotifyPropertyChanged();
            }
        }

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

        private Fixture _LastAddedRow;

        public Fixture LastAddedRow
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
                var entity = DB.Instance.AddNewFixtureRecord();
                if (entity != null)
                {
                    SelectedFixtureRow = entity;
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Database Error AddNewFixtureRecord!");
                throw;
            }
        }

        internal object AddRow(string fixtureId)
        {
            Fixture d = new Fixture()
            {
                // TODO: MachineId
                //MachineId = machineId,
                FixturId = fixtureId,
            };
            try
            {
                DB.Instance.AddNewFixtureRecord(ref d);
                LastAddedRow = d;
                SelectedFixtureRow = d;
            }
            catch (DbEntityValidationException ex)
            {
                var error = ex.EntityValidationErrors.First().ValidationErrors.First();
                ErrorMessage = string.Format("Error Adding to Database: {0}", error.ErrorMessage);
                d = null;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Database Error AddRow!");
                throw;
            }

            return d;
        }

        internal object AddRowFromSelected(string fixtureId)
        {
            Mapper.Initialize(cfg => cfg.CreateMap<Fixture, Fixture>());
            Fixture newRecord = Mapper.Map<Fixture, Fixture>(SelectedFixtureRow);
            // TODO: MachineId
            //newRecord.MachineId = machineId,
            newRecord.FixturId = fixtureId;
            DB.Instance.AddNewFixtureRecord(ref newRecord);
            SelectedFixtureRow = newRecord;

            return newRecord;
        }

        internal void DeleteSelectedRecord()
        {
            ErrorMessage = string.Empty;
            DB.Instance.DeleteFixtureRecord(SelectedFixtureRow);
        }

        internal void TriggerSelectedRow()
        {
            var tmp = SelectedFixtureRow;
            SelectedFixtureRow = null;
            SelectedFixtureRow = tmp;
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

                _FixtureCollection = new ObservableCollection<Fixture>();
                FixtureCollection = null;
                FixtureCollection = DB.Instance.RefreshFixture();
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