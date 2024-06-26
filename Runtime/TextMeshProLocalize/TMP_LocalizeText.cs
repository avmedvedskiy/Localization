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
        [LocalizationKey]
        private string _localization;
        
        [SerializeField]
        private TextMeshProUGUI _textMeshPro;


        private void OnLanguageChanged()
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
            if(_localization.IsNotNullOrEmpty())
                _textMeshPro.text = Localization.Get(_localization);
        }

        private void OnValidate()
        {
            _textMeshPro ??= GetComponent<TextMeshProUGUI>();
        }
    }
}

#endif