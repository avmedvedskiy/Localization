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

        public const string SETTINGS_ASSET_PATH = ASSET_RESOURCES_PATH + "/" + SETTINGS_NAME;
        public const string SETTINGS_NAME = "LocalizationSettings.asset";
        public const string ASSET_RESOURCES_PATH = "Assets/Localization/Resources/Languages";
        internal const string SETTINGS_ASSET_RESOURCES_PATH = "Languages/LocalizationSettings";
        internal const string OTHER_SHEETS_PATH = "Assets/Localization/Languages/";
        internal const string PREDEF_PATH = "Assets/Localization/Resources/Languages/";
        internal const string ADDRESSABLE_DEFAULT_GROUP_NAME = "Localization";

        [SerializeField] private string _documentUrl;
        [SerializeField] private List<SheetInfo> _sheetInfos = new();

        [SerializeField] private bool _useSystemLanguagePerDefault = true;
        [SerializeField] private SystemLanguage _defaultLangCode = SystemLanguage.English;

        [SerializeField] private string _predefSheetTitle = "Predef";

        [SerializeField] private List<SystemLanguage> _languageFilter;

        [SerializeField] private string _predefPath = PREDEF_PATH;
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

        public string PredefSheetTitle
        {
            get => _predefSheetTitle;
            set => _predefSheetTitle = value;
        }

        public List<SystemLanguage> LanguageFilter => _languageFilter;
        public string AddressableGroup => _addressableGroup;
        public string OtherSheetsPath => _otherSheetsPath;

        public string PredefPath => _predefPath;

        public SystemLanguage EditorPreviewCode => _editorPreviewCode;

        public static SystemLanguage GetLanguageEnum(string langCode)
        {
            langCode = langCode.ToUpper();
            foreach (SystemLanguage item in Enum.GetValues(typeof(SystemLanguage)))
            {
                if (item.ToString() == langCode)
                {
                    return item;
                }
            }

            Debug.LogError("ERORR: There is no language: [" + langCode + "]");
            return SystemLanguage.English;
        }

        public string GetAssetFilePath(string sheetTitle)
        {
            if (sheetTitle == PredefSheetTitle)
                return PredefPath;
            return OtherSheetsPath;
        }
    }
}