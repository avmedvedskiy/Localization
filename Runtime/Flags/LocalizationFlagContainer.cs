using System;
using System.Collections.Generic;
using UnityEngine;

namespace LocalizationPackage
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Localization/LocalizationFlagContainer", fileName = "LocalizationFlagContainer", order = 0)]
    public class LocalizationFlagContainer : ScriptableObject
    {
        [Serializable]
        private struct FlagData
        {
            [SerializeField] private SystemLanguage _l;
            [SerializeField] private Sprite _flag;
            
            public SystemLanguage Language => _l;
            public Sprite Flag => _flag;
        }
        
        [SerializeField] private List<FlagData> _flags;

        public Sprite Get(SystemLanguage language)
        {
            for (int i = 0; i < _flags.Count; i++)
            {
                var f = _flags[i];
                if(f.Language == language)
                    return f.Flag;
            }
            return null;
        }
    }
}