using System;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using UnityEngine.Pool;

namespace LocalizationPackage
{
    public static class Localization
    {
        public static event Action OnLanguageChanged;
        public static SystemLanguage CurrentLanguage { get; private set; }
        private static LocalizationSettings Settings => SettingsProvider.Settings;
        
        //maybe need to collapse into one dictionary
        private static readonly Dictionary<string, Dictionary<string, string>> _storage = new();

        static Localization()
        {
            CurrentLanguage = LanguageCodeStorage.GetLanguageCode();
            _storage.Add(Settings.PredefinedSheetTitle, LocalizationLoader.LoadDefaultSheet(CurrentLanguage));
        }

        /// <summary>
        /// Load all other sheets
        /// </summary>
        public static async UniTask LoadAllAsync()
        {
            foreach (var info in Settings.SheetInfos)
            {
                await LoadAsync(info.name);
            }
        }
        
        /// <summary>
        /// Load sheet
        /// </summary>
        public static async UniTask LoadAsync(string sheetName)
        {
            if (!_storage.ContainsKey(sheetName) && Settings.Contains(sheetName))
                _storage.Add(sheetName, await LocalizationLoader.LoadSheetAsync(CurrentLanguage, sheetName));
        }

        public static async UniTask SwitchLanguageAsync(SystemLanguage code)
        {
            LanguageCodeStorage.SetLanguageCode(code);
            CurrentLanguage = code;
            var keys = ListPool<string>.Get();
            foreach (var s in _storage)
            {
                keys.Add(s.Key);
            }
            _storage.Clear();
            foreach (var sheet in keys)
            {
                await LoadAsync(sheet);
            }
            ListPool<string>.Release(keys);
            OnLanguageChanged?.Invoke();
        }

        public static string Get(string key)
        {
            foreach (var sheets in _storage)
            {
                if (sheets.Value.TryGetValue(key, out var result))
                    return result;
            }
            return $"#!#{key}#!#";
        }

        public static string GetFormat(string key, object value) => string.Format(Get(key), value);
        public static string GetFormat(string key, object value1,object value2) => string.Format(Get(key), value1,value2);
        public static string GetFormat(string key, object value1,object value2, object value3) => string.Format(Get(key), value1,value2,value3);
        public static string GetFormat(string key, params object[] values) => string.Format(Get(key),values);

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