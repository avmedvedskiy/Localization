using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace LocalizationPackage
{
    [System.Serializable]
    public class LocalizationSettings : ScriptableObject
    {
        [System.Serializable]
        public struct SheetInfo
        {
            public string name;
            public string id;
        }

        public const string SETTINGS_ASSET_PATH = ASSET_RESOURCES_PATH + "/" + SETTINGS_NAME;
        public const string SETTINGS_NAME = "LocalizationSettings.asset";
        public const string ASSET_RESOURCES_PATH = "Assets/Localization/Resources/Languages";
        public const string SETTINGS_ASSET_RESOURCES_PATH = "Languages/LocalizationSettings";
        public const string OTHER_SHEETS_PATH = "Assets/Localization/Languages/";
        public const string PREDEF_PATH = "Assets/Localization/Resources/Languages/";
        public const string ADDRESSABLE_DEFAULT_GROUP_NAME = "Localization";

        public string documentUrl;
        public List<SheetInfo> sheetInfos = new List<SheetInfo>();

        public bool useSystemLanguagePerDefault = true;
        public LanguageCode defaultLangCode = LanguageCode.EN;

        public string predefSheetTitle = "Predef";

        public List<LanguageCode> languageFilter;
        
        public string predefPath = PREDEF_PATH;
        public string addressableGroup = ADDRESSABLE_DEFAULT_GROUP_NAME;
        public string otherSheetsPath = OTHER_SHEETS_PATH;

        [Space(10)]
        public LanguageCode editorPreviewCode = LanguageCode.EN;
        
        //GENERAL
        public static LanguageCode GetLanguageEnum(string langCode)
        {
            langCode = langCode.ToUpper();
            foreach (LanguageCode item in Enum.GetValues(typeof(LanguageCode)))
            {
                if (item.ToString() == langCode)
                {
                    return item;
                }
            }
            Debug.LogError("ERORR: There is no language: [" + langCode + "]");
            return LanguageCode.EN;
        }

        public string GetAssetFilePath(string sheetTitle)
        {
            if (sheetTitle == predefSheetTitle)
                return predefPath;
            return otherSheetsPath;
        }
    }
}