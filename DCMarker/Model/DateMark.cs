using DCMarkerEF;
using System;
using System.Diagnostics;
using System.Globalization;

namespace DCMarker.Model
{
    public class DateMark
    {
        private DateTime _issued;
        private string _mark;

        public DateMark(DateTime issued)
        {
            _issued = issued;
            _mark = GetDateMark();
        }

        private string GetDateMark()
        {
            var result = string.Empty;
            int weekno = GetIso8601WeekOfYear(_issued);
            int quarter = GetQuarter(_issued);

            using (var context = new DCLasermarkContext())
            {
                context.Database.Log = s => Debug.WriteLine(s);
                var weekcode = context.WeekCode.Find(weekno).Code;
                var qcode = context.QuarterCode.Find(_issued.Year.ToString());
                //var qcode = context.QuarterCode.Find(_issued.Year);
                string quartercode;
                switch (quarter)
                {
                    case 1:
                        quartercode = qcode.Q1;
                        break;

                    case 2:
                        quartercode = qcode.Q2;
                        break;

                    case 3:
                        quartercode = qcode.Q3;
                        break;

                    case 4:
                        quartercode = qcode.Q4;
                        break;

                    default:

                        quartercode = string.Empty;
                        break;
                }
                result = string.Format("{0}{1}", weekcode, quartercode);
            }

            return result;
        }

        private static int GetQuarter(DateTime _issued)
        {
            var result = 0;
            result = (_issued.Month + 2) / 3;
            return result;
        }

        public string Mark { get { return _mark; } }
        public string MarkLong { get { return _mark; } }
        public string MarkShort { get { return _mark.Substring(0, 1); } }

        // This presumes that weeks start with Monday.
        // Week 1 is the 1st week of the year with a Thursday in it.
        private static int GetIso8601WeekOfYear(DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            var day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
    }
}