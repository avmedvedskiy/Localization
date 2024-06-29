using UnityEditor;
using UnityEngine;

namespace LocalizationPackage
{
    [CustomEditor(typeof(LocalizationSettings))]
    public class LocalizationSettingsEditor : Editor
    {
        private readonly LocalizationEditorTools _tools = new();
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Update All translations"))
            {
                _tools.UpdateLocalization(true);
                EditorUtility.ClearProgressBar();
                
            }
        }
    }
}