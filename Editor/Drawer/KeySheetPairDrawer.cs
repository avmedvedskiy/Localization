using UnityEditor;
using UnityEngine;

namespace LocalizationPackage
{
    [CustomPropertyDrawer(typeof(KeySheetPair))]
    public class KeySheetPairDrawer : PropertyDrawer
    {
        private LocalizationSettings Settings => LocalizationEditorUtils.Settings;

        private readonly LocalizationEditorPopupHelper _popupHelper = new LocalizationEditorPopupHelper();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var initialPosition = position;
            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            position.height /= 2f;
            // Calculate rects
            var partPosition = position.width / 3f;
            var sheetRect = new Rect(position.x, position.y, partPosition, position.height);
            var keyRect = new Rect(position.x + partPosition, position.y, partPosition * 2f, position.height);

            var sheetProp = property.FindPropertyRelative("sheet");
            var keyProp = property.FindPropertyRelative("key");

            if (Settings)
            {
                _popupHelper.DrawPopup(sheetRect, label, sheetProp, LocalizationEditorUtils.SheetList);
            }
            else
            {
                EditorGUI.PropertyField(sheetRect, sheetProp, GUIContent.none);
            }

            EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none);

            string key = keyProp.stringValue;
            string title = sheetProp.stringValue;
            var labelPosition = initialPosition;
            labelPosition.y += position.height / 2f;

            var localizationText = LocalizationEditorUtils.GetLocalizationText(Settings.EditorPreviewCode, title, key);

            if (string.IsNullOrEmpty(localizationText))
            {
                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.normal.textColor = Color.red;
                EditorGUI.LabelField(labelPosition, "Key not found", style);
            }
            else
            {
                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.normal.textColor = Color.blue;
                EditorGUI.LabelField(labelPosition, localizationText, style);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * 2f;
        }
    }
}