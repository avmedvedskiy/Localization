using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
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
                ParseData(data, info.name);
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
                    var value = line[i];
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

        void SaveToFiles(Dictionary<string, Dictionary<string, string>> page, string sheetTitle)
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

        void SaveToFile(string sheetTitle, Dictionary<string, string> languageData, string langCode)
        {
            LocalizationAsset asset = ScriptableObject.CreateInstance<LocalizationAsset>();
            asset.values = languageData
                .Select(x => new LocalizationAsset.LanguageData(x.Key, x.Value))
                .ToList();

            if (sheetTitle != _settings.PredefinedSheetTitle)
            {
                var folderPath = $"{_settings.OtherSheetsPath}/{langCode}";
                Directory.CreateDirectory(folderPath);
                var filePath = $"{folderPath}/{sheetTitle}.asset";
                AssetDatabase.CreateAsset(asset, filePath);
                if (!string.IsNullOrEmpty(_settings.AddressableGroup))
                    AddAssetToGroup(folderPath, _settings.AddressableGroup, $"{langCode}");
            }
            else
            {
                var folderPath = $"{_settings.PredefinedPath}/{langCode}";
                Directory.CreateDirectory(folderPath);
                var filePath = $"{folderPath}/{sheetTitle}.asset";
                AssetDatabase.CreateAsset(asset, filePath);
            }
        }

        void ParseData(string data, string sheetTitle)
        {
            try
            {
                var loadedPage = LoadPage(data, sheetTitle);
                SaveToFiles(loadedPage, sheetTitle);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                AddError(e.Message, "Unknown");
            }
        }

        void AddAssetToGroup(string path, string groupName, string key = "")
        {
            var group = AddressableAssetSettingsDefaultObject.Settings.FindGroup(groupName);
            if (!group)
            {
                group = AddressableAssetSettingsDefaultObject.Settings.CreateGroup(groupName, false, false, true, null,
                    typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));
            }

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
            CreateFolder(_settings.OtherSheetsPath);
            CreateFolder(_settings.PredefinedPath);
        }

        void CreateFolder(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            Directory.CreateDirectory(path);
        }
    }
}