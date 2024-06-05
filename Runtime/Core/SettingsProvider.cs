using UnityEngine;

namespace LocalizationPackage
{
    internal static class SettingsProvider
    {
        private static LocalizationSettings _settings;

        public static LocalizationSettings Settings
        {
            get
            {
                //automatically load settings from resources if the pointer is null (to avoid null-ref-exceptions!)
                if (_settings == null)
                {
                    _settings = Resources.Load<LocalizationSettings>(LocalizationSettings
                        .SETTINGS_ASSET_RESOURCES_PATH);
                }

                return _settings;
            }
        }
    }
}