using System.Text.RegularExpressions;

namespace Utils.TsExport.Utilities.String
{
    public static class ConvertToCase
    {
        /// <summary>
        /// Convert String to various cases
        /// </summary>
        /// <param name="input"></param>
        /// <param name="stringCase"></param>
        /// <returns></returns>
        public static string ToCase(this string input, Case stringCase)
        {
            switch (stringCase) {

                case Case.CammelCase:
                    return input.ToCamelCase();

                case Case.KebabCase:
                    return input.ToKebabCase();

                case Case.LowerCase:
                    return input.ToLower();

                case Case.PascalCase:
                    return input.ToPascalCase();

                case Case.UpperCase:
                    return input.ToUpper();

                default:
                    return input;
            }

        }

        /// <summary>
        /// Convert to Pascal Case.
        /// Eg: MyCoolFriend => my-cool-friend
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>

        public static string ToPascalCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var cammelCased = input.ToCamelCase();
            return cammelCased[0].ToString().ToUpper() + cammelCased.Substring(1);
        }

        /// <summary>
        /// Convert Pascal Case to Kebab Case.
        /// Eg: MyCoolFriend => my-cool-friend
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToKebabCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return 
                input[0].ToString().ToLower() + 
                Regex.Replace(
                input.Substring(1),
                "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])",
                "-$1",
                RegexOptions.Compiled)
                .Trim()
                .ToLower();
        }


        /// <summary>
        /// Convert a string to Camel Case
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToCamelCase(this string input)
        {
            if (string.IsNullOrEmpty(input) || !char.IsUpper(input[0]))
            {
                return input;
            }

            char[] chars = input.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                if (i == 1 && !char.IsUpper(chars[i]))
                {
                    break;
                }

                bool hasNext = (i + 1 < chars.Length);
                if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
                {
                    break;
                }

                chars[i] = char.ToLowerInvariant(chars[i]);
            }

            return new string(chars);
        }



    }
}
