using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace IngameScript
{
    public class ConfigHandler
    {
        private MyIni Ini;
        private readonly string Tag;
        public Dictionary<string, string> settings;

        public ConfigHandler(MyIni ini, string tag, string customData, Dictionary<string, string> defaultSettings)
        {
            Ini = ini;
            Tag = tag;
            settings = defaultSettings;
            Load(customData);
        }

        public void Load(string customData)
        {
            if (Ini.TryParse(customData))
            {
                List<MyIniKey> keys = new List<MyIniKey>();
                Ini.GetKeys(Tag, keys);
                foreach (MyIniKey key in keys)
                {
                    if (!settings.ContainsKey(key.Name))
                    {
                        continue;
                    }

                    string value = Ini.Get(key).ToString();
                    settings[key.Name] = value;
                }
            }
        }

        public string Save(string customData)
        {
            if (Ini.TryParse(customData))
            {
                foreach (string key in settings.Keys)
                {
                    Ini.Set(Tag, key, settings[key]);
                }
            } else {
                Ini.Clear();
                foreach (string key in settings.Keys)
                {
                    Ini.Set(Tag, key, settings[key]);
                }
            }
            return Ini.ToString();
        }

        public string GetSetting(string key, string defaultValue = null)
        {
            return settings.GetValueOrDefault(key, defaultValue);
        }

        public void SetSetting(string key, string value)
        {
            settings[key] = value;
        }
    }
}