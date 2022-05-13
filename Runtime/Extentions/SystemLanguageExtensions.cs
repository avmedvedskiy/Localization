using UnityEngine;

namespace LocalizationPackage
{
    public static class SystemLanguageExtensions
    {
        /// <summary>
        /// Languagesname to code. Note that this will not take variations such as EN_GB and PT_BR into account
        /// </summary>
        /// <returns>The name to code.</returns>
        public static LanguageCode ToLanguageCode(this SystemLanguage name)
        {
            if (name == SystemLanguage.Afrikaans) return LanguageCode.AF;
            if (name == SystemLanguage.Arabic) return LanguageCode.AR;
            if (name == SystemLanguage.Basque) return LanguageCode.BA;
            if (name == SystemLanguage.Belarusian) return LanguageCode.BE;
            if (name == SystemLanguage.Bulgarian) return LanguageCode.BG;
            if (name == SystemLanguage.Catalan) return LanguageCode.CA;
            if (name == SystemLanguage.Czech) return LanguageCode.CS;
            if (name == SystemLanguage.Danish) return LanguageCode.DA;
            if (name == SystemLanguage.Dutch) return LanguageCode.NL;
            if (name == SystemLanguage.English) return LanguageCode.EN;
            if (name == SystemLanguage.Estonian) return LanguageCode.ET;
            if (name == SystemLanguage.Faroese) return LanguageCode.FA;
            if (name == SystemLanguage.Finnish) return LanguageCode.FI;
            if (name == SystemLanguage.French) return LanguageCode.FR;
            if (name == SystemLanguage.German) return LanguageCode.DE;
            if (name == SystemLanguage.Greek) return LanguageCode.EL;
            if (name == SystemLanguage.Hebrew) return LanguageCode.HE;
            if (name == SystemLanguage.Hungarian) return LanguageCode.HU;
            if (name == SystemLanguage.Icelandic) return LanguageCode.IS;
            if (name == SystemLanguage.Indonesian) return LanguageCode.ID;
            if (name == SystemLanguage.Italian) return LanguageCode.IT;
            if (name == SystemLanguage.Japanese) return LanguageCode.JA;
            if (name == SystemLanguage.Korean) return LanguageCode.KO;
            if (name == SystemLanguage.Latvian) return LanguageCode.LA;
            if (name == SystemLanguage.Lithuanian) return LanguageCode.LT;
            if (name == SystemLanguage.Norwegian) return LanguageCode.NO;
            if (name == SystemLanguage.Polish) return LanguageCode.PL;
            if (name == SystemLanguage.Portuguese) return LanguageCode.PT;
            if (name == SystemLanguage.Romanian) return LanguageCode.RO;
            if (name == SystemLanguage.Russian) return LanguageCode.RU;
            if (name == SystemLanguage.SerboCroatian) return LanguageCode.SH;
            if (name == SystemLanguage.Slovak) return LanguageCode.SK;
            if (name == SystemLanguage.Slovenian) return LanguageCode.SL;
            if (name == SystemLanguage.Spanish) return LanguageCode.ES;
            if (name == SystemLanguage.Swedish) return LanguageCode.SW;
            if (name == SystemLanguage.Thai) return LanguageCode.TH;
            if (name == SystemLanguage.Turkish) return LanguageCode.TR;
            if (name == SystemLanguage.Ukrainian) return LanguageCode.UK;
            if (name == SystemLanguage.Vietnamese) return LanguageCode.VI;
            if (name == SystemLanguage.Chinese) return LanguageCode.ZH;
            if (name == SystemLanguage.ChineseSimplified) return LanguageCode.ZH_CN;
            if (name == SystemLanguage.ChineseTraditional) return LanguageCode.ZH_TW;
            if (name == SystemLanguage.Unknown) return LanguageCode.N;

            return LanguageCode.N;
        }
    }
}