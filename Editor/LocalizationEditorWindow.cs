using System;
using UnityEngine;
using UnityEditor;
using System.Linq;
#if USE_ADDRESSABLES
#endif

namespace LocalizationPackage
{
    public class LocalizationEditorWindow : EditorWindow
    {

        [MenuItem("Tools/Localization/LoadLocalization")]
        static void OpenWindow()
        {
            var window = GetWindow(typeof(LocalizationEditorWindow));
            window.maxSize = new Vector2(500f, 300f);
            window.minSize = window.maxSize;
            window.titleContent = new GUIContent("LocalizationEditor");
            window.Show();
        }

        private LocalizationEditorTools _tools = new LocalizationEditorTools();
        //Settings
        private LocalizationSettings _settings = null;

        string _gDocsURL = string.Empty;
        bool _useSystemLang = true;
        SystemLanguage _defaultLanguageCode = SystemLanguage.English;
        string _predefSheetTitle;

        bool _updatingTranslation;
        readonly string status = "...";
        string _selectedSheet;

        Vector2 _scrollView;
        
        private void OnGUI()
        {
            GUILayout.Label("Settings", EditorStyles.boldLabel);
            if (EditorApplication.isPlaying)
            {
                GUILayout.Label("Editor is in play mode.");
                return;
            }

            LoadSettings();
            if (_settings == null)
            {
                GUILayout.Label("Settings not found " + LocalizationSettings.SETTINGS_ASSET_PATH);
                return;
            }


            _scrollView = GUILayout.BeginScrollView(_scrollView);
            EditorGUILayout.ObjectField("Settings",_settings, typeof(LocalizationSettings),false);
            _useSystemLang = EditorGUILayout.Toggle("Try system language", _useSystemLang);
            _defaultLanguageCode = (SystemLanguage) EditorGUILayout.EnumPopup("Default language", _defaultLanguageCode);

            var sheetInfos = _settings.SheetInfos;
            int index = sheetInfos.FindIndex(x => x.name == _predefSheetTitle);
            if (index > -1)
            {
                index = EditorGUILayout.Popup("PreDefault Sheet Title", index,
                    sheetInfos.Select(x => x.name).ToArray());
                _predefSheetTitle = sheetInfos[index].name;
            }
            else
            {
                _predefSheetTitle = EditorGUILayout.DelayedTextField("PreDefault Sheet Title", _predefSheetTitle);
            }

            _gDocsURL = EditorGUILayout.TextField("gDocs Link", _gDocsURL);

            if (GUI.changed)
            {
                _tools.SaveSettingsFile(_defaultLanguageCode,_useSystemLang, _predefSheetTitle);
            }

            GUILayout.Space(10f);
            if (status != null)
            {
                GUILayout.Label(status, EditorStyles.label);
            }

            if (!_updatingTranslation)
            {
                if (GUILayout.Button("Update All translations"))
                {
                    _updatingTranslation = true;
                    UpdateLocalization();
                }

                GUILayout.Space(10f);

                var settingsSheetInfos = sheetInfos;
                int selectSheetIndex = settingsSheetInfos.FindIndex(x => x.name == _selectedSheet);
                selectSheetIndex = EditorGUILayout.Popup("Selected Sheet Title", selectSheetIndex,
                    settingsSheetInfos.Select(x => x.name).ToArray());

                if (selectSheetIndex != -1)
                    _selectedSheet = settingsSheetInfos[selectSheetIndex].name;

                //update single sheet;
                if (selectSheetIndex != -1 && GUILayout.Button("Update Selected translation"))
                {
                    _updatingTranslation = true;
                    var info = settingsSheetInfos.FirstOrDefault(x => x.name == _selectedSheet);
                    UpdateSheet(info);
                }
            }

            if (_tools.Errors is {Count:>0})
            {
                GUIStyle style = new GUIStyle()
                {
                    normal = new GUIStyleState()
                    {
                        textColor = Color.red
                    }
                };
                foreach (var errorInfo in _tools.Errors.Distinct())
                {
                    EditorGUILayout.LabelField(errorInfo.error,style);
                }
            }

            GUILayout.Space(10f);
            //DrawUtility();
            GUILayout.EndScrollView();
        }

        void LoadSettings()
        {
            if (_settings != null)
                return;

            _settings = _tools.LoadSettings();

            _useSystemLang = _settings.UseSystemLanguagePerDefault;
            _defaultLanguageCode = _settings.DefaultLangCode;
            _predefSheetTitle = _settings.PredefinedSheetTitle;
            _gDocsURL = _settings.DocumentUrl;
        }

        void UpdateSheet(LocalizationSettings.SheetInfo info)
        {
            LoadSettings();
            _tools.ClearErrors();
            _tools.UpdateSheet(info, true);
            EditorUtility.ClearProgressBar();
            _updatingTranslation = false;
        }

        private void UpdateLocalization()
        {
            LoadSettings();
            _tools.ClearErrors();
            _tools.UpdateLocalization(true);
            _updatingTranslation = false;
            EditorUtility.ClearProgressBar();
        }


        #region LocalizationFontAssetsUtility

        //public LocalizationFontAssetsUtility utility;

        //void DrawUtility()
        //{
        //    if (utility == null)
        //    {
        //        utility = AssetDatabase.LoadAssetAtPath<LocalizationFontAssetsUtility>(LocalizationFontAssetsUtility.PATH);
        //        if (utility == null && GUILayout.Button("Create LocalizationFontAssetsUtility"))
        //        {
        //            utility = CreateInstance<LocalizationFontAssetsUtility>();
        //            AssetDatabase.CreateAsset(utility, LocalizationFontAssetsUtility.PATH);
        //            AssetDatabase.SaveAssets();
        //            AssetDatabase.Refresh();
        //            utility = AssetDatabase.LoadAssetAtPath<LocalizationFontAssetsUtility>(LocalizationFontAssetsUtility.PATH);
        //        }
        //    }
        //    else
        //    {
        //        EditorGUILayout.ObjectField("Utility", utility, typeof(LocalizationFontAssetsUtility), false);
        //    }
        //}

        #endregion
    }
}