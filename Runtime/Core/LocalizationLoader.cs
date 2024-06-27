using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace LocalizationPackage
{
    internal static class LocalizationLoader
    {
        private static LocalizationSettings Settings => SettingsProvider.Settings;
        public static Dictionary<string, string> LoadDefaultSheet(SystemLanguage code)
        {
            return LoadFromResources(code, Settings.PredefinedSheetTitle); 
        }

        public static async UniTask<Dictionary<string, string>> LoadSheetAsync(SystemLanguage code, string sheetTitle)
        {
            if (sheetTitle == Settings.PredefinedSheetTitle || string.IsNullOrEmpty(Settings.AddressableGroup))
            {
                return await LoadFromResourcesAsync(code, sheetTitle);
            }
            return await LoadFromBundleAsync(code, sheetTitle);
        }

        private static async UniTask<Dictionary<string, string>> LoadFromBundleAsync(
            SystemLanguage code, 
            string sheetTitle)
        {
            var op = Addressables.LoadAssetAsync<LocalizationAsset>($"{code}/{sheetTitle}.asset");
            var file = await op.ToUniTask();
            var values =  ConvertAsset(file);
            Addressables.Release(op);
            Resources.UnloadAsset(file);
            return values;
        }

        private static async UniTask<Dictionary<string, string>>  LoadFromResourcesAsync(
            SystemLanguage code,
            string sheetTitle)
        {
            var path = TrimResourcesPath($"{Settings.GetAssetFilePath(sheetTitle)}/{code}/{sheetTitle}");
            var file = (LocalizationAsset)await Resources.LoadAsync<LocalizationAsset>(path).ToUniTask();
            var values =  ConvertAsset(file);
            Resources.UnloadAsset(file);
            return values;
        }

        private static Dictionary<string, string> LoadFromResources(
            SystemLanguage code,
            string sheetTitle)
        {
            var path = TrimResourcesPath($"{Settings.GetAssetFilePath(sheetTitle)}/{code}/{sheetTitle}");
            var file = Resources.Load<LocalizationAsset>(path);
            var values =  ConvertAsset(file);
            Resources.UnloadAsset(file);
            return values;
        }

        private static string TrimResourcesPath(string path)
        {
            int index = path.IndexOf("/Resources/", StringComparison.OrdinalIgnoreCase);
            if (index != -1)
            {
                return path.Substring(index + "/Resources/".Length);
            }

            return path;
        }

        private static Dictionary<string, string> ConvertAsset(LocalizationAsset localizationAsset) 
            => localizationAsset.values.ToDictionary(x => x.key, y => y.value);
    }
}