using UnityEngine;
using System.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LocalizationPackage
{
    public static class Localization
    {
        private const string LAST_LANGUAGE_KEY = "LastLanguage";

        //For settings, see TOOLS->LOCALIZATION
        private static LocalizationSettings _settings;

        private static LocalizationSettings Settings
        {
            get
            {
                //automatically load settings from resources if the pointer is null (to avoid null-ref-exceptions!)
                if (_settings == null)
                {
                    _settings = Resources.Load<LocalizationSettings>(LocalizationSettings
                        .SETTINGS_ASSET_RESOURCES_PATH);
                }

                return _settings;
            }
        }

        private static List<LanguageCode> LanguageFilter => Settings.languageFilter;
        private static ILocalizationLoader _loader;
        private static LanguageCode _currentLanguage = LanguageCode.N;
        private static Dictionary<string, Dictionary<string, string>> _currentEntrySheets;

        /// <summary>
        /// Init with default loader
        /// </summary>
        public static void Init()
        {
            Init(new ResourcesLoader());
        }

        /// <summary>
        /// Set loader for loading assets from resources or bundles, can be used own loader
        /// </summary>
        /// <param name="loader"></param>
        public static void Init(ILocalizationLoader loader)
        {
            _loader = loader;

            bool useSystemLanguagePerDefault = Settings.useSystemLanguagePerDefault;
            //ISO 639-1 (two characters). See: http://en.wikipedia.org/wiki/List_of_ISO_639-1_codes
            LanguageCode useLang = Settings.defaultLangCode;

            //See if we can use the last used language (playerprefs)
            string lastLang = PlayerPrefs.GetString(LAST_LANGUAGE_KEY, string.Empty);
            LanguageCode lastLangCode = LocalizationSettings.GetLanguageEnum(lastLang);

#if UNITY_EDITOR
            if (!Application.isPlaying)
                SwitchLanguage(useLang);
#endif

            if (!string.IsNullOrEmpty(lastLang) && IsLanguageAvailable(lastLangCode))
            {
                SwitchLanguage(LocalizationSettings.GetLanguageEnum(lastLang));
            }
            else
            {
                //See if we can use the local system language: if so, we overwrite useLang
                if (useSystemLanguagePerDefault)
                {
                    //Attempt 1. Use Unity system lang
                    LanguageCode localLang = Application.systemLanguage.ToLanguageCode();
                    if (localLang == LanguageCode.N)
                    {
                        //Attempt 2. Otherwise try .NET cultureinfo; doesnt work on mobile systems
                        // Also returns EN (EN-US) on my dutch pc (interface is english but Country&region is Netherlands)
                        //BUGGED IN MONO? See: http://forum.unity3d.com/threads/5452-Getting-user-s-language-preference
                        string langIso = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
                        if (langIso != "iv") //IV = InvariantCulture
                            localLang = LocalizationSettings.GetLanguageEnum(langIso);
                    }

                    if (IsLanguageAvailable(localLang))
                    {
                        useLang = localLang;
                    }
                    else
                    {
                        //hack for PT_BR
                        //We dont have the local lang..try a few common exceptions
                        if (localLang == LanguageCode.PT)
                        {
                            //  we didn't have PT, can we show PT_BR instead?
                            if (LanguageFilter.Contains(LanguageCode.PT_BR))
                            {
                                useLang = LanguageCode.PT_BR;
                            }
                        }
                    }
                }

                SwitchLanguage(useLang);
            }
        }

        public static void SwitchLanguage(LanguageCode code, bool ignoreCurrent = false)
        {
            if (_currentLanguage == code && !ignoreCurrent)
                return;

            if (IsLanguageAvailable(code))
            {
                DoSwitch(code);
            }
            else
            {
                Debug.LogError($"Could not switch from language {_currentLanguage} to {code}");
                if (_currentLanguage == LanguageCode.N)
                {
                    DoSwitch(LanguageFilter[0]);
                    Debug.LogError($"Switched to {_currentLanguage} instead");
                }
            }
        }

        private static bool IsLanguageAvailable(LanguageCode code)
        {
            return LanguageFilter.Contains(code);
        }

        private static void DoSwitch(LanguageCode newLang)
        {
            PlayerPrefs.SetString(LAST_LANGUAGE_KEY, newLang.ToString());

            _currentLanguage = newLang;
            _currentEntrySheets = new Dictionary<string, Dictionary<string, string>>();

            foreach (var sheetTitle in Settings.sheetInfos)
            {
                var asset = _loader.GetLanguageFileAsset(_settings, _currentLanguage, sheetTitle.name);
                if (asset != null)
                    _currentEntrySheets[sheetTitle.name] = asset.values.ToDictionary(x => x.key, y => y.value);
            }

            OnLanguageSwitch();
        }


        private static void OnLanguageSwitch()
        {
            Component[] components;

            var mainCanvas = GameObject.FindWithTag("MainCanvas");
            if (mainCanvas != null)
            {
                components = mainCanvas.GetComponentsInChildren(typeof(MonoBehaviour), true);
            }
            else
            {
                Debug.LogError(
                    "Not Found MainCanvas Tag GameObject, it will be need for better performance when change localization");
                return;
            }

            if (components != null)
            {
                for (int i = 0; i < components.Length; i++)
                {
                    var c = components[i];
                    if (c != null && c is ILocalize localize)
                        localize.OnLanguageSwitch();
                }
            }
        }


        public static LanguageCode CurrentLanguage()
        {
            return _currentLanguage;
        }

        public static string Get(string key)
        {
            if (_currentEntrySheets == null || _currentEntrySheets.Count == 0)
                return $"#!#{key}#!#";

            return Get(key, Settings.sheetInfos[0].name);
        }


        public static string Get(string key, string sheetTitle)
        {
            if (Has(key, sheetTitle))
            {
                return _currentEntrySheets[sheetTitle][key];
            }

            if (Has(key, Settings.predefSheetTitle))
            {
                return _currentEntrySheets[Settings.predefSheetTitle][key];
            }

            return $"#!#{key}#!#";
        }

        public static bool Has(string key)
        {
            return Has(key, Settings.sheetInfos[0].name);
        }

        private static bool Has(string key, string sheetTitle)
        {
            if (_currentEntrySheets == null || !_currentEntrySheets.ContainsKey(sheetTitle))
                return false;
            return _currentEntrySheets[sheetTitle].ContainsKey(key);
        }
    }
}