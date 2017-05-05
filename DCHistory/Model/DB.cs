using DCLog;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using DCHistory.ExpressionBuilder;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DCHistory.Model
{
    internal class DB
    {
        public static string ErrorMessage { get; set; }

        internal static HistoryData FindSerialNumber(string serialNumber)
        {
            using (var context = new DCHistoryContext())
            {
                var entity = context.HistoryDatas.FirstOrDefault<HistoryData>(e => e.Snr == serialNumber);

                return entity;
            }
        }

        internal static ObservableCollection<HistoryData> GetAllHistory()
        {
            ObservableCollection<HistoryData> result = null;

            try
            {
                using (var context = new DCHistoryContext())
                {
                    var query = context.HistoryDatas.AsNoTracking().OrderByDescending(x => x.Issued);
                    result = new ObservableCollection<HistoryData>(query);
                }
            }
            catch (OutOfMemoryException ex)
            {
                // To many rows in select!
                Log.Error(ex, "GetAllHistory OutOfMemory");
                throw;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "GetAllHistory exception");
                throw;
            }

            return result;
        }

        internal static ObservableCollection<HistoryData> GetDateFilteredHistory(DateTime start, DateTime end)
        {
            ObservableCollection<HistoryData> result = null;

            try
            {
                using (var context = new DCHistoryContext())
                {
                    context.Database.Log = Console.Write;
                    var query = context.HistoryDatas.AsNoTracking().OrderByDescending(x => x.Issued).Where(w => w.Issued >= start && w.Issued <= end);
                    result = new ObservableCollection<HistoryData>(query);
                }
            }
            catch (OutOfMemoryException ex)
            {
                // To many rows in select!
                Log.Debug("Out of memory! Please use a filter to make selection smaller!");

                ErrorMessage = "Out of memory! Please use a filter to make selection smaller!";
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "GetAllHistory exception");
                throw;
            }

            return result;
        }

        internal static ObservableCollection<HistoryData> GetFilteredBoolHistory(string filterKey, bool filterValue)
        {
            ObservableCollection<HistoryData> result = null;

            try
            {
                //var value = Expression.Constant(filterValue, typeof(bool));
                //var value = Expression.Convert(filterValue, filterValue.GetType());
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

                var deleg = ExpressionBuilder.ExpressionBuilder.GetExpressionOrElse<HistoryData>(filter);

                using (var context = new DCHistoryContext())
                {
                    context.Database.Log = Console.Write;
                    var query = context.HistoryDatas.AsNoTracking()
                    .OrderByDescending(x => x.Issued)
                    .Where(deleg).ToList();
                    result = new ObservableCollection<HistoryData>(query);
                }
            }
            catch (OutOfMemoryException ex)
            {
                // To many rows in select!
                ErrorMessage = "Out of memory! Please use a filter to make selection smaller!";
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "GetFilteredHistory exception");
                throw;
            }

            return result;
        }

        internal static ObservableCollection<HistoryData> GetFilteredTextHistory(string filterKey, string filterValue)
        {
            ObservableCollection<HistoryData> result = null;

            try
            {
                //var filter = new List<Filter>()
                //{
                //    new Filter { PropertyName = filterKey ,
                //        Operation = Op.StartsWith, Value = filterValue},
                //};

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
                var deleg = ExpressionBuilder.ExpressionBuilder.GetExpressionOrElse<HistoryData>(filter);

                using (var context = new DCHistoryContext())
                {
                    context.Database.Log = Console.Write;
                    var query = context.HistoryDatas.AsNoTracking()
                     .OrderByDescending(x => x.Issued)
                    .Where(deleg).ToList();
                    result = new ObservableCollection<HistoryData>(query);
                }
            }
            catch (OutOfMemoryException ex)
            {
                // To many rows in select!
                ErrorMessage = "Out of memory! Please use a filter to make selection smaller!";
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "GetFilteredHistory exception");
                throw;
            }

            return result;
        }

        internal static ObservableCollection<string> GetHistoryDataColumns()
        {
            ObservableCollection<string> result;

            try
            {
                var query = (from t in typeof(HistoryData).GetProperties()
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
    }
}