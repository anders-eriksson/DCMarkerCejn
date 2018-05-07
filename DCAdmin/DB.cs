using DCAdmin.ExpressionBuilder;
using DCLog;
using DCMarkerEF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using GlblRes = global::DCAdmin.Properties.Resources;

namespace DCAdmin
{
    public class DB : IDisposable
    {
        private static readonly object mutex = new object();
        private static volatile DB instance;
        private DCLasermarkContext _context;

        public DB()
        {
            _context = new DCLasermarkContext();
            Log.Trace(GlblRes.Created_Context);
            Context = _context;
#if DEBUG
            //_context.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
#endif
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

        public void AddLaserData(ref LaserData d)
        {
            try
            {
                var x = _context.LaserData.Add(d);
                SaveChanges();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Database Error while Adding LaserData");
                throw;
            }
        }

        public bool ExistsLaserData(string machineId, string article, string kant)
        {
            bool result = false;
            var found = _context.LaserData.FirstOrDefault(p => p.F1 == article && p.Kant == kant);
            if (found != null)
            {
                result = true;
            }

            return result;
        }

        public bool ExistsFixture(string fixtureId)
        {
            bool result = false;
            var found = _context.Fixture.FirstOrDefault(p => p.FixturId == fixtureId);
            if (found != null)
            {
                result = true;
            }

            return result;
        }

        public LaserData FindArticle(string articleNumber)
        {
            var entity = _context.LaserData.OrderBy(o => o.F1).ThenBy(o => o.Kant).FirstOrDefault<LaserData>(e => e.F1.StartsWith(articleNumber));

            return entity;
        }

        public ObservableCollection<string> GetLaserDataColumns()
        {
            ObservableCollection<string> result;

            try
            {
                //var query = (from t in typeof(LaserData).GetProperties()
                //             select t.Name);
                //result = new ObservableCollection<string>(query);
                //int pos = result.IndexOf("Id");
                //result[pos] = "";

                var query = (from t in typeof(LaserData).GetProperties()
                             select t.Name);
                result = new ObservableCollection<string>(query.OrderBy(x => x));
                if (result != null)
                {
                    int pos = result.IndexOf("Id");
                    result.RemoveAt(pos);
                    result.Insert(0, "");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error EntityFramework");
                throw;
            }

            return result;
        }

        public bool IsChangesPending()
        {
            return _context.ChangeTracker.HasChanges();
        }

        public ObservableCollection<Fixture> LoadFixture()
        {
            ObservableCollection<Fixture> result;

            try
            {
                Dispose();
                _context = new DCLasermarkContext();
                _context.Fixture.OrderBy(x => x.FixturId).Load();
                result = _context.Fixture.Local;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error EntityFramework");
                throw;
            }

            return result;
        }

        /// <summary>
        /// Load data from LaserData table
        /// </summary>
        /// <returns>All the data in table sorted by column F1 (Article number)</returns>
        public ObservableCollection<LaserData> LoadLaserData()
        {
            ObservableCollection<LaserData> result;

            try
            {
                Dispose();
                _context = new DCLasermarkContext();

                _context.LaserData.OrderBy(x => x.F1).ThenBy(x => x.Kant).ToList();
                result = _context.LaserData.Local;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error EntityFramework");
                throw;
            }

            //Debug.WriteLine(GlblRes.LoadLaserData_0, result.Count);
            return result;
        }

        public ObservableCollection<LaserData> LoadLaserDataFilteredText(string filterKey, string filterValue)
        {
            ObservableCollection<LaserData> result = null;

            try
            {
                var filter = new List<Filter>();
                var f = new Filter
                {
                    PropertyName = filterKey,
                    Operation = Op.Equals,
                    Value = filterValue
                };
                filter.Add(f);
                if (filterValue == null)
                {
                    f = new Filter
                    {
                        PropertyName = filterKey,
                        Operation = Op.Equals,
                        Value = string.Empty
                    };
                    filter.Add(f);
                }
                var deleg = ExpressionBuilder.ExpressionBuilder.GetExpressionOrElse<LaserData>(filter);

                if (IsChangesPending())
                {
                    SaveChanges();
                }
                Dispose();
                _context = new DCLasermarkContext();

                _context.LaserData
                    .OrderBy(x => x.F1).ThenBy(x => x.Kant)
                    .Where(deleg).ToList();
                //.AsQueryable()
                //.Load();
                result = _context.LaserData.Local;
            }
            catch (OutOfMemoryException ex)
            {
                // To many rows in select!
                ErrorMessage = GlblRes.Out_of_memory_Please_use_a_filter_to_make_selection_smaller;
                Log.Fatal(ex, GlblRes.Out_of_memory_Please_use_a_filter_to_make_selection_smaller);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, GlblRes.LoadLaserDataFiltered_exception);
                throw;
            }
            //Debug.WriteLine(GlblRes.LoadLaserDataFiltered_0, result.Count);
            return result;
        }

        internal ObservableCollection<LaserData> LoadLaserDataFilteredBool(string filterKey, bool filterValue)
        {
            ObservableCollection<LaserData> result = null;

            try
            {
                var filter = new List<Filter>();
                var f = new Filter
                {
                    PropertyName = filterKey,
                    Operation = Op.Equals,
                    Value = filterValue
                };
                filter.Add(f);
                if (!filterValue)
                {
                    f = new Filter
                    {
                        PropertyName = filterKey,
                        Operation = Op.Equals,
                        Value = null
                    };
                    filter.Add(f);
                }

                var deleg = ExpressionBuilder.ExpressionBuilder.GetExpressionOrElse<LaserData>(filter);

                if (IsChangesPending())
                {
                    SaveChanges();
                }
                Dispose();
                _context = new DCLasermarkContext();

                _context.Database.Log = Console.Write;
                var query = _context.LaserData
                .OrderBy(x => x.F1).ThenBy(x => x.Kant)
                .Where(deleg).ToList();
                result = new ObservableCollection<LaserData>(query);
            }
            catch (OutOfMemoryException ex)
            {
                // To many rows in select!
                ErrorMessage = "Out of memory! Please use a filter to make selection smaller!";
                Log.Error(ex, ErrorMessage);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "GetFilteredHistory exception");
                throw;
            }

            return result;
        }

