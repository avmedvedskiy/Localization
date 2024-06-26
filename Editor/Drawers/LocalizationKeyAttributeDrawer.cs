using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace LocalizationPackage.Drawers
{
    class LocalizationDropdown : AdvancedDropdown
    {
        class LocalizationDropdownItem : AdvancedDropdownItem
        {
            public string Key { get; }
            public string Value { get; }

            public LocalizationDropdownItem(string key, string value) : base($"{key}  |  {value}")
            {
                Key = key;
                Value = value;
            }
        }

        private readonly Dictionary<string, List<LocalizationAsset.LanguageData>> _items;
        private readonly Action<string> _onSelect;

        public LocalizationDropdown(AdvancedDropdownState state, Dictionary<string, List<LocalizationAsset.LanguageData>> items,
            Action<string> onSelect) : base(state)
        {
            _items = items;
            _onSelect = onSelect;
            minimumSize = new Vector2(0, 350);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Localization");
            foreach (var item in _items)
            {
                var key = new AdvancedDropdownItem(item.Key);
                foreach (var value in item.Value)
                {
                    key.AddChild(new LocalizationDropdownItem(value.key, value.value));
                }

                root.AddChild(key);
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item is LocalizationDropdownItem localizationDropdownItem)
                _onSelect?.Invoke(localizationDropdownItem.Key);
        }
    }

    [CustomPropertyDrawer(typeof(LocalizationKeyAttribute))]
    public class LocalizationKeyAttributeDrawer : PropertyDrawer
    {
        private const float SEARCH_BUTTON_SIZE = 50f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //base.OnGUI(position, property, label);
            var firstLinePosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            var nextLinePosition = new Rect(position.x,
                position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
                position.width, EditorGUIUtility.singleLineHeight);
            var buttonPosition = new Rect(position.x + EditorGUIUtility.labelWidth - SEARCH_BUTTON_SIZE, position.y,
                SEARCH_BUTTON_SIZE, EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(firstLinePosition, property, label);
            DrawLocalizedLabel(property, nextLinePosition);

            if (GUI.Button(buttonPosition, "Search"))
                DrawDropdown(position, property);

            if (EditorGUI.EndChangeCheck())
                property.serializedObject.ApplyModifiedProperties();
        }

        private void DrawDropdown(Rect position, SerializedProperty property)
        {
            var dropdown = new LocalizationDropdown(
                new AdvancedDropdownState(),
                LocalizationEditorUtils.GetAllKeys(),
                name => ApplyKey(property, name));
            dropdown.Show(position);
        }

        private static void ApplyKey(SerializedProperty property, string name)
        {
            property.stringValue = name;
            property.serializedObject.ApplyModifiedProperties();
        }

        private void DrawLocalizedLabel(SerializedProperty property, Rect nextLinePosition)
        {
            EditorGUI.LabelField(nextLinePosition, string.Empty,
                LocalizationEditorUtils.GetLocalizationText(property.stringValue),
                GetBlueTextStyle());
        }

        private static GUIStyle GetBlueTextStyle()
        {
            return new(GUI.skin.label)
            {
                normal =
                {
                    textColor = Color.blue
                }
            };
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * 2f;
        }
    }
}