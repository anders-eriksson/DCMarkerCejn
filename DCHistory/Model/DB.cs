using DCLog;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace DCHistory.Model
{
    internal class DB
    {
        public static string ErrorMessage { get; set; }

        public static ObservableCollection<HistoryData> GetAllHistory()
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
                ErrorMessage = "Out of memory! Please use a filter to make selection smaller!";
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "GetAllHistory exception");
                throw;
            }

            return result;
        }

        public static ObservableCollection<HistoryData> GetDateFilteredHistory(DateTime start, DateTime end)
        {
            ObservableCollection<HistoryData> result = null;

            try
            {
                using (var context = new DCHistoryContext())
                {
                    var query = context.HistoryDatas.AsNoTracking().OrderByDescending(x => x.Issued).Where(w => w.Issued >= start && w.Issued <= end);
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
                Log.Fatal(ex, "GetAllHistory exception");
                throw;
            }

            return result;
        }

        internal static HistoryData FindSerialNumber(string serialNumber)
        {
            using (var context = new DCHistoryContext())
            {
                var entity = context.HistoryDatas.FirstOrDefault<HistoryData>(e => e.Snr == serialNumber);

                return entity;
            }
        }
    }
}
