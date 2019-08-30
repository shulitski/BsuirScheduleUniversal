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

        private static string CachedSchedules
        {
            get => LocalSettings.Values["cachedSchedules"] as string;
            set => LocalSettings.Values["cachedSchedules"] = value;
        }

        public static IEnumerable<string> CachedSchedulesArray => CachedSchedules?.Split(',');

        private static bool IsScheduleCached(string name)
        {
            return CachedSchedulesArray?.Contains(name) ?? false;
        }

        private static async Task SaveToFile(string json, string name)
        {
            string fileName = $"schedule_{name}.json";
            StorageFile sampleFile = await LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(sampleFile, json);
            if (string.IsNullOrEmpty(CachedSchedules) || !CachedSchedules.Contains(name))
            {
                if (string.IsNullOrEmpty(CachedSchedules))
                    CachedSchedules += $"{name}";
                else
                    CachedSchedules += $",{name}";
            }
        }

        private static void Save(string name, ScheduleResponse scheduleResponse)
        {
            if (cache.ContainsKey(name))
                cache.Remove(name);
            cache.Add(name, scheduleResponse);
        }

        public static async Task<ScheduleResponse> Load(ScheduleQuery query, bool allowCache = true)
        {
            if (allowCache && cache.ContainsKey(query.Value))
                return cache[query.Value];

            _loading = true;
            string json;
            string fileName = $"schedule_{query.Value}.json";

            ScheduleResponse scheduleResponse = null;
            if (allowCache && IsScheduleCached(query.Value))
            {
                StorageFile file = await LocalFolder.GetFileAsync(fileName);
                json = await FileIO.ReadTextAsync(file);
                scheduleResponse = JsonConvert.DeserializeObject<ScheduleResponse>(json);
            }
            else
            {
                //Schedule not found
                var format = query.IsGroup ? Constants.studentScheduleFormat : Constants.employeeScheduleFormat;
                string url = string.Format(format, query.Value);
                json = await Utils.LoadString(url);
                scheduleResponse = JsonConvert.DeserializeObject<ScheduleResponse>(json);
                if(scheduleResponse != null)
                    await SaveToFile(json, query.Value);
            }
            if (scheduleResponse == null)
                throw new ScheduleLoadingException();

            Save(query.Value, scheduleResponse);

            var scheduleDate = DateTime.ParseExact(scheduleResponse.todayDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            Utils.SetWeekCalculationBase(scheduleDate, scheduleResponse.currentWeekNumber);
            _loading = false;
            return scheduleResponse;
        }

        public static async Task<List<Pair>> LoadPairs(ScheduleQuery query, DateTime day, int subgroup)
        {
            var response = await Load(query);

            var examDates = response.examSchedules.Select((obj) => obj.GetDate());
            examDates = examDates.Any() ? examDates : null;
            var minExamDate = examDates?.Min();

            Schedule schedule;

            if(examDates != null && minExamDate <= day)
            {
                schedule = response.examSchedules.Find(s => s.GetDate() == day);

                if (schedule == null)
                    return new List<Pair>();

                return schedule.schedule;
            }
            else
            {
                schedule = response.schedules.Find(s => Utils.StringToDayOfWeek(s.weekday) == day.DayOfWeek);

                return schedule?.schedule?.FindAll(pair =>
                Utils.FilterSubgroup(subgroup, pair.numSubgroup)
                && pair.weekNumber.Contains(day.BsuirWeekNum()));
            }
        }

        public static async Task<List<Pair>> LoadPairsFull(ScheduleQuery query, DayOfWeek day, int subgroup)
        {
            var response = await Load(query);

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
                var name = (string) state;
                var lastUpdate = await LastUpdate.Loader.Load(name);
                var schedule = await Load(new ScheduleQuery { Value = name });
                var updateDate =
                    DateTime.ParseExact(lastUpdate.lastUpdateDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                var scheduleDate = DateTime.ParseExact(schedule.todayDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                if (updateDate <= scheduleDate) return;

                var newSchedule = await Load(new ScheduleQuery { Value = name }, false);
                _onScheluleUpdated(newSchedule);
            }
            catch (Exception)
            {
                //Ignore
            }
            
        }

        public static void AddScheduleUpdateListener(Action<ScheduleResponse> onScheduleUpdated, string name)
        {
            _onScheluleUpdated = onScheduleUpdated;
            _updateTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            _updateTimer = new Timer(CheckScheduleUpdate, name, 10000, 10000);
        }

        public static void RemoveScheduleUpdateListener()
        {
            _onScheluleUpdated = null;
        }

        public static async void DeletePair(Pair pair)
        {
            string name = null;
            foreach(var cacheEntry in cache)
            {
                foreach (var schedule in cacheEntry.Value.schedules)
                {
                    if (schedule.schedule.Contains(pair))
                    {
                        schedule.schedule.Remove(pair);
                        name = cacheEntry.Key;
                        break;
                    }
                }
            }
            if (name == null)
                return;

            if (IsScheduleCached(name))
            {
                string fileName = $"schedule_{name}.json";
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
                await SaveToFile(json, name);
            }
        }

        public static async Task DeleteSchedule(string name)
        {
            cache.Remove(name);

            if (IsScheduleCached(name))
            {
                string fileName = $"schedule_{name}.json";
                StorageFile file = await LocalFolder.GetFileAsync(fileName);
                await file.DeleteAsync();
                string newCachedScheduless = "";
                foreach(var cachedSchedule in CachedSchedulesArray)
                {
                    if(cachedSchedule != name)
                    {
                        if(newCachedScheduless == "")
                            newCachedScheduless += cachedSchedule;
                        else
                            newCachedScheduless += "," + cachedSchedule;
                    }
                }
                CachedSchedules = (newCachedScheduless != "") ? newCachedScheduless : null;
            }
        }
    }
}
