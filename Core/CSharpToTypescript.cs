using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Utils.TsExport.Utilities.String;

namespace Utils.TsExport.Core
{
    public class CSharpToTypescript
    {
        public string solutionNamespace = "Omnidek";
        public bool useTab = false;
        public bool useSemiColon = true;
        public int spaces = 4;
        public string output = "export";
        public bool toCammelCase = true;
        public bool initilizeEnumerables = true;
        public bool overrideFiles = true;
        public bool exportSeperated = true;
        public Type BaseClassType { get; set; }
        public Dictionary<string, Dictionary<string, string>> PropertyExceptions = new Dictionary<string, Dictionary<string, string>>();

        public void Export()
        {
            FillPropertyExceptions();
            if(exportSeperated)
            {
                ExportSeperately();
                return;
            }
            var classes = GetClasses().Where(t => !t.Name.Contains("<>c")).ToList();
            var str = ConvertCSharpToTypescript(classes);
            var path = Path.GetFullPath(output);
            File.WriteAllText(path, str);
            Console.WriteLine("Written to: " + path);
        }

        private void FillPropertyExceptions()
        {
            Dictionary<string, string> FQSTP = new Dictionary<string, string>();
            FQSTP.Add("value", "any");
            PropertyExceptions.Add("FormsQuestionsAnswerTypesProperty", FQSTP);
        }

        private void ExportSeperately()
        {
            var path = Path.GetFullPath(output);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var classes = GetClasses().Where(t => !t.Name.Contains("<>c")).ToList();
            foreach (var c in classes)
            {
                var str = ConvertCSharpToTypescript(new List<Type> { c });
                var file = path + "/" + c.Name.ToKebabCase() + ".ts";
                File.WriteAllText(file, str);
            }


            Console.WriteLine("Written to: " + output);
        }

        private IEnumerable<Type> GetClasses()
        {
            var assembly = Assembly.GetAssembly(BaseClassType);
            var allItems = assembly.GetTypes();
            var items = assembly.GetTypes()
                // .Where(t => String.Equals(t.Namespace, BaseClassType.Namespace, StringComparison.Ordinal))
                .ToList();
            return Organize(items);
        }

        private List<Type> GetEntityClasses()
        {
            var list = new List<Type>();

            foreach (var t in GetClasses().Where(x => x.IsClass && !x.IsAbstract).ToList())
            {
                //if (t.IsSubclassOf(BaseClassType))
                //{
                    list.Add(t);
                //}
            }
            return list;

        }



        private string ExportPropertyName(string name)
        {
            if (toCammelCase)
            {
                return name.ToCamelCase();
            }
            return name;
        }

        private string ConvertCSharpToTypescript(IEnumerable<Type> classes)
        {

            var str = "";
            foreach (var tsClass in classes)
            {
                var dependencies = new List<string>();

                if (tsClass.IsEnum)
                {
                    #region Export Enum

                    str += Environment.NewLine;
                    str += Environment.NewLine;
                    str += string.Format("export enum {0}", tsClass.Name);
                    str += " {";

                    // Enum Backet End

                    #region Enum Properties

                    foreach (var property in Enum.GetValues(tsClass))
                    {
                        str += Environment.NewLine;
                        if (useTab)
                        {
                            str += "\t";
                        }
                        else
                        {
                            for (int i = 0; i < spaces; i++)
                            {
                                str += " ";
                            }
                        }

                        str += string.Format("{0} = {1}, ", ExportPropertyName(property.ToString()), (int)property);

                    }
                    #endregion

                    // Enum Backet End
                    str += Environment.NewLine;
                    str += "}";
                    str += Environment.NewLine;


                    #endregion
                }
                else if (tsClass.IsClass || tsClass.IsInterface)
                {
                    #region Export Classes

                    str += Environment.NewLine;
                    str += Environment.NewLine;

                    if (tsClass.IsInterface)
                    {
                        str += string.Format("export interface {0}", tsClass.Name);
                    }
                    else
                    {
                        var extends = CSharpTypeToTS(tsClass.BaseType.ToString());
                        str += string.Format("export class {0}", tsClass.Name);
                        if (extends != "Object")
                        {
                            str += string.Format(" extends {0}", extends);
                            dependencies.Add(extends);
                        }
                    }

                    // Classes Backet Open
                    str += " {";

                    #region Class Properties

                    var properties = tsClass.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                    Dictionary<string, string> classPropertyExceptions = null;
                    if (PropertyExceptions.Where(i => i.Key == tsClass.Name).Count() > 0)
                    {
                        classPropertyExceptions = PropertyExceptions.Where(i => i.Key == tsClass.Name).Select(i => i.Value).FirstOrDefault();
                    }

                    foreach (var property in properties)
                    {
                        var tsDataType = CSharpTypeToTS(property.PropertyType.ToString());
                        var propertyName = property.Name;
                        str += Environment.NewLine;

                        if (useTab)
                        {
                            str += "\t";
                        }
                        else
                        {
                            for (int i = 0; i < spaces; i++)
                            {
                                str += " ";
                            }
                        }

                        string exportedPropertyName = ExportPropertyName(propertyName);
                        if (classPropertyExceptions != null && classPropertyExceptions.Where(i => i.Key == exportedPropertyName).Count() > 0)
                            tsDataType = classPropertyExceptions.Where(i => i.Key == exportedPropertyName).Select(i => i.Value).FirstOrDefault();

                        str += string.Format("{0}: {1}", exportedPropertyName, tsDataType);
                        dependencies.Add(tsDataType);
                        if (initilizeEnumerables)
                        {
                            if (tsDataType.Contains("[") && tsDataType.EndsWith("]"))
                            {
                                str += " = []";
                            }
                        }

                        if (useSemiColon)
                        {
                            str += ";";
                        }
                    }


                    #endregion

                    // Classes Backet End
                    str += Environment.NewLine;
                    str += "}";
                    str += Environment.NewLine;

                    #endregion
                }

                if (exportSeperated)
                {
                    // Add Dependencies

                    var _depencencyStr = "";
                    foreach (var dep in CleanDependences(dependencies))
                    {
                        if (dep == tsClass.Name)
                            continue;

                        _depencencyStr += Environment.NewLine;
                        _depencencyStr += string.Format("import {{ {0} }} from './{1}';", dep, dep.ToKebabCase());
                    }
                    str = _depencencyStr + str;
                }


            }

           

            return str;
        }