        public ObservableCollection<QuarterCode> LoadQuarterCode()
        {
            ObservableCollection<QuarterCode> result;

            Dispose();
            _context = new DCLasermarkContext();
            try
            {
                _context.QuarterCode.OrderBy(x => x.QYear).Load();
                result = _context.QuarterCode.Local;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error EntityFramework");
                throw;
            }

            return result;
        }

        public ObservableCollection<WeekCode> LoadWeekCode()
        {
            ObservableCollection<WeekCode> result;

            Dispose();
            _context = new DCLasermarkContext();

            try
            {
                _context.WeekCode.OrderBy(x => x.WeekNo).Load();
                result = _context.WeekCode.Local;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error EntityFramework");
                throw;
            }

            return result;
        }

        public void SaveChanges()
        {
            try
            {
#if DEBUG
                bool brc = IsChangesPending();
#endif
                _context.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (DbEntityValidationResult entityErr in dbEx.EntityValidationErrors)
                {
                    foreach (DbValidationError error in entityErr.ValidationErrors)
                    {
                        Log.Error(string.Format(GlblRes.Error_Property_Name_0__Error_Message_1,
                                            error.PropertyName, error.ErrorMessage));
                    }
                }

                throw;
            }
            catch (Exception ex)
            {
                var errors = _context.GetValidationErrors();
                Log.Error(ex, GlblRes.SaveChanges_Exception);
                throw;
            }
        }

        internal Fixture AddNewFixtureRecord()
        {
            Fixture result;
            Fixture entity = new Fixture();

            result = _context.Fixture.Add(entity);

            return result;
        }

        internal void AddNewFixtureRecord(ref Fixture fixture)
        {
            try
            {
                var x = _context.Fixture.Add(fixture);
                SaveChanges();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Database Error while Adding Fixture Data");
                throw;
            }
        }

        internal LaserData AddNewLaserDataRecord()
        {
            LaserData result;
            LaserData entity = new LaserData();

            result = _context.LaserData.Add(entity);

            return result;
        }

        internal QuarterCode AddNewQuartalCodeRecord()
        {
            QuarterCode result;
            QuarterCode entity = new QuarterCode();

            result = _context.QuarterCode.Add(entity);

            return result;
        }

