using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace LocalizationPackage
{
    public enum AddressableType
    {
        Resources,
        PerFolder,
        PerFile,
    }
    [Serializable]
    public class LocalizationSettings : ScriptableObject
    {
        
        [Serializable]
        public struct SheetInfo
        {
            public string name;
            public string id;
            public string addressableGroup;
            public AddressableType addressableType;
        }

        public const string SETTINGS_ASSET_PATH = "Assets/Localization/Resources/" + SETTINGS_NAME;
        public const string SETTINGS_NAME = "LocalizationSettings.asset";
        public const string ASSET_RESOURCES_PATH = "Assets/Localization/Resources/Languages";
        internal const string SETTINGS_ASSET_RESOURCES_PATH = "LocalizationSettings";
        internal const string ADDRESSABLE_SHEETS_PATH = "Assets/Localization/Addressables/Languages/";
        internal const string RESOURCES_PATH = "Assets/Localization/Resources/Languages";

        [SerializeField] private string _documentUrl;
        [SerializeField] private List<SheetInfo> _sheetInfos = new();

        [SerializeField] private bool _useSystemLanguagePerDefault = true;
        [SerializeField] private SystemLanguage _defaultLangCode = SystemLanguage.English;

        [SerializeField] private string _predefinedSheetTitle = "Predefined";

        [SerializeField] private List<SystemLanguage> _languageFilter;

        [SerializeField] private string _resourcesPath = RESOURCES_PATH;
        [SerializeField] private string _addressablePath = ADDRESSABLE_SHEETS_PATH;
        
        [Space(10)] [SerializeField] private SystemLanguage _editorPreviewCode = SystemLanguage.English;

        public string DocumentUrl => _documentUrl;
        public List<SheetInfo> SheetInfos => _sheetInfos;

        public bool UseSystemLanguagePerDefault
        {
            get => _useSystemLanguagePerDefault;
            set => _useSystemLanguagePerDefault = value;
        }

        public SystemLanguage DefaultLangCode
        {
            get => _defaultLangCode;
            set => _defaultLangCode = value;
        }

        public string PredefinedSheetTitle
        {
            get => _predefinedSheetTitle;
            set => _predefinedSheetTitle = value;
        }

        public List<SystemLanguage> LanguageFilter => _languageFilter;
        public string AddressablePath => _addressablePath;
        public string ResourcesPath => _resourcesPath;
        public SystemLanguage EditorPreviewCode => _editorPreviewCode;

        public SheetInfo GetSheetInfo(string sheetName) => _sheetInfos.Find(x => x.name == sheetName);
        
        public string GetAssetFilePath(string sheetTitle) => GetSheetInfo(sheetTitle).addressableType == AddressableType.Resources
            ? ResourcesPath 
            : AddressablePath;
    }
}