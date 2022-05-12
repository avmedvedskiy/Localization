using TMPro;
using UnityEngine;

#if TMP
namespace LocalizationPackage.TextMeshPro
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    [AddComponentMenu("UI/TextMeshPro - Localize Text (UI)", 11)]
    public class TMP_LocalizeText : MonoBehaviour, ILocalize
    {
        [SerializeField]
        private KeySheetPair _localizationKey;
        
        [SerializeField]
        private TextMeshProUGUI _textMeshPro;


        public void OnLanguageSwitch()
        {
            UpdateText();
        }
        
        private void OnDisable()
        {
            UpdateText();
        }
        
        private void UpdateText()
        {
            _textMeshPro.text = _localizationKey.ToString();
        }
    }
}

#endif