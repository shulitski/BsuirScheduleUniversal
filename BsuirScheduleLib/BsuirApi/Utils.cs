using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BsuirScheduleLib.BsuirApi.Schedule;
using Newtonsoft.Json;

namespace BsuirScheduleLib.BsuirApi
{
    internal static class Utils
    {
        internal static string LoadString(string url)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                var task = client.GetStringAsync("https://students.bsuir.by/api/v1/studentGroup/schedule?studentGroup=551005");
                task.Wait();
                return task.Result;
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

        internal static int BsuirWeekNum(this DateTime date)
        {
            const int magic = 2;
            return (date.DayOfYear / 7 + magic) % 4 + 1;
        }

        internal static bool FilterSubgroup(int filter, int subgroup)
        {
            return (filter == 0) || (subgroup == 0) || (subgroup == filter);
        }
    }
}
