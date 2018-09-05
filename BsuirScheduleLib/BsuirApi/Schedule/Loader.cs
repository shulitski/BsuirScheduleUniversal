using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using BsuirScheduleLib.BsuirApi;

namespace BsuirScheduleLib.BsuirApi.Schedule
{
    public static class Loader
    {
        private static Dictionary<string, ScheduleResponse> cache = new Dictionary<string, ScheduleResponse>();

        private static void Save(string group, ScheduleResponse scheduleResponse)
        {
            cache.Add(group, scheduleResponse);
        }

        public static ScheduleResponse Load(string group)
        {
            if (cache.ContainsKey(group)) return cache[group];

            string url = $"https://students.bsuir.by/api/v1/studentGroup/schedule?studentGroup={@group}";
            string json =
                Utils.LoadString(url);
            ScheduleResponse scheduleResponse = JsonConvert.DeserializeObject<ScheduleResponse>(json);
            Save(group, scheduleResponse);
            return scheduleResponse;
        }

        public static List<Pair> LoadPairs(string group, DateTime day, int subgroup)
        {
            var response = Load(group);
            var schedule = response.schedules.Find(s => Utils.StringToDayOfWeek(s.weekday) == day.DayOfWeek);
            return schedule?.schedule?.FindAll(pair => 
                Utils.FilterSubgroup(subgroup, pair.numSubgroup)
                && pair.weekNumber.Contains(day.BsuirWeekNum()));
        }
    }
}
