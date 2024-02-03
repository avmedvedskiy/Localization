using System;
using TMPro;
using UnityEngine;

#if TMP
namespace LocalizationPackage.TextMeshPro
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    [AddComponentMenu("UI/TextMeshPro - Localize Text (UI)", 11)]
    public class TMP_LocalizeText : MonoBehaviour
    {
        [SerializeField]
        private KeySheetPair _localizationKey;
        
        [SerializeField]
        private TextMeshProUGUI _textMeshPro;
        

        public void OnLanguageChanged()
        {
            UpdateText();
        }
        
        private void OnEnable()
        {
            UpdateText();
            Localization.OnLanguageChanged += OnLanguageChanged;
        }

        private void OnDisable()
        {
            Localization.OnLanguageChanged -= OnLanguageChanged;
        }

        private void UpdateText()
        {
            _textMeshPro.text = _localizationKey.ToString();
        }

        private void OnValidate()
        {
            _textMeshPro ??= GetComponent<TextMeshProUGUI>();
        }
    }
}

#endif