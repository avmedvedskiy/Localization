using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.Networking;

namespace LocalizationPackage
{
    public class LocalizationEditorTools
    {
        private LocalizationSettings _settings;
        private readonly List<(string error, string language)> _errors = new();

        public IReadOnlyList<(string error, string language)> Errors => _errors;

        public LocalizationSettings LoadSettings()
        {
            if (_settings != null)
                return _settings;

            if (File.Exists(LocalizationSettings.SETTINGS_ASSET_PATH))
            {
                _settings = AssetDatabase.LoadAssetAtPath<LocalizationSettings>(LocalizationSettings
                    .SETTINGS_ASSET_PATH);
            }
            else
            {
                Debug.LogError("does not exist, Create new");
                LocalizationSettings newSettings = ScriptableObject.CreateInstance<LocalizationSettings>();

                if (!Directory.Exists(LocalizationSettings.ASSET_RESOURCES_PATH))
                {
                    Directory.CreateDirectory(LocalizationSettings.ASSET_RESOURCES_PATH);
                }

                AssetDatabase.CreateAsset(newSettings, LocalizationSettings.SETTINGS_ASSET_PATH);
                AssetDatabase.Refresh();

                _settings = AssetDatabase.LoadAssetAtPath<LocalizationSettings>(LocalizationSettings
                    .SETTINGS_ASSET_PATH);
            }

            return _settings;
        }

        public void ClearErrors()
        {
            _errors.Clear();
        }

        /// <summary>
        /// Can be used for autobuild system
        /// </summary>
        public void LoadAndUpdateLocalization()
        {
            UpdateLocalization(false);
        }

        public void UpdateLocalization(bool displayProgressBar)
        {
            LoadSettings();
            CreateOrClearAddressableAssetGroup();
            CreateLanguageFolder();
            foreach (var info in _settings.SheetInfos)
            {
                UpdateSheet(info, displayProgressBar);
            }
        }

        public void UpdateSheet(LocalizationSettings.SheetInfo info, bool displayProgressBar)
        {
            string url = $"{_settings.DocumentUrl}&gid={info.id}";
            var request = UnityWebRequest.Get(url);
            var async = request.SendWebRequest();

            Debug.Log("Start Loading " + info.name);

            while (!async.isDone)
            {
                if (displayProgressBar)
                {
                    EditorUtility.DisplayProgressBar("Loading", info.name, async.progress);
                    System.Threading.Thread.Sleep(200);
                }
            }

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("isNetworkError " + info.name);
            }
            else
            {
                var data = request.downloadHandler.text;
                Debug.Log("Start Parsing " + info.name);
                ParseData(data, info);
                Debug.Log("Complete Parsing " + info.name);
            }
        }

        public void SaveSettingsFile(SystemLanguage defaultLangCode, bool useSystemLang, string predefSheetTitle)
        {
            if (_settings == null)
            {
                _settings = (LocalizationSettings)ScriptableObject.CreateInstance(typeof(LocalizationSettings));
                string settingsPath = Path.GetDirectoryName(LocalizationSettings.SETTINGS_ASSET_PATH);
                Directory.CreateDirectory(settingsPath);
                if (!Directory.Exists(settingsPath))
                {
                    AssetDatabase.CreateAsset(_settings, LocalizationSettings.SETTINGS_ASSET_PATH);
                }
                else
                {
                    AssetDatabase.SaveAssets();
                }
            }

            _settings.DefaultLangCode = defaultLangCode;
            _settings.UseSystemLanguagePerDefault = useSystemLang;
            _settings.PredefinedSheetTitle = predefSheetTitle;

            EditorUtility.SetDirty(_settings);
        }

        Dictionary<string, Dictionary<string, string>> LoadPage(string data, string sheetTitle)
        {
            var lines = SplitLines(data)
                .Select(SplitTSVLine)
                .ToList();

            var languages = lines
                .First()
                .Skip(1)
                .ToList();

            var resultLanguages
                = languages.ToDictionary(x => x, _ => new Dictionary<string, string>());

            foreach (var line in lines
                         .Skip(1)
                         .Where(x => string.IsNullOrEmpty(x.First()) == false))
            {
                string key = line[0];
                for (int i = 1; i < line.Count; i++)
                {
                    var langName = languages[i - 1];
                    var langData = resultLanguages[langName];
                    var value = line[i].UnescapeXML();
                    if (string.IsNullOrEmpty(value))
                        AddError($"Empty Key {key} in sheet {sheetTitle} (lang={langName})", langName);

                    if (!langData.TryAdd(key, value))
                        AddError($"Duplicated Key {key} in sheet {sheetTitle} (lang={langName})", langName);
                }
            }

            return resultLanguages;
        }

