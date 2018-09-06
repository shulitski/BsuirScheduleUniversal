﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Newtonsoft.Json;
using BsuirScheduleLib.BsuirApi;

namespace BsuirScheduleLib.BsuirApi.Schedule
{
    public static class Loader
    {
        private static readonly Dictionary<string, ScheduleResponse> cache = new Dictionary<string, ScheduleResponse>();
        private static StorageFolder LocalFolder => ApplicationData.Current.LocalFolder;
        private static ApplicationDataContainer LocalSettings => ApplicationData.Current.LocalSettings;

        private static string CachedGroups
        {
            get => LocalSettings.Values["cachedGroups"] as string;
            set => LocalSettings.Values["cachedGroups"] = value;
        }

        public static string[] CachedGroupsArray => CachedGroups?.Split(',');

        private static bool IsGroupCached(string group)
        {
            return CachedGroupsArray?.Contains(group) ?? false;
        }

        private static void Save(string group, ScheduleResponse scheduleResponse)
        {
            cache.Add(group, scheduleResponse);
        }

        public static async Task<ScheduleResponse> Load(string group)
        {
            if (cache.ContainsKey(group)) return cache[group];

            string json;
            string fileName = $"group_schedule_{group}.json";
            
            if(IsGroupCached(group))
            {
                StorageFile file = await LocalFolder.GetFileAsync(fileName);
                json = await FileIO.ReadTextAsync(file);
            }
            else
            {
                //Schedule not found
                string url = $"https://students.bsuir.by/api/v1/studentGroup/schedule?studentGroup={group}";
                json = await Utils.LoadString(url);
                StorageFile sampleFile = await LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(sampleFile, json);
                if(string.IsNullOrEmpty(CachedGroups))
                    CachedGroups += $"{group}";
                else
                    CachedGroups += $",{group}";
            }

            ScheduleResponse scheduleResponse = JsonConvert.DeserializeObject<ScheduleResponse>(json);
            Save(group, scheduleResponse);
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
    }
}
