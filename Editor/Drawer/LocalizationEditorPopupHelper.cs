using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LocalizationPackage
{
    public class LocalizationEditorPopupHelper
    {
        private int _selectionIndex;

        public void DrawPopup(Rect position, GUIContent label, SerializedProperty sheetProp, List<string> items)
        {
            if (GUI.Button(position, sheetProp.stringValue, EditorStyles.popup))
            {
                _selectionIndex = items.FindIndex(x => sheetProp.stringValue == x);
                _selectionIndex = EditorGUI.Popup(position, label.text, _selectionIndex, items.ToArray());

                GenericMenu menu = new GenericMenu();
                for (int i = 0; i < items.Count; i++)
                {
                    int index = i;
                    menu.AddItem(new GUIContent(items[index]), _selectionIndex == index,
                        () =>
                        {
                            _selectionIndex = index;
                            sheetProp.stringValue = items[_selectionIndex];
                            sheetProp.serializedObject.ApplyModifiedProperties();
                        });
                }

                menu.ShowAsContext();
            }
        }
    }
}