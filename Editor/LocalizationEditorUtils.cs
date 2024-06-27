using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LocalizationPackage
{
    public static class LocalizationEditorUtils
    {
        private const string KEY_NOT_FOUND = "Key not found";
        private static LocalizationSettings _settings;
        private static List<string> _sheetList;
        private static readonly Dictionary<string, LocalizationAsset> _cacheLocalizationAssets = new();

        public static LocalizationSettings Settings
        {
            get
            {
                if (_settings == null)
                    _settings = FindSettings();
                return _settings;
            }
        }

        private static LocalizationSettings FindSettings()
        {
            var assets = AssetDatabase.FindAssets("t:LocalizationSettings");
            if (assets.Length != 0)
            {
                return AssetDatabase.LoadAssetAtPath<LocalizationSettings>(
                    AssetDatabase.GUIDToAssetPath(assets[0]));
            }

            return null;
        }


        private static LocalizationAsset GetLanguageAsset(SystemLanguage languageCode, string sheetTitle)
        {
            string fileName = $"{languageCode}/{sheetTitle}.asset";
            if (_cacheLocalizationAssets.TryGetValue(fileName, out var languageAsset) && languageAsset != null)
                return languageAsset;


            string assetFilePath = Settings.GetAssetFilePath(sheetTitle);
            var asset = AssetDatabase.LoadAssetAtPath<LocalizationAsset>($"{assetFilePath}/{fileName}");
            _cacheLocalizationAssets[fileName] = asset;
            return asset;
        }

        public static Dictionary<string, List<LocalizationAsset.LanguageData>> GetAllKeys()
        {
           return Settings.SheetInfos
                .Select(x => (x.name, GetLanguageAsset(Settings.EditorPreviewCode, x.name)))
                .ToDictionary(y => y.name, v => v.Item2.values);
        }

        public static string GetLocalizationText(string key)
        {
            foreach (var sheetInfo in Settings.SheetInfos)
            {
                var asset = GetLanguageAsset(Settings.EditorPreviewCode, sheetInfo.name);
                if (asset != null)
                {
                    var text = asset.values.Find(x => x.key == key).value;
                    if (!text.IsNullOrEmpty())
                        return text;
                }
            }

            return KEY_NOT_FOUND;
        }
    }
}