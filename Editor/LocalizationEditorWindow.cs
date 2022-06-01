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
            window.maxSize = new Vector2(500f, 230f);
            window.minSize = window.maxSize;
            window.titleContent = new GUIContent("LocalizationEditor");
            window.Show();
        }

        private LocalizationEditorTools _tools = new LocalizationEditorTools();
        //Settings
        private LocalizationSettings _settings = null;

        string _gDocsURL = string.Empty;
        bool _useSystemLang = true;
        LanguageCode _defaultLanguageCode = LanguageCode.EN;
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
            _defaultLanguageCode = (LanguageCode) EditorGUILayout.EnumPopup("Default language", _defaultLanguageCode);

            int index = _settings.sheetInfos.FindIndex(x => x.name == _predefSheetTitle);
            if (index > -1)
            {
                index = EditorGUILayout.Popup("PreDefault Sheet Title", index,
                    _settings.sheetInfos.Select(x => x.name).ToArray());
                _predefSheetTitle = _settings.sheetInfos[index].name;
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

                int selectSheetIndex = _settings.sheetInfos.FindIndex(x => x.name == _selectedSheet);
                selectSheetIndex = EditorGUILayout.Popup("Selected Sheet Title", selectSheetIndex,
                    _settings.sheetInfos.Select(x => x.name).ToArray());

                if (selectSheetIndex != -1)
                    _selectedSheet = _settings.sheetInfos[selectSheetIndex].name;

                //update single sheet;
                if (selectSheetIndex != -1 && GUILayout.Button("Update Selected translation"))
                {
                    _updatingTranslation = true;
                    var info = _settings.sheetInfos.FirstOrDefault(x => x.name == _selectedSheet);
                    UpdateSheet(info);
                }
            }

            int unresolvedErrors = _tools.UnresolvedErrors;
            if (unresolvedErrors > 0)
            {
                Rect rec = GUILayoutUtility.GetLastRect();
                GUI.color = Color.red;
                EditorGUI.DropShadowLabel(new Rect(0, rec.yMin + 15, 200, 20),
                    "Unresolved errors: " + unresolvedErrors);
                GUI.color = Color.white;
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

            _useSystemLang = _settings.useSystemLanguagePerDefault;
            _defaultLanguageCode = _settings.defaultLangCode;
            _predefSheetTitle = _settings.predefSheetTitle;
            _gDocsURL = _settings.documentUrl;
        }

        void UpdateSheet(LocalizationSettings.SheetInfo info)
        {
            _tools.ClearErrors();
            _tools.UpdateSheet(info, true);
            EditorUtility.ClearProgressBar();
            _updatingTranslation = false;
        }

        private void UpdateLocalization()
        {
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