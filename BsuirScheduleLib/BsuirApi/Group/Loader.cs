using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Windows.Storage;

namespace BsuirScheduleLib.BsuirApi.Group
{
    public class Loader
    {
        private static ApplicationDataContainer LocalSettings => ApplicationData.Current.LocalSettings;
        private static StorageFolder CacheFolder => ApplicationData.Current.LocalCacheFolder;

        private static bool IsCachedGroups
        {
            get => (LocalSettings.Values["IsCachedGroup"] as bool?).GetValueOrDefault(false);
            set => LocalSettings.Values["IsCachedGroup"] = value;
        }

        private static List<Group> _cachedGroups;

        public static async Task<List<Group>> Get()
        {
            if (_cachedGroups == null)
            {
                if (IsCachedGroups)
                {
                    StorageFile file = await CacheFolder.GetFileAsync("Groups.json");
                    var json = await FileIO.ReadTextAsync(file);
                    _cachedGroups = JsonConvert.DeserializeObject<List<Group>>(json);
                }
                else
                {
                    await UpdateCache();
                }
            }
            return _cachedGroups;
        }

        public static async Task UpdateCache(Action<List<Group>> OnUpdate = null)
        {
            var json = await Utils.LoadString(Constants.groupsUrl);
            StorageFile groupsFile = await CacheFolder.CreateFileAsync("Groups.json", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(groupsFile, json);
            _cachedGroups = JsonConvert.DeserializeObject<List<Group>>(json);
            IsCachedGroups = true;
            OnUpdate?.Invoke(_cachedGroups);
        }
    }
}
