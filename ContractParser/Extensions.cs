namespace ContractParser
{
    public static class Extensions
    {
        public static string RemoveOperator(this string s, char[] operators = null)
        {
            operators ??= new char[2] { '@', '%' };

            return s.TrimStart(operators);
        }

        public static int ParseOrDefaultInt(this string text, int defVal = 0)
        {
            if (int.TryParse(text, out int tmp))
                return tmp;
            else
                return defVal;
        }

        public static float ParseOrDefaultFloat(this string text, float defVal = 0)
        {
            if (float.TryParse(text, out float tmp))
                return tmp;
            else
                return defVal;
        }

        public static bool ParseOrDefaultBool(this string text, bool defVal = false)
        {
            if (bool.TryParse(text, out bool tmp))
                return tmp;
            else
                return defVal;
        }
    }
}
