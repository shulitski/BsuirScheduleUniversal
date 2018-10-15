using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Newtonsoft.Json;
using BsuirScheduleLib.BsuirApi;
using System.Globalization;
using System.Threading;

namespace BsuirScheduleLib.BsuirApi.Schedule
{
    public static class Loader
    {
        private static readonly Dictionary<string, ScheduleResponse> cache = new Dictionary<string, ScheduleResponse>();
        private static StorageFolder LocalFolder => ApplicationData.Current.LocalFolder;
        private static ApplicationDataContainer LocalSettings => ApplicationData.Current.LocalSettings;
        private static Action<ScheduleResponse> _onScheluleUpdated;
        private static Timer _updateTimer;
        private static bool _loading;

        private static string CachedGroups
        {
            get => LocalSettings.Values["cachedGroups"] as string;
            set => LocalSettings.Values["cachedGroups"] = value;
        }

        public static IEnumerable<string> CachedGroupsArray => CachedGroups?.Split(',');

        private static bool IsGroupCached(string group)
        {
            return CachedGroupsArray?.Contains(group) ?? false;
        }

        private static async Task SaveToFile(string json, string group)
        {
            string fileName = $"group_schedule_{group}.json";
            StorageFile sampleFile = await LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(sampleFile, json);
            if (string.IsNullOrEmpty(CachedGroups) || !CachedGroups.Contains(group))
            {
                if (string.IsNullOrEmpty(CachedGroups))
                    CachedGroups += $"{group}";
                else
                    CachedGroups += $",{group}";
            }
        }

        private static void Save(string group, ScheduleResponse scheduleResponse)
        {
            if (cache.ContainsKey(group))
                cache.Remove(group);
            cache.Add(group, scheduleResponse);
        }

        public static async Task<ScheduleResponse> Load(string group, bool allowCache = true)
        {
            if (allowCache && cache.ContainsKey(group))
                return cache[group];

            _loading = true;
            string json;
            string fileName = $"group_schedule_{group}.json";
            
            if(allowCache && IsGroupCached(group))
            {
                StorageFile file = await LocalFolder.GetFileAsync(fileName);
                json = await FileIO.ReadTextAsync(file);
            }
            else
            {
                //Schedule not found
                string url = $"https://students.bsuir.by/api/v1/studentGroup/schedule?studentGroup={group}";
                json = await Utils.LoadString(url);
                await SaveToFile(json, group);
            }

            ScheduleResponse scheduleResponse = JsonConvert.DeserializeObject<ScheduleResponse>(json);
            Save(group, scheduleResponse);

            var scheduleDate = DateTime.ParseExact(scheduleResponse.todayDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            Utils.SetWeekCalculationBase(scheduleDate, scheduleResponse.currentWeekNumber);
            _loading = false;
            return scheduleResponse;
        }

        public static async Task<List<Pair>> LoadPairs(string group, DateTime day, int subgroup)
        {
            var response = await Load(group);
            var schedule = response.schedules.Find(s => Utils.StringToDayOfWeek(s.weekday) == day.DayOfWeek);
            return schedule?.schedule?.FindAll(pair => 
                Utils.FilterSubgroup(subgroup, pair.numSubgroup)
                && pair.weekNumber.Contains(day.BsuirWeekNum()));
        }

        public static async Task<List<Pair>> LoadPairsFull(string group, DayOfWeek day, int subgroup)
        {
            var response = await Load(group);
            var schedule = response.schedules.Find(s => Utils.StringToDayOfWeek(s.weekday) == day);
            var pairs = schedule?.schedule?.FindAll(pair => Utils.FilterSubgroup(subgroup, pair.numSubgroup));
            pairs?.Sort((p1, p2) =>
            {
                DateTime d1 = DateTime.ParseExact(p1.startLessonTime, "HH:mm", CultureInfo.InvariantCulture);
                DateTime d2 = DateTime.ParseExact(p2.startLessonTime, "HH:mm", CultureInfo.InvariantCulture);
                if (d1 < d2) return -1;
                if (d1 > d2) return 1;
                return 0;
            });
            return pairs;
        }

        private static async void CheckScheduleUpdate(object state)
        {
            try
            {
                if(_loading)
                    return;
                var group = (string) state;
                var lastUpdate = await LastUpdate.Loader.Load(group);
                var schedule = await Load(group);
                var updateDate =
                    DateTime.ParseExact(lastUpdate.lastUpdateDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                var scheduleDate = DateTime.ParseExact(schedule.todayDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                if (updateDate <= scheduleDate) return;

                var newSchedule = await Load(group, false);
                _onScheluleUpdated(newSchedule);
            }
            catch (Exception)
            {
                //Ignore
            }
            
        }

        public static void AddScheduleUpdateListener(Action<ScheduleResponse> onScheduleUpdated, string group)
        {
            _onScheluleUpdated = onScheduleUpdated;
            _updateTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            _updateTimer = new Timer(CheckScheduleUpdate, group, 10000, 10000);
        }

        public static async void DeletePair(Pair pair)
        {
            string group = null;
            foreach(var cacheEntry in cache)
            {
                foreach (var schedule in cacheEntry.Value.schedules)
                {
                    if (schedule.schedule.Contains(pair))
                    {
                        schedule.schedule.Remove(pair);
                        group = cacheEntry.Key;
                        break;
                    }
                }
            }
            if (group == null)
                return;

            if (IsGroupCached(group))
            {
                string fileName = $"group_schedule_{group}.json";
                StorageFile file = await LocalFolder.GetFileAsync(fileName);
                string json = await FileIO.ReadTextAsync(file);
                ScheduleResponse scheduleResponse = JsonConvert.DeserializeObject<ScheduleResponse>(json);
                foreach (var schedule in scheduleResponse.schedules)
                {
                    Pair pairToDelete = null;
                    foreach(var filePair in schedule.schedule)
                    {
                        if (pair.Equals(filePair))
                        {
                            pairToDelete = filePair;
                            break;
                        }
                    }
                    schedule.schedule.Remove(pair);
                }
                json = JsonConvert.SerializeObject(scheduleResponse);
                await SaveToFile(json, group);
            }
        }
    }
}
