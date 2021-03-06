using System.Collections;
using System.Collections.Generic;
using System.IO;
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
            foreach (var info in _settings.sheetInfos)
            {
                UpdateSheet(info, displayProgressBar);
            }
        }

        public void UpdateSheet(LocalizationSettings.SheetInfo info, bool displayProgressBar)
        {
            string url = $"{_settings.documentUrl}&gid={info.id}";
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

        public void SaveSettingsFile(LanguageCode defaultLangCode, bool useSystemLang, string predefSheetTitle)
        {
            if (_settings == null)
            {
                _settings = (LocalizationSettings) ScriptableObject.CreateInstance(typeof(LocalizationSettings));
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

            _settings.defaultLangCode = defaultLangCode;
            _settings.useSystemLanguagePerDefault = useSystemLang;
            _settings.predefSheetTitle = predefSheetTitle;

            EditorUtility.SetDirty(_settings);
        }
        

        void LoadCSV(Hashtable loadLanguages, Hashtable loadEntries, string data, string sheetTitle)
        {
            List<string> lines = GetCVSLines(data);

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                List<string> contents = GetCVSLine(line);
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
                        Hashtable hTable = (Hashtable) loadEntries[j];
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
            Hashtable sampleData = (Hashtable) loadEntries[1];
            for (int j = 2; j < loadEntries.Count; j++)
            {
                Hashtable otherData = ((Hashtable) loadEntries[j]);

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

                string langCode = ((string) langs.Value).TrimEnd(System.Environment.NewLine.ToCharArray());
                if (string.IsNullOrEmpty(langCode))
                    continue;

                LanguageCode lc = (LanguageCode) System.Enum.Parse(typeof(LanguageCode), langCode);
                if (!_settings.languageFilter.Exists(x => x == lc))
                    continue;

                int langID = (int) langs.Key;
                Hashtable entries = (Hashtable) loadEntries[langID];
                foreach (DictionaryEntry item in entries)
                {
                    asset.values.Add(new LocalizationAsset.LanguageData()
                        {key = item.Key + "", value = (item.Value + "").UnescapeXML()});
                }

                if (sheetTitle != _settings.predefSheetTitle)
                {
                    var path = $"{_settings.otherSheetsPath}/{langCode}_{sheetTitle}.asset";
                    AssetDatabase.CreateAsset(asset, path);
#if USE_ADDRESSABLES
                    if(!string.IsNullOrEmpty(_settings.addressableGroup))
                        AddAssetToGroup(path,_settings.addressableGroup, $"{langCode}_{sheetTitle}");
#endif
                }
                else
                {
                    var path = $"{_settings.predefPath}/{langCode}_{sheetTitle}.asset";
                    AssetDatabase.CreateAsset(asset, path);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void AddAssetToGroup(string path ,string groupName, string key = "")
        {
            var group = AddressableAssetSettingsDefaultObject.Settings.FindGroup(groupName);
            if (!group)
            {
                group = AddressableAssetSettingsDefaultObject.Settings.CreateGroup(groupName, false, false, true, null, typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));
            }
            var assetPathToGuid = AssetDatabase.AssetPathToGUID(path);
            var entry = AddressableAssetSettingsDefaultObject.Settings.CreateOrMoveEntry(assetPathToGuid, group,
                false,
                true);
            if (entry == null)
            {
                Debug.LogError($"Addressable : can't add {path} to group {groupName}");
            }
            else
            {
                if(!string.IsNullOrEmpty(key));
                entry.address = key;
            }
        }


        List<string> GetCVSLines(string data)
        {
            List<string> lines = new List<string>();
            int i = 0;
            int searchCloseTags = 0;
            int lastSentenceStart = 0;
            while (i < data.Length)
            {
                if (data[i] == '"')
                {
                    if (searchCloseTags == 0)
                        searchCloseTags++;
                    else
                        searchCloseTags--;
                }
                else if (data[i] == '\n')
                {
                    if (searchCloseTags == 0)
                    {
                        lines.Add(data.Substring(lastSentenceStart, i - lastSentenceStart));
                        lastSentenceStart = i + 1;
                    }
                }

                i++;
            }

            if (i - 1 > lastSentenceStart)
            {
                lines.Add(data.Substring(lastSentenceStart, i - lastSentenceStart));
            }

            return lines;
        }

        List<string> GetCVSLine(string line)
        {
            List<string> list = new List<string>();
            int i = 0;
            int searchCloseTags = 0;
            int lastEntryBegin = 0;
            while (i < line.Length)
            {
                if (line[i] == '"')
                {
                    if (searchCloseTags == 0)
                        searchCloseTags++;
                    else
                        searchCloseTags--;
                }
                else if (line[i] == ',')
                {
                    if (searchCloseTags == 0)
                    {
                        list.Add(StripQuotes(line.Substring(lastEntryBegin, i - lastEntryBegin)));
                        lastEntryBegin = i + 1;
                    }
                }

                i++;
            }

            if (line.Length > lastEntryBegin)
            {
                list.Add(StripQuotes(line.Substring(lastEntryBegin))); //Add last entry
            }

            return list;
        }

        //Remove the double " that CVS adds inside the lines, and the two outer " as well
        string StripQuotes(string input)
        {
            if (input.Length < 1 || input[0] != '"')
                return input; //Not a " formatted line

            string output = "";
            ;
            int i = 1;
            bool allowNextQuote = false;
            while (i < input.Length - 1)
            {
                string curChar = input[i] + "";
                if (curChar == "\"")
                {
                    if (allowNextQuote)
                        output += curChar;
                    allowNextQuote = !allowNextQuote;
                }
                else
                {
                    output += curChar;
                }

                i++;
            }

            return output;
        }

        void CreateLanguageFolder()
        {
            CreateFolder(_settings.otherSheetsPath);
#if  !USE_ADDRESSABLES
            _settings.addressableGroup = string.Empty;
#endif
            CreateFolder(_settings.predefPath);
        }

        void CreateFolder(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}