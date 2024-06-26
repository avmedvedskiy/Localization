using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace LocalizationPackage
{
    public class LocalizationEditorCleanUp
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void CleanUpFastMode()
        {
            var instanceField =
                typeof(Localization).GetField("_storage",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            instanceField?.SetValue(null, new Dictionary<string, Dictionary<string, string>>());
        }
    }
}