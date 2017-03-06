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
    public class DB
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
            _context.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
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

        public LaserData FindArticle(string articleNumber)
        {
            var entity = _context.LaserData.OrderBy(o => o.F1).ThenBy(o => o.Kant).FirstOrDefault<LaserData>(e => e.F1.StartsWith(articleNumber));

            return entity;
        }

        public ObservableCollection<string> GetLaserDataColumns()
        {
            var query = (from t in typeof(LaserData).GetProperties()
                         select t.Name);
            var result = new ObservableCollection<string>(query);
            int pos = result.IndexOf("Id");
            result[pos] = "";
            return result;
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

        /// <summary>
        /// Load data from LaserData table
        /// </summary>
        /// <returns>All the data in table sorted by column F1 (Article number)</returns>
        public ObservableCollection<LaserData> LoadLaserData()
        {
            ObservableCollection<LaserData> result;

            _context.LaserData.OrderBy(x => x.F1).ThenBy(x => x.Kant).ToList();
            result = Context.LaserData.Local;

            Debug.WriteLine(GlblRes.LoadLaserData_0, result.Count);
            return result;
        }

        public ObservableCollection<LaserData> LoadLaserDataFiltered(string key, string value)
        {
            ObservableCollection<LaserData> result = null;

            try
            {
                List<Filter> filter = new List<Filter>()
                {
                    new Filter { PropertyName = key ,
                        Operation = Op.StartsWith, Value = value},
                };

                var deleg = ExpressionBuilder.ExpressionBuilder.GetExpression<LaserData>(filter);

                if (IsChangesPending())
                {
                    SaveChanges();
                }
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
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, GlblRes.LoadLaserDataFiltered_exception);
                throw;
            }
            Debug.WriteLine(GlblRes.LoadLaserDataFiltered_0, result.Count);
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

        internal WeekCode AddNewWeekCodeRecord()
        {
            WeekCode result;
            WeekCode entity = new WeekCode();

            result = _context.WeekCode.Add(entity);

            return result;
        }

        internal void DeleteLaserDataRecord(LaserData selectedItem)
        {
            if (selectedItem != null)
            {
                _context.LaserData.Remove(selectedItem);
            }
        }

        internal void DeleteWeekCodeRecord(WeekCode selectedItem)
        {
            if (selectedItem != null)
            {
                _context.WeekCode.Remove(selectedItem);
            }
        }

        internal void DeleteQuarterCodeRecord(QuarterCode selectedItem)
        {
            if (selectedItem != null)
            {
                _context.QuarterCode.Remove(selectedItem);
            }
        }

        internal void DeleteFixtureRecord(Fixture selectedItem)
        {
            if (selectedItem != null)
            {
                _context.Fixture.Remove(selectedItem);
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
            Log.Trace(GlblRes.Created_Context);
            Context = _context;
        }
    }
}