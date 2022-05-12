using UnityEditor;
using UnityEngine;

namespace LocalizationPackage
{
    [CustomPropertyDrawer(typeof(LocalizationSheetAttribute))]
    public class LocalizationSheetAttributeDrawer : PropertyDrawer
    {
        private LocalizationSettings Settings => LocalizationEditorUtils.Settings;
        private readonly LocalizationEditorPopupHelper _popupHelper = new LocalizationEditorPopupHelper();
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            if (property.serializedObject.hasModifiedProperties)
                property.serializedObject.ApplyModifiedProperties();

            property.serializedObject.Update();
            
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            
            if (Settings)
            {
                _popupHelper.DrawPopup(position, label, property, LocalizationEditorUtils.SheetList);
            }
            else
            {
                EditorGUI.PropertyField(position, property, GUIContent.none);
            }
            
            EditorGUI.EndProperty();
        }
    }
}