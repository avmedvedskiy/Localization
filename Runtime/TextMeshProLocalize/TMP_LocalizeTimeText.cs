using LocalizationPackage.Utilities;
using TMPro;
using UnityEngine;

namespace LocalizationPackage.TextMeshPro
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    [AddComponentMenu("UI/TextMeshPro - Localize Time Text (UI)", 11)]
    public class TMP_LocalizeTimeText : MonoBehaviour, ILocalize
    {
        [SerializeField]
        private int _seconds;

        [SerializeField]
        private TimeFormatManager.TimeFrom _timeFrom = TimeFormatManager.TimeFrom.Days;
        
        [SerializeField]
        [LocalizationSheet]
        private string _localizationSheet = TimeFormatManager.LOCALIZATION_SHEET_COMMON;
        
        [SerializeField]
        private TextMeshProUGUI _textMeshPro;


        public void OnLanguageSwitch()
        {
            UpdateText();
        }
        
        public void SetTime(int seconds)
        {
            _seconds = seconds;
            UpdateText();
        }
        
        private void OnDisable()
        {
            UpdateText();
        }
        
        private void UpdateText()
        {
            _textMeshPro.text = TimeFormatManager.FormatTime(_timeFrom,_seconds,_localizationSheet);
        }
    }
}