using AutoMapper;
using Contracts;
using DCLog;
using DCMarkerEF;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Reflection;
using GlblRes = global::DCMarker.Properties.Resources;

namespace DCMarker.Model
{
    public interface ITimestamp
    {
        DateTime Now { get; }
    }

    public class DB
    {
        public static List<LaserObjectData> ConvertHistoryDataToList(HistoryData historyData)
        {
            List<LaserObjectData> result = new List<LaserObjectData>();
            Type historyDataType = historyData.GetType();
            PropertyInfo[] pinfoArr = historyDataType.GetProperties();
            foreach (var pinfo in pinfoArr)
            {
                LaserObjectData hrec = new LaserObjectData();
                hrec.ID = pinfo.Name.ToString();
                hrec.Value = pinfo.GetValue(historyData).ToString();
                result.Add(hrec);
            }

            return result;
        }

        public HistoryData AddHistoryDataToDB(HistoryData historyData)
        {
            HistoryData result = null;

            using (var _context = new DCLasermarkContext())
            {
                try
                {
                    result = _context.HistoryData.Add(historyData);
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
                    result = null;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, GlblRes.AddHistroyDataToDB_Exception);
                    result = null;
                }
            }
            return result;
        }

        public HistoryData CreateHistoryData(string article, string kant, bool hasEdges = false)
        {
            HistoryData result = new HistoryData();
            SerialNumber snr;

            if (hasEdges)
            {
                snr = GetLastSerialNumber();
            }
            else
            {
                snr = CreateSerialNumber();
            }
            LaserData ldata = GetLaserData(article, kant);
            result = FillHistoryData(ldata, snr);

            return result;
        }

        public HistoryData CreateHistoryData(LaserData laserDta)
        {
            var snr = CreateSerialNumber();
            var historyData = FillHistoryData(laserDta, snr);
            return historyData;
        }

        internal void IsConnectionOK()
        {
            try
            {
                using (var context = new DCLasermarkContext())
                {
                    var tmp = context.LaserData.First();
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "IsConnection Failed!");
                throw;
            }
        }

        public LaserData GetLaserData(string articleNumber, string kant)
        {
            LaserData result = null;

            using (var context = new DCLasermarkContext())
            {
                result = context.LaserData
                    // .Where(r => r.F1 == articleNumber && r.Kant == kant)
                    .FirstOrDefault(r => r.F1 == articleNumber && r.Kant == kant);
            }
            return result;
        }

        public List<LaserObjectData> GetLaserDataAsObjects(string articleNumber)
        {
            List<LaserObjectData> result = null;
            using (var _context = new DCLasermarkContext())
            {
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
            }
            return result;
        }

        internal List<Article> GetArticle(string articleNumber)
        {
            List<Article> result = null;

            using (var context = new DCLasermarkContext())
            {
                result = context.LaserData
                    .OrderBy(x => x.F1).ThenBy(x => x.Kant).Where(r => r.F1 == articleNumber)
                  .Select(x => new Article
                  {
                      Id = x.Id,
                      F1 = x.F1,
                      Kant = x.Kant,
                      FixtureId = x.FixtureId,
                      EnableTO = x.EnableTO,
                      Template = x.Template,
                  }).ToList();
            }
            return result;
        }

        private SerialNumber CreateSerialNumber()
        {
            SerialNumber result = null;

            using (var context = new DCLasermarkContext())
            {
                try
                {
                    var currentTime = DateTime.Now;
                    var snr = new SerialNumber
                    {
                        Issued = currentTime
                    };

                    result = context.SerialNumber.Add(snr);
                    context.SaveChanges();
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
                    Log.Error(ex, GlblRes.CreateSerialNumber_Exception);
                    throw;
                }
            }
            return result;
        }

        private HistoryData FillHistoryData(LaserData laserDta, SerialNumber snr)
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<LaserData, HistoryData>();
            });

            var dtMark = new DateMark(snr.Issued);
            HistoryData result = Mapper.Map<LaserData, HistoryData>(laserDta);
            result.Snr = snr.Snr.ToString();
            result.Issued = DateTime.Now;
            result.DateMark = dtMark.Mark;
            result.DateMarkLong = dtMark.MarkLong;
            result.DateMarkShort = dtMark.MarkShort;

            return result;
        }

        private SerialNumber GetLastSerialNumber()
        {
            SerialNumber result = null;
            using (var context = new DCLasermarkContext())
            {
                result = context.SerialNumber
                        .OrderByDescending(s => s.Snr)
                       .First();

                result.Issued = DateTime.Now;
            }

            return result;
        }
    }
}