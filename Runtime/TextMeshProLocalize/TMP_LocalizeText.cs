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
        
        private void Start()
        {
            UpdateText();
            Localization.OnLanguageChanged += OnLanguageChanged;
        }

        private void OnDestroy()
        {
            Localization.OnLanguageChanged -= OnLanguageChanged;
        }

        private void UpdateText()
        {
            _textMeshPro.text = _localizationKey.ToString();
        }
    }
}

#endif