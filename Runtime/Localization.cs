using System;
using UnityEngine;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace LocalizationPackage
{
    public static class Localization
    {
        public static event Action OnLanguageChanged;
        public static SystemLanguage CurrentLanguage => _currentLanguage;

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

        private static List<SystemLanguage> LanguageFilter => Settings.LanguageFilter;
        private static SystemLanguage _currentLanguage = SystemLanguage.English;

        private static readonly Dictionary<string, Dictionary<string, string>> _currentEntrySheets = new();


        public static async UniTask InitAsync()
        {
            var code = GetDefaultLanguageCode();
            await SwitchLanguageAsync(code);
        }

        private static SystemLanguage GetDefaultLanguageCode()
        {
            bool useSystemLanguagePerDefault = Settings.UseSystemLanguagePerDefault;
            SystemLanguage useLang = Settings.DefaultLangCode;
            
            string lastLang = PlayerPrefs.GetString(LAST_LANGUAGE_KEY, string.Empty);
            SystemLanguage lastLangCode = LocalizationSettings.GetLanguageEnum(lastLang);

            if (!string.IsNullOrEmpty(lastLang) && IsLanguageAvailable(lastLangCode))
            {
                return LocalizationSettings.GetLanguageEnum(lastLang);
            }
            
            if (useSystemLanguagePerDefault)
            {
                SystemLanguage localLang = Application.systemLanguage;
                if (IsLanguageAvailable(localLang))
                {
                    useLang = localLang;
                }
            }

            return useLang;
        }

        public static async UniTask SwitchLanguageAsync(SystemLanguage code, bool ignoreCurrent = false)
        {
            if (_currentLanguage == code && !ignoreCurrent)
                return;

            if (!IsLanguageAvailable(code))
            {
                Debug.LogError($"Could not switch from language {_currentLanguage} to {code}");
                if (_currentLanguage == SystemLanguage.English)
                {
                    code = LanguageFilter[0];
                    Debug.LogError($"Switched to {_currentLanguage} instead");
                }
            }

            await DoSwitchAsync(code);
        }

        private static bool IsLanguageAvailable(SystemLanguage code)
        {
            return LanguageFilter.Contains(code);
        }

        private static async UniTask DoSwitchAsync(SystemLanguage newLang)
        {
            PlayerPrefs.SetString(LAST_LANGUAGE_KEY, newLang.ToString());

            _currentLanguage = newLang;
            _currentEntrySheets.Clear();

            foreach (var sheetTitle in Settings.SheetInfos)
            {
                await LoadAndConvertAsync(_settings, _currentLanguage, sheetTitle.name);
            }

            OnLanguageChanged?.Invoke();
        }


        private static async UniTask LoadAndConvertAsync(
            LocalizationSettings settings,
            SystemLanguage code,
            string sheetTitle)
        {
            if (sheetTitle == settings.PredefSheetTitle || string.IsNullOrEmpty(settings.AddressableGroup))
            {
                await LoadFromResources(settings, code, sheetTitle);
            }
            else
            {
                await LoadFromAddressables(code, sheetTitle);
            }
        }

        private static async UniTask LoadFromAddressables(SystemLanguage code, string sheetTitle)
        {
            var op = Addressables.LoadAssetAsync<LocalizationAsset>($"{code}_{sheetTitle}");
            var file = await op.ToUniTask();
            ConvertAsset(file, sheetTitle);
            Addressables.Release(op);
        }

        private static async UniTask LoadFromResources(
            LocalizationSettings settings,
            SystemLanguage code,
            string sheetTitle)
        {
            var file = (LocalizationAsset)await Resources.LoadAsync<LocalizationAsset>(
                $"{settings.GetAssetFilePath(sheetTitle)}/{code}_{sheetTitle}.asset").ToUniTask();
            ConvertAsset(file, sheetTitle);
            Resources.UnloadAsset(file);
        }

        private static void ConvertAsset(LocalizationAsset localizationAsset, string name)
        {
            if (localizationAsset != null)
                _currentEntrySheets[name] =
                    localizationAsset.values.ToDictionary(x => x.key, y => y.value);
        }

        public static string Get(string key)
        {
            if (_currentEntrySheets == null || _currentEntrySheets.Count == 0)
                return $"#!#{key}#!#";

            return Get(key, Settings.SheetInfos[0].name);
        }


        public static string Get(string key, string sheetTitle)
        {
            if (Has(key, sheetTitle))
            {
                return _currentEntrySheets[sheetTitle][key];
            }

            if (Has(key, Settings.PredefSheetTitle))
            {
                return _currentEntrySheets[Settings.PredefSheetTitle][key];
            }

            return $"#!#{key}#!#";
        }

        public static bool Has(string key)
        {
            return Has(key, Settings.SheetInfos[0].name);
        }

        private static bool Has(string key, string sheetTitle)
        {
            if (_currentEntrySheets == null || !_currentEntrySheets.ContainsKey(sheetTitle))
                return false;
            return _currentEntrySheets[sheetTitle].ContainsKey(key);
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void CleanUpFastMode()
        {
            var instanceField =
                typeof(Localization).GetField("_currentLanguage",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            instanceField?.SetValue(null, SystemLanguage.English);
        }
#endif
    }
}