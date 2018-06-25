using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Utils.TsExport.Core
{

    public class TsProperty
    {
        public string DataType { get; set; }
        public string PropertyName { get; set; }
        public bool IsNullable { get; set; }
        public bool IsEnumerable { get; set; }
        public TsProperty(PropertyInfo info)
        {
            DataType = GetTsDataType(info.PropertyType);
            PropertyName = info.Name;
        }

        private string GetTsDataType(Type propertyType)
        {
            var name = propertyType.ToString();

            name = CheckNullability(name); // Check if nullable and clean
            name = CheckEnumerable(name);  // Check if Enumeable and clean
            name = CleanNamespace(name);   // Remove namespace and make a clean name

            if (PropertyMap.ContainsKey(name))
            {
                return PropertyMap[name];
            }

            return name;
        }

        public static string[] DefinedProperties = new[] { "any", "number", "string", "boolean", "Date" };
        public static Dictionary<string, string> PropertyMap => new Dictionary<string, string>
        {
            ["Int32"] = "number",
            ["Int64"] = "number",
            ["Double"] = "number",
            ["Decimal"] = "number",
            ["Float"] = "number",
            ["Single"] = "number",
            ["Guid"] = "string",
            ["String"] = "string",
            ["Guid"] = "string",
            ["Boolean"] = "boolean",
            ["DateTime"] = "Date"
        };

        private string CheckNullability(string input)
        {
            string pattern = @"System.Nullable`1\[([A-Z0-9.]*)\]";
            RegexOptions options = RegexOptions.IgnoreCase;
            var matches = Regex.Matches(input, pattern, options);
            if (matches.Count == 1 && matches[0].Groups.Count == 2)
            {
                input = matches[0].Groups[1].ToString().TrimStart('[').TrimEnd(']');
                IsNullable = true;
            }
            return input;
        }

        private string CheckEnumerable(string input)
        {
            string pattern = @"System.Collections.Generic.([A-Z]*)`1\[([A-Z0-9.]*)\]";
            RegexOptions options = RegexOptions.IgnoreCase;
            var matches = Regex.Matches(input, pattern, options);
            if (matches.Count == 1 && matches[0].Groups.Count == 3)
            {
                input = matches[0].Groups[2].ToString().TrimStart('[').TrimEnd(']');
                IsEnumerable = true;
            }
            return input;
        }

        private string CleanNamespace(string input)
        {
            var particles = input.Split('.');
            return particles[particles.Length - 1];
        }


    }

}
