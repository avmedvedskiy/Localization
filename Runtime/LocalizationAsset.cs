﻿using System.Collections.Generic;
using UnityEngine;

namespace LocalizationPackage
{
    public class LocalizationAsset : ScriptableObject
    {
        [System.Serializable]
        public struct LanguageData
        {
            public string key;
            public string value;
        }

        public List<LanguageData> values = new List<LanguageData>();
    }
}