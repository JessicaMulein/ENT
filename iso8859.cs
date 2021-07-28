namespace entropy
{
    /// <summary>
    /// Summary description for iso8859.
    /// </summary>
    public sealed class iso8859
    {
        /* ISO 8859/1 Latin-1 alphabetic and upper and lower case bit vector tables. */

        /* LINTLIBRARY */

        readonly char[] isoalpha = new char[32] { 0, 0, 0, 0, 0, 0, 0, 0, 127, 255, 255, 224, 127, 255, 255, 224, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 254, 255, 255, 255, 254, 255 };
        readonly char[] isoupper = new char[32] { 0, 0, 0, 0, 0, 0, 0, 0, 127, 255, 255, 224, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 254, 254, 0, 0, 0, 0 };
        readonly char[] isolower = new char[32] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 127, 255, 255, 224, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 255, 255, 254, 255 };

        public static bool isascii(char x)
        {
            return (x < 0x80);
        }
        public static bool isISOspace(char x)
        {
            return ((isascii(x) && isspace(x)) || (x == 0xA0));
        }
        public static bool isISOalpha(char x)
        {
            return ((isoalpha[(x / 8)] & (0x80 >> ((x % 8) != 0))));
        }
        public static bool isISOupper(char x)
        {
            return ((isoupper[(x / 8)] & (0x80 >> ((x % 8) != 0))));
        }
        public static bool isISOlower(char x)
        {
            return ((isolower[(x / 8)] & (0x80 >> (x % 8))) != 0);
        }

        public static bool isISOprint(char x)
        {
            return ((((x) >= ' ') && ((x) <= '~')) || ((x) >= 0xA0));
        }
        public static bool toISOupper(char x)
        {
            return (isISOlower(x) ? (isascii(x) ? toupper(x) : (((x != 0xDF) && (x != 0xFF)) ? (x - 0x20) : (x))) : (x));
        }
        public static bool toISOlower(char x)
        {
            return (isISOupper(x) ? (isascii(x) ? tolower(x) : (x + 0x20)) : (x));
        }
    }
}
