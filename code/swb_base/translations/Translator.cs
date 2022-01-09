using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sandbox;

namespace SWB_Base.Translations
{
    public class Translator
    {
        private static Translator _instance;
        private static readonly object _lock = new object();

        public Language ActiveLanguage;

        public static Translator GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new Translator();
                        LoadLanguage();
                    }
                }
            }

            return _instance;
        }

        private static void LoadLanguage()
        {
            var langCode = GetLanguageCode();
            var basePath = "code/swb_base/translations/languages/";
            var filePath = basePath + langCode + ".json";
            var baseFileSystem = FileSystem.Mounted;

            // Language not found
            if (!baseFileSystem.FileExists(filePath))
            {
                filePath = basePath + "en.json";
            }

            _instance.ActiveLanguage = baseFileSystem.ReadJson<Language>(filePath);
        }

        public static string GetLanguageCode()
        {
            // Waiting for feature request: https://github.com/Facepunch/sbox-issues/issues/1462
            return "en";
        }

        public string Translate(string key, string arg1 = null, string arg2 = null)
        {
            string translation;
            if (ActiveLanguage.Translations.TryGetValue(key, out translation))
            {
                return string.Format(translation, arg1, arg2);
            }

            // Translation not found
            return key;
        }
    }
}
