using System.Collections.Generic;

namespace Utils.TsExport.Utilities.String
{
    public static class Templates
    {
        /// <summary>
        /// Replace the variabales in key value pair with keys wrapped with [[]] or {{}}
        /// </summary>
        /// <param name="input"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static string FormatWith(this string input, Dictionary<string, string> fields)
        {
            foreach (var f in fields)
            {
                input = input.Replace("{{" + f.Key + "}}", f.Value);
                input = input.Replace("[[" + f.Key + "]]", f.Value);
            }
            return input;
        }

    }
}