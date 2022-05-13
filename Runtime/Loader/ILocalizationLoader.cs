using System;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LocalizationPackage
{
    public interface ILocalizationLoader
    {
        /// <summary>
        /// Get Language file asse;
        /// </summary>
        LocalizationAsset GetLanguageFileAsset(LocalizationSettings settings,LanguageCode code, string sheetTitle);
    }
}