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
        private int _unresolvedErrors;

        public int UnresolvedErrors => _unresolvedErrors;

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
            _unresolvedErrors = 0;
        }

        /// <summary>
        /// Can be used for autobuild system
        /// </summary>
        public void LoadAndUpdateLocalization()
        {
            LoadSettings();
            UpdateLocalization(false);
        }
        
        public void UpdateLocalization(bool displayProgressBar)
        {
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
            _settings.PredefSheetTitle = predefSheetTitle;

            EditorUtility.SetDirty(_settings);
        }


        void LoadCSV(Hashtable loadLanguages, Hashtable loadEntries, string data, string sheetTitle)
        {
            List<string> lines = SplitCVSLines(data);

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                List<string> contents = SplitCVSLine(line);
                if (i == 0)
                {
                    //Language titles
                    for (int j = 1; j < contents.Count; j++)
                    {
                        loadLanguages[j] = contents[j];
                        loadEntries[j] = new Hashtable();
                    }
                }
                else if (contents.Count > 1)
                {
                    string key = contents[0];
                    if (key == "")
                        continue; //Skip entries with empty keys (the other values can be used as labels)
                    for (int j = 1; j < (loadLanguages.Count + 1) && j < contents.Count; j++)
                    {
                        string content = contents[j];
                        Hashtable hTable = (Hashtable)loadEntries[j];
                        if (hTable.ContainsKey(key))
                        {
                            Debug.LogError("ERROR: Double key [" + key + "] Sheet: " + sheetTitle);
                            _unresolvedErrors++;
                        }

                        hTable[key] = System.Security.SecurityElement.Escape(content);
                    }
                }
            }
        }

        void ParseData(string data, string sheetTitle)
        {
            CreateLanguageFolder();

            Hashtable loadLanguages = new Hashtable();
            Hashtable loadEntries = new Hashtable();

            LoadCSV(loadLanguages, loadEntries, data, sheetTitle);

            if (loadEntries.Count < 1)
            {
                _unresolvedErrors++;
                Debug.LogError("Sheet " + sheetTitle + " contains no languages!");
                return;
            }

            //Verify loaded data
            Hashtable sampleData = (Hashtable)loadEntries[1];
            for (int j = 2; j < loadEntries.Count; j++)
            {
                Hashtable otherData = ((Hashtable)loadEntries[j]);

                foreach (DictionaryEntry item in otherData)
                {
                    if (!sampleData.ContainsKey(item.Key))
                    {
                        Debug.LogError("[" + loadLanguages[1] + "] [" + item.Key + "] Key is missing!");
                        _unresolvedErrors++;
                    }
                }

                foreach (DictionaryEntry item in sampleData)
                {
                    if (!otherData.ContainsKey(item.Key))
                    {
                        Debug.LogError("Sheet(" + sheetTitle + ") [" + loadLanguages[j] + "] [" + item.Key +
                                       "] Key is missing!");
                        _unresolvedErrors++;
                    }
                }
            }

            //Save the loaded data
            foreach (DictionaryEntry langs in loadLanguages)
            {
                LocalizationAsset asset = ScriptableObject.CreateInstance<LocalizationAsset>();

                string langCode = ((string)langs.Value).TrimEnd(System.Environment.NewLine.ToCharArray());
                if (string.IsNullOrEmpty(langCode))
                    continue;

                SystemLanguage lc = (SystemLanguage)System.Enum.Parse(typeof(SystemLanguage), langCode);
                if (!_settings.LanguageFilter.Exists(x => x == lc))
                    continue;
                
                
                int langID = (int)langs.Key;
                Hashtable entries = (Hashtable)loadEntries[langID];
                foreach (DictionaryEntry item in entries)
                {
                    asset.values.Add(new LocalizationAsset.LanguageData()
                        { key = (string)item.Key, value = ((string)item.Value).UnescapeXML() });
                }

                if (sheetTitle != _settings.PredefSheetTitle)
                {
                    var path = $"{_settings.OtherSheetsPath}/{langCode}_{sheetTitle}.asset";
                    AssetDatabase.CreateAsset(asset, path);
                    if (!string.IsNullOrEmpty(_settings.AddressableGroup))
                        AddAssetToGroup(path, _settings.AddressableGroup, $"{langCode}_{sheetTitle}");
                }
                else
                {
                    var path = $"{_settings.PredefPath}/{langCode}_{sheetTitle}.asset";
                    AssetDatabase.CreateAsset(asset, path);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void AddAssetToGroup(string path, string groupName, string key = "")
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
                if (!string.IsNullOrEmpty(key)) ;
                entry.address = key;
            }
        }


        private List<string> SplitCVSLines(string data) => data.Split("\r\n").ToList();

        List<string> SplitCVSLine(string line)
        {
            return line.Split(",").ToList();
        }

        void CreateLanguageFolder()
        {
            CreateFolder(_settings.OtherSheetsPath);
            CreateFolder(_settings.PredefSheetTitle);
        }

        void CreateFolder(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}