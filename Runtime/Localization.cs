using System;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;

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
        public static async UniTask LoadAsync()
        {
            foreach (var info in Settings.SheetInfos)
            {
                if (!_storage.ContainsKey(info.name))
                    _storage.Add(info.name, await LocalizationLoader.LoadSheetAsync(CurrentLanguage, info.name));
            }
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
            foreach (var sheets in _storage)
            {
                if (sheets.Value.TryGetValue(key, out var result))
                    return result;
            }
            return $"#!#{key}#!#";
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