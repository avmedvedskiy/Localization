using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace LocalizationPackage.TextMeshPro
{
    [AddComponentMenu("UI/TextMeshPro - Localize Font size Text (UI)", 12)]
    public class TMP_LocalizeFontSizeText : MonoBehaviour
    {
        [Serializable]
        private struct LanguageSize
        {
            public SystemLanguage code;
            public float size;
        }
        
        [SerializeField] private float _defaultSize;
        [SerializeField] private LanguageSize[] _sizes;
        [SerializeField] private TMP_Text[] _texts;
        

        private void OnEnable()
        {
            UpdateText();
            Localization.OnLanguageChanged += OnLanguageChanged;
        }

        private void OnDisable()
        {
            Localization.OnLanguageChanged -= OnLanguageChanged;
        }

        private void OnLanguageChanged()
        {
            UpdateText();
        }
        
        private void UpdateText()
        {
            for (int i = 0; i < _sizes.Length; i++)
            {
                var size = _sizes[i];
                if (size.code == Localization.CurrentLanguage)
                {
                    for (int j = 0; j < _texts.Length; j++)
                    {
                        var text = _texts[j];
                        text.enableAutoSizing = false;
                        text.fontSize = size.size;
                    }
                    return;
                }
            }
        }

        private void OnValidate()
        {
            _texts ??= GetComponentsInChildren<TMP_Text>();
            for (int i = 0; i < _texts.Length; i++)
            {
                var text = _texts[i];
                text.enableAutoSizing = false;
                text.fontSize = _defaultSize;
            }
        }
    }
}