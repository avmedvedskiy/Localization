using System;
using UnityEngine;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;

#if USE_ASYNCTASK
using Cysharp.Threading.Tasks;
#endif
#if USE_ADDRESSABLES
using UnityEngine.AddressableAssets;
#endif

namespace LocalizationPackage
{
    public static class Localization
    {
        public static LanguageCode CurrentLanguage => _currentLanguage;

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
        private static LanguageCode _currentLanguage = LanguageCode.N;

        private static readonly Dictionary<string, Dictionary<string, string>> _currentEntrySheets =
            new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// Init with default loader
        /// </summary>
        public static void Init()
        {
            var code = GetDefaultLanguageCode();
            SwitchLanguage(code);
        }

#if USE_ASYNCTASK
        public static async UniTask InitAsync()
        {
            var code = GetDefaultLanguageCode();
            await SwitchLanguageAsync(code);
        }
#endif

        private static LanguageCode GetDefaultLanguageCode()
        {
            bool useSystemLanguagePerDefault = Settings.useSystemLanguagePerDefault;
            //ISO 639-1 (two characters). See: http://en.wikipedia.org/wiki/List_of_ISO_639-1_codes
            LanguageCode useLang = Settings.defaultLangCode;

            //See if we can use the last used language (playerprefs)
            string lastLang = PlayerPrefs.GetString(LAST_LANGUAGE_KEY, string.Empty);
            LanguageCode lastLangCode = LocalizationSettings.GetLanguageEnum(lastLang);

            if (!string.IsNullOrEmpty(lastLang) && IsLanguageAvailable(lastLangCode))
            {
                return LocalizationSettings.GetLanguageEnum(lastLang);
            }

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

            return useLang;
        }

        public static void SwitchLanguage(LanguageCode code, bool ignoreCurrent = false)
        {
            if (_currentLanguage == code && !ignoreCurrent)
                return;

            if (!IsLanguageAvailable(code))
            {
                Debug.LogError($"Could not switch from language {_currentLanguage} to {code}");
                if (_currentLanguage == LanguageCode.N)
                {
                    code = LanguageFilter[0];
                    Debug.LogError($"Switched to {_currentLanguage} instead");
                }
            }

            DoSwitch(code);
        }

#if USE_ASYNCTASK
        public static async UniTask SwitchLanguageAsync(LanguageCode code, bool ignoreCurrent = false)
        {
            if (_currentLanguage == code && !ignoreCurrent)
                return;

            if (!IsLanguageAvailable(code))
            {
                Debug.LogError($"Could not switch from language {_currentLanguage} to {code}");
                if (_currentLanguage == LanguageCode.N)
                {
                    code = LanguageFilter[0];
                    Debug.LogError($"Switched to {_currentLanguage} instead");
                }
            }

            await DoSwitchAsync(code);
        }
#endif

        private static bool IsLanguageAvailable(LanguageCode code)
        {
            return LanguageFilter.Contains(code);
        }

        private static void DoSwitch(LanguageCode newLang)
        {
            PlayerPrefs.SetString(LAST_LANGUAGE_KEY, newLang.ToString());

            _currentLanguage = newLang;
            _currentEntrySheets.Clear();

            foreach (var sheetTitle in Settings.sheetInfos)
            {
                LoadAndConvertFileAsset(_settings, _currentLanguage, sheetTitle.name, Convert);

                void Convert(LocalizationAsset localizationAsset)
                {
                    if (localizationAsset != null)
                        _currentEntrySheets[sheetTitle.name] =
                            localizationAsset.values.ToDictionary(x => x.key, y => y.value);
                }
            }

            OnLanguageSwitch();
        }

#if USE_ASYNCTASK
        private static async UniTask DoSwitchAsync(LanguageCode newLang)
        {
            PlayerPrefs.SetString(LAST_LANGUAGE_KEY, newLang.ToString());

            _currentLanguage = newLang;
            _currentEntrySheets.Clear();

            foreach (var sheetTitle in Settings.sheetInfos)
            {
                await LoadAndConvertFileAssetAsync(_settings, _currentLanguage, sheetTitle.name, Convert);

                void Convert(LocalizationAsset localizationAsset)
                {
                    if (localizationAsset != null)
                        _currentEntrySheets[sheetTitle.name] =
                            localizationAsset.values.ToDictionary(x => x.key, y => y.value);
                }
            }

            OnLanguageSwitch();
        }
#endif

#region LoadFilesRegion
        
        private static void LoadAndConvertFileAsset(LocalizationSettings settings, LanguageCode code, string sheetTitle,
            Action<LocalizationAsset> convertMethod)
        {
            if (sheetTitle == settings.predefSheetTitle || string.IsNullOrEmpty(settings.addressableGroup))
            {
                LocalizationAsset file = Resources.Load<LocalizationAsset>(
                    $"{settings.GetAssetFilePath(sheetTitle)}/{code}_{sheetTitle}.asset");
                convertMethod(file);
                Resources.UnloadAsset(file);
                return;
            }

#if USE_ADDRESSABLES
            var op = Addressables.LoadAssetAsync<LocalizationAsset>($"{code}_{sheetTitle}");
            LocalizationAsset adFile = op.WaitForCompletion();
            convertMethod(adFile);
            Addressables.Release(op);
#endif
        }

#if USE_ASYNCTASK
        private static async UniTask LoadAndConvertFileAssetAsync(LocalizationSettings settings,
            LanguageCode code, string sheetTitle,
            Action<LocalizationAsset> convertMethod)
        {
            if (sheetTitle == settings.predefSheetTitle || string.IsNullOrEmpty(settings.addressableGroup))
            {
                LocalizationAsset file = (LocalizationAsset)await Resources.LoadAsync<LocalizationAsset>(
                    $"{settings.GetAssetFilePath(sheetTitle)}/{code}_{sheetTitle}.asset");
                convertMethod(file);
                Resources.UnloadAsset(file);
                return;
            }

#if USE_ADDRESSABLES
            var op = Addressables.LoadAssetAsync<LocalizationAsset>($"{code}_{sheetTitle}");
            LocalizationAsset adFile = await op;
            convertMethod(adFile);
            Addressables.Release(op);
#endif
        }
#endif

#endregion


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