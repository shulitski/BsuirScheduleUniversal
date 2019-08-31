using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Windows.Storage;

namespace BsuirScheduleLib.BsuirApi.Employee
{
    public class Loader
    {

        private static ApplicationDataContainer LocalSettings => ApplicationData.Current.LocalSettings;
        private static StorageFolder CacheFolder => ApplicationData.Current.LocalCacheFolder;

        private static bool IsCachedEmployees
        {
            get => (LocalSettings.Values["IsCachedEmployees"] as bool?).GetValueOrDefault(false);
            set => LocalSettings.Values["IsCachedEmployees"] = value;
        }

        private static List<Employee> _cachedEmployees;

        public static async Task<List<Employee>> Get()
        {
            if (_cachedEmployees == null)
            {
                if (IsCachedEmployees)
                {
                    StorageFile file = await CacheFolder.GetFileAsync("Employees.json");
                    var json = await FileIO.ReadTextAsync(file);
                    _cachedEmployees = JsonConvert.DeserializeObject<List<Employee>>(json);
                }
                else
                {
                    await UpdateCache();
                }
            }
            return _cachedEmployees;
        }

        public static async Task UpdateCache(Action<List<Employee>> OnUpdate = null)
        {
            var json = await Utils.LoadString(Constants.employeesUrl);
            StorageFile employeesFile = await CacheFolder.CreateFileAsync("Employees.json", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(employeesFile, json);
            _cachedEmployees = JsonConvert.DeserializeObject<List<Employee>>(json);
            IsCachedEmployees = true;
            OnUpdate?.Invoke(_cachedEmployees);
        }
    }
}
