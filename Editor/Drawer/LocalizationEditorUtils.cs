using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;

namespace LocalizationPackage
{
    public static class LocalizationEditorUtils
    {
        private static LocalizationSettings _settings;
        private static List<string> _sheetList;
        private static readonly Dictionary<string, LocalizationAsset> _cacheLocalizationAssets =
            new Dictionary<string, LocalizationAsset>();

        public static LocalizationSettings Settings
        {
            get
            {
                if (_settings == null)
                    _settings = FindSettings();
                return _settings;
            }
        }

        public static List<string> SheetList
        {
            get
            {
                if(_sheetList == null)
                    _sheetList = Settings.sheetInfos.Select(x => x.name).ToList();
                return _sheetList;
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


        public static LocalizationAsset GetLanguageAsset(LanguageCode languageCode, string sheetTitle)
        {
            string fileName = $"{languageCode}_{sheetTitle}.asset";
            if (_cacheLocalizationAssets.ContainsKey(fileName))
                return _cacheLocalizationAssets[fileName];


            string assetFilePath = Settings.GetAssetFilePath(sheetTitle);
            var asset = AssetDatabase.LoadAssetAtPath<LocalizationAsset>($"{assetFilePath}{fileName}");
            _cacheLocalizationAssets.Add(fileName, asset);
            return asset;
        }


        public static string GetLocalizationText(LanguageCode languageCode, string sheetTitle, string key)
        {
            var asset = LocalizationEditorUtils.GetLanguageAsset(languageCode, sheetTitle);
            if (asset == null)
                return null;
            return asset.values.Find(x => x.key == key).value;
        }
        
    }
}