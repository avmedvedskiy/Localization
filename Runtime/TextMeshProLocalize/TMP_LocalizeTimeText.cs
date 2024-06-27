using LocalizationPackage.Utilities;
using TMPro;
using UnityEngine;

namespace LocalizationPackage.TextMeshPro
{
    [RequireComponent(typeof(TMP_Text))]
    [AddComponentMenu("UI/TextMeshPro - Localize Time Text (UI)", 11)]
    public class TMP_LocalizeTimeText : MonoBehaviour
    {
        [SerializeField]
        private int _seconds;

        [SerializeField]
        private TimeFormatManager.TimeFrom _timeFrom = TimeFormatManager.TimeFrom.Days;
        
        [SerializeField]
        private TMP_Text _textMeshPro;

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

        private void OnValidate()
        {
            _textMeshPro ??= GetComponent<TMP_Text>();
        }
    }
}