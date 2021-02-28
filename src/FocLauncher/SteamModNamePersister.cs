using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace FocLauncher.Mods
{
    public class SteamModNamePersister
    {
        private const string FileName = "SteamModNames.json";

        private readonly Dictionary<string, string> _modNames = new Dictionary<string, string>(); 

        private readonly object _syncObject = new object();
        private static SteamModNamePersister? _instance;
        public static SteamModNamePersister Instance => _instance ??= new SteamModNamePersister();

        public string FilePath { get; }

        private SteamModNamePersister()
        {
            FilePath = Path.Combine(LauncherConstants.ApplicationBasePath, FileName);

            if (!File.Exists(FilePath))
            {
                SetDefaultDictionary();
                WriteDataToFile();
            }
            else
            {
                var json = ReadFile();
                _modNames = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
        }

        public bool TryFind(string key, out string name)
        {
            name = default;
            if (_modNames.ContainsKey(key))
            {
                name = _modNames[key];
                return true;
            }
            return false;
        }

        public void AddModName(string id, string name)
        {
            if (_modNames.ContainsKey(id))
                _modNames[id] = name;
            else
                _modNames.Add(id, name);
        }

        public void Save()
        {
            WriteDataToFile();
        }

        private string ReadFile()
        {
            lock (_syncObject)
            {
                return File.ReadAllText(FilePath);
            }
        }


        private void WriteDataToFile()
        {
            var json = JsonConvert.SerializeObject(_modNames, Formatting.Indented);
            var bytes = Encoding.Default.GetBytes(json);
            lock (_syncObject)
            {
                using (var fs = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                    fs.Write(bytes, 0, bytes.Length);
            }
        }

        private void SetDefaultDictionary()
        {
            _modNames.Clear();
            _modNames.Add("1125571106", "Thrawn's Revenge: Imperial Civil War");
            _modNames.Add("1129810972", "Republic at War");
            _modNames.Add("1235783994", "Phoenix Rising");
            _modNames.Add("1397421866", "Awakening of the Rebellion");
            _modNames.Add("1126673817", "The Clone Wars");
            _modNames.Add("1125764259", "Star Wars Battlefront Commander");
            _modNames.Add("1172650432", "Yodenmod2018");
            _modNames.Add("1130150761", "Old Republic at War (Classic)");
            _modNames.Add("1193084624", "Awakening of the Rebellion 2.6");
            _modNames.Add("1156943126", "Ultimate Galactic Conquest Custom Edition");
            _modNames.Add("1197705959", "Star Wars Return of the Clones");
            _modNames.Add("1382582782", "Absolute Chaos");
            _modNames.Add("1301047166", "Nojembre's Republic at War Submod");
            _modNames.Add("1126880602", "Stargate - Empire at War: Pegasus Chronicles");
            _modNames.Add("1241979729", "FOC Alliance Rebellion");
            _modNames.Add("1976399102", "Fall of the Republic");
            _modNames.Add("1770851727", "Empire At War Remake");
        }

        public static SteamModNamePersister CreateInstance()
        {
            return Instance;
        }
    }
}