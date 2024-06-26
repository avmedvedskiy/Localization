namespace LocalizationPackage
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);
        public static bool IsNotNullOrEmpty(this string str) => !string.IsNullOrEmpty(str);
        
        public static string UnescapeXML(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;
            
            return s
                .Replace("&apos;", "'")
                .Replace("&quot;", "\"")
                .Replace("&gt;", ">")
                .Replace("&lt;", "<")
                .Replace("&amp;", "&");
        }
    }
}