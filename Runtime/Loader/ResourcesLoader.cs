using System.Collections;
using UnityEngine;

#if  USE_ADDRESSABLES
using UnityEngine.AddressableAssets;
#endif

namespace LocalizationPackage
{
    public sealed class ResourcesLoader : ILocalizationLoader
    {
        public LocalizationAsset GetLanguageFileAsset(LocalizationSettings settings,LanguageCode code, string sheetTitle)
        {
            if (sheetTitle == settings.predefSheetTitle || string.IsNullOrEmpty(settings.addressableGroup))
                return Resources.Load<LocalizationAsset>($"{settings.GetAssetFilePath(sheetTitle)}/{code}_{sheetTitle}.asset");
            
#if  USE_ADDRESSABLES
            return Addressables.LoadAssetAsync<LocalizationAsset>($"{code}_{sheetTitle}").WaitForCompletion();
#endif
        }
    }
}