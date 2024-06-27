using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace LocalizationPackage
{
    [Serializable]
    public class LocalizationSettings : ScriptableObject
    {
        [Serializable]
        public struct SheetInfo
        {
            public string name;
            public string id;
        }

        public const string SETTINGS_ASSET_PATH = "Assets/Localization/Resources/" + SETTINGS_NAME;
        public const string SETTINGS_NAME = "LocalizationSettings.asset";
        public const string ASSET_RESOURCES_PATH = "Assets/Localization/Resources/Languages";
        internal const string SETTINGS_ASSET_RESOURCES_PATH = "LocalizationSettings";
        internal const string OTHER_SHEETS_PATH = "Assets/Localization/Languages/";
        internal const string PREDEFINED_PATH = "Assets/Localization/Resources/Languages";
        internal const string ADDRESSABLE_DEFAULT_GROUP_NAME = "Localization";

        [SerializeField] private string _documentUrl;
        [SerializeField] private List<SheetInfo> _sheetInfos = new();

        [SerializeField] private bool _useSystemLanguagePerDefault = true;
        [SerializeField] private SystemLanguage _defaultLangCode = SystemLanguage.English;

        [SerializeField] private string _predefinedSheetTitle = "Predefined";

        [SerializeField] private List<SystemLanguage> _languageFilter;

        [SerializeField] private string _predefinedPath = PREDEFINED_PATH;
        [SerializeField] private string _addressableGroup = ADDRESSABLE_DEFAULT_GROUP_NAME;
        [SerializeField] private string _otherSheetsPath = OTHER_SHEETS_PATH;

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
        public string AddressableGroup => _addressableGroup;
        public string OtherSheetsPath => _otherSheetsPath;

        public string PredefinedPath => _predefinedPath;

        public SystemLanguage EditorPreviewCode => _editorPreviewCode;

        public string GetAssetFilePath(string sheetTitle) => sheetTitle == PredefinedSheetTitle ? PredefinedPath : OtherSheetsPath;
    }
}