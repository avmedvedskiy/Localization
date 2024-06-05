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
        public static SystemLanguage CurrentLanguage { get; private set; }
        private static LocalizationSettings Settings => SettingsProvider.Settings;

        private static readonly Dictionary<string, Dictionary<string, string>> _storage = new();

        /// <summary>
        /// Initialization with predefined sheet, and set current language
        /// </summary>
        public static void Init()
        {
            CurrentLanguage = LanguageCodeStorage.GetLanguageCode();
            _storage.Add(Settings.PredefSheetTitle, LocalizationLoader.LoadDefaultSheet(CurrentLanguage));
        }

        /// <summary>
        /// Load all other sheets
        /// </summary>
        public static async UniTask LoadAsync()
        {
            foreach (var info in Settings.SheetInfos)
            {
                if (!_storage.ContainsKey(info.name))
                    _storage.Add(info.name, await LocalizationLoader.LoadSheetAsync(CurrentLanguage, info.name));
            }
        }

        /// <summary>
        /// initialization and load all sheets, if using remote settings use Init and Load separately
        /// </summary>
        public static async UniTask InitAsync()
        {
            Init();
            await LoadAsync();
        }

        public static async UniTask SwitchLanguageAsync(SystemLanguage code)
        {
            LanguageCodeStorage.SetLanguageCode(code);
            CurrentLanguage = code;
            _storage.Clear();
            await LoadAsync();
            OnLanguageChanged?.Invoke();
        }


        public static string Get(string key)
        {
            if (_storage == null || _storage.Count == 0)
                return $"#!#{key}#!#";

            return Get(key, Settings.SheetInfos[0].name);
        }


        public static string Get(string key, string sheetTitle)
        {
            if (Has(key, sheetTitle))
            {
                return _storage[sheetTitle][key];
            }

            if (Has(key, Settings.PredefSheetTitle))
            {
                return _storage[Settings.PredefSheetTitle][key];
            }

            return $"#!#{key}#!#";
        }

        public static bool Has(string key)
        {
            return Has(key, Settings.SheetInfos[0].name);
        }

        private static bool Has(string key, string sheetTitle)
        {
            if (_storage == null || !_storage.ContainsKey(sheetTitle))
                return false;
            return _storage[sheetTitle].ContainsKey(key);
        }
        
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void CleanUpFastMode()
        {
            var instanceField =
                typeof(Localization).GetField(nameof(_storage),
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            instanceField?.SetValue(null, new Dictionary<string, Dictionary<string, string>>());
        }
#endif
    }
}