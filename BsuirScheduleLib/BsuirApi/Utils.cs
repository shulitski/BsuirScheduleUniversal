using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using BsuirScheduleLib.BsuirApi.Schedule;
using Newtonsoft.Json;

namespace BsuirScheduleLib.BsuirApi
{
    internal static class Utils
    {
        private static DateTime _weekCalculationBaseDate = new DateTime(2018, 10, 1);
        private static int _weekCalculationBaseWeek = 2;

        internal static async Task<string> LoadString(string url)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(5);
                return await client.GetStringAsync(url);
            }
        }

        internal static DayOfWeek StringToDayOfWeek(string name)
        {
            string str = name.ToLower().Trim();
            switch (str)
            {
                case "понедельник": return DayOfWeek.Monday;
                case "вторник": return DayOfWeek.Tuesday;
                case "среда": return DayOfWeek.Wednesday;
                case "четверг": return DayOfWeek.Thursday;
                case "пятница": return DayOfWeek.Friday;
                case "суббота": return DayOfWeek.Saturday;
                case "воскресенье": return DayOfWeek.Sunday;
                default: throw new ArgumentException("Invalid day of week name");
            }
        }

        internal static void SetWeekCalculationBase(DateTime date, int weekNum)
        {
            var tempDate = date;
            while (tempDate.DayOfWeek != DayOfWeek.Monday)
            {
                tempDate = tempDate.AddDays(-1);
            }

            _weekCalculationBaseDate = tempDate.Date;
            _weekCalculationBaseWeek = weekNum;
        }

        internal static int BsuirWeekNum(this DateTime date)
        {
            if (date >= _weekCalculationBaseDate)
            {
                var interval = date - _weekCalculationBaseDate;
                return (interval.Days / 7 + (_weekCalculationBaseWeek - 1)) % 4 + 1;
            }
            else
            {
                var interval = _weekCalculationBaseDate - date;
                var weekInterval = interval.Days / 7 + ((interval.Days % 7 > 0) ? 1 : 0); 
                // '+1' - округление в большую сторону.
                // Если сейчас 30.09.2018, а база 01.10.2018, то мы находимся уже на прошлой неделе, а не на этой же.

                return ((_weekCalculationBaseWeek - 1) - weekInterval + 4) % 4 + 1;
                // Пример: weekInterval = 3, baseWeek = 2. (baseweek - 1) - weekInerval = -2
                // Номер недели, если считать с нуля: -2 + 4 = 2
                // Номер недели, если считать с единицы: -2 + 4 + 1 = 3
            }
        }

        internal static bool FilterSubgroup(int filter, int subgroup)
        {
            return (filter == 0) || (subgroup == 0) || (subgroup == filter);
        }

        internal static T WaitResult<T>(this IAsyncOperation<T> operation)
        {
            operation.AsTask().Wait();
            return operation.GetResults();
        }
    }
}
