namespace LocalizationPackage
{
    [System.Serializable]
    public struct KeySheetPair
    {
        public string key;
        public string sheet;

        public override string ToString() => Localization.Get(key, sheet);
        public bool IsNullOrEmpty() => string.IsNullOrEmpty(key);
    }
}