        void AddError(string error, string language)
        {
            if (IsAvailableLanguage(language))
                _errors.Add((error, language));
        }

        void SaveToFiles(Dictionary<string, Dictionary<string, string>> page, LocalizationSettings.SheetInfo sheetTitle)
        {
            //Save the loaded data
            foreach (var languageData in page)
            {
                if (IsAvailableLanguage(languageData.Key))
                    SaveToFile(sheetTitle, languageData.Value, languageData.Key);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        bool IsAvailableLanguage(string language) => _settings.LanguageFilter.Exists(x => x.ToString() == language);

        void SaveToFile(LocalizationSettings.SheetInfo sheetInfo, Dictionary<string, string> languageData, string langCode)
        {
            LocalizationAsset asset = ScriptableObject.CreateInstance<LocalizationAsset>();
            asset.values = languageData
                .Select(x => new LocalizationAsset.LanguageData(x.Key, x.Value))
                .ToList();

            if (sheetInfo.addressableType != AddressableType.Resources)
            {
                var folderPath = $"{_settings.AddressablePath}/{langCode}";
                CreateAssetFile(sheetInfo.name, folderPath, asset);
                
                switch (sheetInfo.addressableType)
                {
                    case AddressableType.Resources:
                        break;
                    case AddressableType.PerFolder:
                        AddAssetToGroup(folderPath, sheetInfo.addressableGroup, langCode);
                        break;
                    case AddressableType.PerFile:
                        AddAssetToGroup($"{folderPath}/{sheetInfo.name}.asset", sheetInfo.addressableGroup, $"{langCode}/{sheetInfo.name}.asset");
                        break;
                }
            }
            else
            {
                var folderPath = $"{_settings.ResourcesPath}/{langCode}";
                CreateAssetFile(sheetInfo.name, folderPath, asset);
            }
        }

        private void CreateAssetFile(string sheetTitle, string folderPath, LocalizationAsset asset)
        {
            Directory.CreateDirectory(folderPath);
            var filePath = $"{folderPath}/{sheetTitle}.asset";
            AssetDatabase.CreateAsset(asset, filePath);
        }

        void ParseData(string data, LocalizationSettings.SheetInfo sheetInfo)
        {
            try
            {
                var loadedPage = LoadPage(data, sheetInfo.name);
                SaveToFiles(loadedPage, sheetInfo);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                AddError(e.Message, "Unknown");
            }
        }

        void AddAssetToGroup(string path, string groupName, string key = "")
        {
            if (string.IsNullOrEmpty(groupName))
            {
                Debug.LogError($"Addressable group can't be empty");
                return;
            }
            
            var group = AddressableAssetSettingsDefaultObject.Settings.FindGroup(groupName);

            var assetPathToGuid = AssetDatabase.AssetPathToGUID(path);
            var entry = AddressableAssetSettingsDefaultObject.Settings.CreateOrMoveEntry(assetPathToGuid, group);
            if (entry == null)
            {
                Debug.LogError($"Addressable : can't add {path} to group {groupName}");
            }
            else
            {
                if (!string.IsNullOrEmpty(key))
                    entry.address = key;
            }
        }

        private void CreateOrClearAddressableAssetGroup()
        {
            foreach (var groupName in _settings.SheetInfos
                         .Where(x=>!string.IsNullOrEmpty(x.addressableGroup))
                         .Select(x => x.addressableGroup)
                         .Distinct())
            {
                var group = AddressableAssetSettingsDefaultObject.Settings.FindGroup(groupName);
                if (!group)
                {
                    AddressableAssetSettingsDefaultObject.Settings.CreateGroup(groupName, false, false, true, null,
                        typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));
                }
                else
                {
                    //clear groups
                    var keys = group.entries.ToList();
                    foreach(var k in keys)
                        group.RemoveAssetEntry(k);
                }
                
            }
        }


        List<string> SplitLines(string data) => data.Split("\r\n").ToList();

        List<string> SplitTSVLine(string line)
        {
            return line
                .Split("\t")
                .Select(System.Security.SecurityElement.Escape)
                .ToList();
        }

        void CreateLanguageFolder()
        {
            CreateFolder(_settings.AddressablePath);
            CreateFolder(_settings.ResourcesPath);
        }

        void CreateFolder(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            Directory.CreateDirectory(path);
        }
    }
}