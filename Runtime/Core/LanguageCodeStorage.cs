using System;
using UnityEngine;

namespace LocalizationPackage
{
    internal static class LanguageCodeStorage
    {
        private const string LAST_LANGUAGE_KEY = "LastLanguage";
        private static LocalizationSettings Settings => SettingsProvider.Settings;

        public static void SetLanguageCode(SystemLanguage code)
        {
            if (IsLanguageAvailable(code) == false)
                throw new SystemException($"Is Language {code} not supported");
            
            PlayerPrefs.SetString(LAST_LANGUAGE_KEY, code.ToString());
        }
        
        public static SystemLanguage GetLanguageCode()
        {
            var useSystemLanguagePerDefault = Settings.UseSystemLanguagePerDefault;
            var defaultLangCode = Settings.DefaultLangCode;

            if (PlayerPrefs.HasKey(LAST_LANGUAGE_KEY))
            {
                var lastLang = (SystemLanguage)PlayerPrefs.GetInt(LAST_LANGUAGE_KEY, (int)SystemLanguage.English);
                if (IsLanguageAvailable(lastLang))
                {
                    return lastLang;
                }
            }
            
            if (useSystemLanguagePerDefault)
            {
                SystemLanguage localLang = Application.systemLanguage;
                if (IsLanguageAvailable(localLang))
                {
                    return localLang;
                }
            }

            return defaultLangCode;
        }

        
        private static bool IsLanguageAvailable(SystemLanguage code) => Settings.LanguageFilter.Contains(code);
        
    }
}