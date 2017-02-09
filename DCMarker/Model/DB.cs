using Contracts;
using DCMarkerEF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;

namespace DCMarker.Model
{
    public class DB
    {
        private DCLasermarkContext _context;

        public DCLasermarkContext Context { get; internal set; }

        public DB()
        {
            _context = new DCLasermarkContext();
            Context = _context;
        }

        public LaserData FindArticle(string articleNumber)
        {
            var entity = _context.LaserData.FirstOrDefault<LaserData>(e => e.F1 == articleNumber);

            return entity;
        }

        public List<LaserObjectData> GetLaserDataAsObjects(string articleNumber)
        {
            List<LaserObjectData> result = null;
            var entity = _context.LaserData.FirstOrDefault<LaserData>(e => e.F1 == articleNumber);
            if (entity != null)
            {
                result = new List<LaserObjectData>();
                Type entityType = entity.GetType();
                PropertyInfo[] pinfoArr = entityType.GetProperties();

                foreach (var pinfo in pinfoArr)
                {
                    var rec = new LaserObjectData
                    {
                        ID = pinfo.Name.ToString(),
                        Value = pinfo.GetValue(entity).ToString()
                    };
                    result.Add(rec);
                }
            }
            return result;
        }

        public ObservableCollection<WeekCode> LoadWeekCode()
        {
            ObservableCollection<WeekCode> result;

            _context.WeekCode.Load();
            result = _context.WeekCode.Local;

            return result;
        }

        public ObservableCollection<QuarterCode> LoadQuarterCode()
        {
            ObservableCollection<QuarterCode> result;

            _context.QuarterCode.Load();
            result = _context.QuarterCode.Local;

            return result;
        }

        public ObservableCollection<Fixture> LoadFixture()
        {
            ObservableCollection<Fixture> result;

            _context.Fixture.Load();
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

            _context.LaserData.OrderBy(x => x.F1).Load();
            result = _context.LaserData.Local;

            return result;
        }

        public bool IsChangesPending()
        {
            return _context.ChangeTracker.HasChanges();
        }

        public void SaveChanges()
        {
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                var errors = _context.GetValidationErrors();
                Console.WriteLine(ex);
                throw;
            }
        }

        internal void Dispose()
        {
            _context.Dispose();
        }

        //internal object AddNewRecord<T>()
        //{
        //    <T> entity = new <T>();

        //    var result = _context.LaserData.Add(entity);

        //    return result;
        //}

        internal object AddNewLaserDataRecord()
        {
            LaserData result;
            LaserData entity = new LaserData();

            result = _context.LaserData.Add(entity);

            return result;
        }

        internal object AddNewWeekCodeRecord()
        {
            WeekCode result;
            WeekCode entity = new WeekCode();

            result = _context.WeekCode.Add(entity);

            return result;
        }

        internal object AddNewQuartalCodeRecord()
        {
            QuarterCode result;
            QuarterCode entity = new QuarterCode();

            result = _context.QuarterCode.Add(entity);

            return result;
        }

        internal object AddNewFixtureRecord()
        {
            LaserData result;
            LaserData entity = new LaserData();

            result = _context.LaserData.Add(entity);

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