namespace LocalizationPackage
{
    [System.Serializable]
    public struct KeySheetPair
    {
        public string key;
        public string sheet;

        public override string ToString()
        {
            return Localization.Get(key, sheet);
        }
    }
}
