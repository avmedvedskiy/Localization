using LocalizationPackage.Utilities;
using TMPro;
using UnityEngine;

namespace LocalizationPackage.TextMeshPro
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    [AddComponentMenu("UI/TextMeshPro - Localize Time Text (UI)", 11)]
    public class TMP_LocalizeTimeText : MonoBehaviour
    {
        [SerializeField]
        private int _seconds;

        [SerializeField]
        private TimeFormatManager.TimeFrom _timeFrom = TimeFormatManager.TimeFrom.Days;
        
        [SerializeField]
        private TextMeshProUGUI _textMeshPro;

        public void SetTime(int seconds)
        {
            _seconds = seconds;
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

        private void OnLanguageChanged()
        {
            UpdateText();
        }
        
        private void UpdateText()
        {
            _textMeshPro.text = TimeFormatManager.FormatTime(_timeFrom,_seconds);
        }
    }
}