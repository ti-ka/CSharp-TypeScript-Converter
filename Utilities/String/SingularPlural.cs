namespace Utils.TsExport.Utilities.String
{
    public static class SingularPlural
    {
        /// <summary>
        /// Convert a string to plural. Dont not qualify all cases.
        /// </summary>
        /// <param name="plural"></param>
        /// <returns></returns>
        public static string ToPlural(this string plural)
        {
            if (plural.EndsWith("es"))
            {
                return plural;
            }

            if (plural.EndsWith("s"))
            {
                return plural + "es";
            }

            if (plural.EndsWith("y"))
            {
                return plural.Substring(0, plural.Length - 1) + "ies";
            }

            return plural + "s";
        }

        /// <summary>
        /// Convert a string to singular. Dont not qualify all cases.
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static string ToSingular(this string word)
        {
            if (word.EndsWith("ies"))
            {
                return word.Substring(0, word.Length - 3) + "y";
            }

            if (word.EndsWith("es"))
            {
                return word.Substring(0, word.Length - 1);
            }

            if (word.EndsWith("s"))
            {
                return word.Substring(0, word.Length - 1);
            }

            return word;
        }

    }
}
