using System;

namespace AnotherECS.Debug.Diagnostic.UIElements
{
    internal static class UINumericFieldsUtils
    {
        public static readonly string k_AllowedCharactersForFloat = "inftynaeINFTYNAE0123456789.,-*/+%^()cosqrludxvRL=pP#";
        public static readonly string k_AllowedCharactersForInt = "0123456789-*/+%^()cosintaqrtelfundxvRL,=pPI#";
        public static readonly string k_DoubleFieldFormatString = "R";
        public static readonly string k_FloatFieldFormatString = "g7";
        public static readonly string k_IntFieldFormatString = "#######0";

        public static bool StringToUlong(string str, out ulong value)
            => ulong.TryParse(str, out value);

        internal static bool StringToUint(string str, out uint value)
            => uint.TryParse(str, out value);

        internal static bool StringToSfloat(string str, out sfloat value)
            => sfloat.TryParse(str, out value);
    }
}