        internal void AddNewQuartalCodeRecord(ref QuarterCode qcode)
        {
            try
            {
                var x = _context.QuarterCode.Add(qcode);
                SaveChanges();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Database Error while Adding QuartalCode Data");
                throw;
            }
        }

        /// <summary>
        /// Don't use!
        /// We don't allow adding a week
        /// </summary>
        /// <returns>new weekcode record</returns>
        internal WeekCode AddNewWeekCodeRecord()
        {
            WeekCode result;
            WeekCode entity = new WeekCode();

            result = _context.WeekCode.Add(entity);

            return result;
        }

        internal void DeleteFixtureRecord(Fixture selectedItem)
        {
            if (selectedItem != null && selectedItem.FixturId != null)
            {
                try
                {
                    if (!_context.Fixture.Local.Any(l => l.FixturId == selectedItem.FixturId))
                    {
                        _context.Fixture.Attach(selectedItem);
                    }

                    _context.Fixture.Remove(selectedItem);
                    SaveChanges();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error when trying to remove an entity that isn't in the context");
                    throw;
                }
            }
        }

        internal void DeleteLaserDataRecord(LaserData selectedItem)
        {
            if (selectedItem != null && selectedItem.F1 != null)
            {
                try
                {
                    if (!_context.LaserData.Local.Any(l => l.F1 == selectedItem.F1 && l.Kant == selectedItem.Kant))
                    {
                        _context.LaserData.Attach(selectedItem);
                    }

                    _context.LaserData.Remove(selectedItem);
                    SaveChanges();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error when trying to remove an entity that isn't in the context");
                    throw;
                }
            }
        }

        internal void DeleteQuarterCodeRecord(QuarterCode selectedItem)
        {
            if (selectedItem != null && selectedItem.QYear != null)
            {
                try
                {
                    if (!_context.QuarterCode.Local.Any(l => l.QYear == selectedItem.QYear))
                    {
                        _context.QuarterCode.Attach(selectedItem);
                    }

                    _context.QuarterCode.Remove(selectedItem);
                    SaveChanges();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error when trying to remove an entity that isn't in the context");
                    throw;
                }
            }
        }

        /// <summary>
        /// This method is not used since the user should not be able to delete a week.
        /// </summary>
        /// <param name="selectedItem">The item that is selected in the datagrid and that will be deleted</param>
        internal void DeleteWeekCodeRecord(WeekCode selectedItem)
        {
            if (selectedItem != null)
            {
                try
                {
                    if (!_context.WeekCode.Local.Any(l => l.WeekNo == selectedItem.WeekNo))
                    {
                        _context.WeekCode.Attach(selectedItem);
                    }

                    _context.WeekCode.Remove(selectedItem);
                    SaveChanges();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error when trying to remove an entity that isn't in the context");
                    throw;
                }
            }
        }

        internal LaserData FindLaserData(LaserData selectedLaserDataRow)
        {
            var found = _context.LaserData.FirstOrDefault(x => x.Id == selectedLaserDataRow.Id);

            return found;
        }

        internal void Refresh()
        {
            if (IsChangesPending())
            {
                SaveChanges();
            }

            Dispose();
            _context = new DCLasermarkContext();
            Log.Trace(GlblRes.Created_Context);
            Context = _context;
        }

        internal ObservableCollection<Fixture> RefreshFixture()
        {
            ObservableCollection<Fixture> result = null;

            try
            {
                result = LoadFixture();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error EntityFramework");
                throw;
            }

            return result;
        }

        internal ObservableCollection<LaserData> RefreshLaserData()
        {
            ObservableCollection<LaserData> result = null;

            try
            {
                result = LoadLaserData();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error EntityFramework");
                throw;
            }

            return result;
        }

        internal ObservableCollection<QuarterCode> RefreshQuarterCode()
        {
            ObservableCollection<QuarterCode> result = null;

            try
            {
                result = LoadQuarterCode();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error EntityFramework");
                throw;
            }

            return result;
        }

        internal ObservableCollection<WeekCode> RefreshWeekCode()
        {
            ObservableCollection<WeekCode> result = null;

            try
            {
                result = LoadWeekCode();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error EntityFramework");
                throw;
            }

            return result;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
                _context.Dispose();
            }
            // free native resources
        }
    }
}