        private List<string> CleanDependences(List<string> list)
        {
            var excluded = new List<string> { "string", "number", "boolean", "Date", "any" };
            return list
                .Where(r => !excluded.Contains(r))
                .Select(r => r.Replace("[]", ""))
                .Distinct().ToList();
        }

        private string CleanNullable(string input)
        {
            string pattern = @"System.Nullable`1\[([A-Z0-9.]*)\]";
            RegexOptions options = RegexOptions.IgnoreCase;
            var matches = Regex.Matches(input, pattern, options);
            if (matches.Count == 1 && matches[0].Groups.Count == 2)
            {
                input = matches[0].Groups[1].ToString().TrimStart('[').TrimEnd(']');
            }
            return input;
        }

        private string CleanEnumerable(string input)
        {
            string pattern = @"System.Collections.Generic.([A-Z]*)`1\[([A-Z0-9.]*)\]";
            RegexOptions options = RegexOptions.IgnoreCase;
            var matches = Regex.Matches(input, pattern, options);
            if (matches.Count == 1 && matches[0].Groups.Count == 3)
            {
                input = matches[0].Groups[2].ToString().TrimStart('[').TrimEnd(']') + "[]";
            }
            return input;
        }

        private string CleanNamespace(string input)
        {
            var particles = input.Split('.');
            return particles[particles.Length - 1];
        }


        private List<Type> Organize(IList<Type> classes)
        {
            int counter = 1;
            int depth = 1000;
            var list = new List<Type>();

            // Enums First:
            list.AddRange(classes.Where(c => c.IsEnum));
            classes = classes.Where(c => !c.IsEnum).ToList();

            var i = 0;
            while (classes.Count > 1)
            {
                var cls = classes.ElementAt(i);
                var hasClassMetDependencies = true;

                // See if it's parent class is in:
                var parent = cls.BaseType;

                if (parent == typeof(Object) || list.Contains(parent))
                {
                    foreach (var prop in cls.GetProperties())
                    {
                        var propertyType = prop.PropertyType;
                        var propertyTypeFullName = prop.PropertyType.FullName;
                        var propertyTypeName = prop.PropertyType.Name;

                        if (propertyTypeFullName.StartsWith("System"))
                        {
                            // Nice. It's System Property
                        }
                        else if (propertyTypeFullName == cls.FullName)
                        {
                            // Hmm, Parent is same as child class
                        }
                        else if (list.Contains(prop.PropertyType))
                        {
                            // We have it on list.
                        }
                        else
                        {
                            // Damn!
                            Console.WriteLine(string.Format("Dependency not met for: {0}/{1}", cls.FullName, prop.Name));
                            hasClassMetDependencies = false;
                            break;
                        }
                    }
                }
                else
                {
                    hasClassMetDependencies = false;
                }

                if (hasClassMetDependencies)
                {
                    list.Add(cls);
                    classes.Remove(cls);
                }

                i++;

                if (i >= classes.Count - 1)
                {
                    // Reset and begin
                    i = 0;
                }

                // Failsafe: Too many loops
                counter++;
                if (counter >= depth)
                {
                    Console.WriteLine("Loop broken. Depth: " + depth);
                    Console.WriteLine();
                    Console.WriteLine("WARNING: Unmet Dependencies for:");
                    Console.WriteLine("*************************************");
                    Console.WriteLine(string.Join(Environment.NewLine, classes.Select(t => t.Name)));
                    Console.WriteLine("*************************************");
                    Console.WriteLine();
                    //list.AddRange(classes);
                    return list;
                }
            }

            return list;
        }

        private string CSharpTypeToTS(string prop)
        {
            prop = CleanNullable(prop);
            prop = CleanEnumerable(prop);
            prop = CleanNamespace(prop);

            var map = TypeMap();
            if (map.ContainsKey(prop))
            {
                return map[prop];
            }


            return prop;
        }

        private IDictionary<string, string> TypeMap()
        {
            var map = new Dictionary<string, string>
            {
                ["Int32"] = "number",
                ["Int64"] = "number",
                ["Double"] = "number",
                ["Decimal"] = "number",
                ["Guid"] = "string",
                ["String"] = "string",
                ["Boolean"] = "boolean",
                ["DateTime"] = "Date"
            };

            return map;
        }


    }
}
