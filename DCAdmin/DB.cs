using DCLog;
using DCMarkerEF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Dynamic;
using System.Windows.Controls;

namespace DCAdmin
{
    public class DB
    {
        private static readonly object mutex = new object();
        private static volatile DB instance;
        private DCLasermarkContext _context;

        public DB()
        {
            _context = new DCLasermarkContext();
            Log.Trace("Created Context");
            Context = _context;
        }

        public static string ErrorMessage { get; set; }

        /// <summary>
        /// Instanciate the one and only object!
        /// </summary>
        public static DB Instance
        {
            get
            {
                if (instance == null)

                {
                    lock (mutex)
                    {
                        if (instance == null)
                        {
                            // Call constructor
                            instance = new DB();
                        }
                    }
                }
                return instance;
            }
        }

        public DCLasermarkContext Context { get; internal set; }

        public LaserData FindArticle(string articleNumber)
        {
            var entity = _context.LaserData.FirstOrDefault<LaserData>(e => e.F1.StartsWith(articleNumber));

            return entity;
        }

        public bool IsChangesPending()
        {
            return _context.ChangeTracker.HasChanges();
        }

        public ObservableCollection<Fixture> LoadFixture()
        {
            ObservableCollection<Fixture> result;

            _context.Fixture.OrderBy(x => x.FixturId).Load();
            result = _context.Fixture.Local;

            return result;
        }

        public ObservableCollection<string> GetLaserDataColumns()
        {
            var query = (from t in typeof(LaserData).GetProperties()
                         select t.Name);
            var result = new ObservableCollection<string>(query);

            return result;
        }

        /// <summary>
        /// Load data from LaserData table
        /// </summary>
        /// <returns>All the data in table sorted by column F1 (Article number)</returns>
        public ObservableCollection<LaserData> LoadLaserData()
        {
            ObservableCollection<LaserData> result;

            _context.LaserData.OrderBy(x => x.F1).ThenBy(x => x.Kant).Load();
            result = _context.LaserData.Local;

            return result;
        }

        public ObservableCollection<LaserData> LoadLaserDataFiltered(string key, string value)
        {
            ObservableCollection<LaserData> result = null;

            try
            {
                var query = _context.LaserData
                    .Where("@0==@1", key, value)
                    .OrderByDescending(x => x.F1)
                    ;
                result = new ObservableCollection<LaserData>(query);
            }
            catch (OutOfMemoryException ex)
            {
                // To many rows in select!
                ErrorMessage = "Out of memory! Please use a filter to make selection smaller!";
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "GetAllHistory exception");
                throw;
            }

            return result;
        }

        public ObservableCollection<QuarterCode> LoadQuarterCode()
        {
            ObservableCollection<QuarterCode> result;

            _context.QuarterCode.OrderBy(x => x.QYear).Load();
            result = _context.QuarterCode.Local;

            return result;
        }

        public ObservableCollection<WeekCode> LoadWeekCode()
        {
            ObservableCollection<WeekCode> result;

            _context.WeekCode.OrderBy(x => x.WeekNo).Load();
            result = _context.WeekCode.Local;

            return result;
        }

        public void SaveChanges()
        {
            try
            {
                _context.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (DbEntityValidationResult entityErr in dbEx.EntityValidationErrors)
                {
                    foreach (DbValidationError error in entityErr.ValidationErrors)
                    {
                        Log.Error(string.Format("Error Property Name {0} : Error Message: {1}",
                                            error.PropertyName, error.ErrorMessage));
                    }
                }
            }
            catch (Exception ex)
            {
                var errors = _context.GetValidationErrors();
                Log.Error(ex, "SaveChanges Exception");
                throw;
            }
        }

        internal object AddNewFixtureRecord()
        {
            LaserData result;
            LaserData entity = new LaserData();

            result = _context.LaserData.Add(entity);

            return result;
        }

        internal object AddNewLaserDataRecord()
        {
            LaserData result;
            LaserData entity = new LaserData();

            result = _context.LaserData.Add(entity);

            return result;
        }

        internal object AddNewQuartalCodeRecord()
        {
            QuarterCode result;
            QuarterCode entity = new QuarterCode();

            result = _context.QuarterCode.Add(entity);

            return result;
        }

        internal object AddNewWeekCodeRecord()
        {
            WeekCode result;
            WeekCode entity = new WeekCode();

            result = _context.WeekCode.Add(entity);

            return result;
        }

        internal void DeleteLaserDataRecord(IList<DataGridCellInfo> selectedItems)
        {
            if (selectedItems != null)
            {
                var selectedItem = selectedItems[0];
                var item = (LaserData)selectedItem.Item;
                var id = item.Id;
                var entity = _context.LaserData.Find(id);
                _context.LaserData.Remove(entity);
            }
        }

        internal void Dispose()
        {
            _context.Dispose();
        }

        internal void Refresh()
        {
            if (IsChangesPending())
            {
                SaveChanges();
            }

            Dispose();
            _context = new DCLasermarkContext();
            Context = _context;
        }
    }
}