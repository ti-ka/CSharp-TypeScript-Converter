using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utils.TsExport.Utilities.String;

namespace Utils.TsExport.Core
{
    public class Engine
    {
        private readonly Configuration _configuration;
        private List<TsClass> _classes { get; set; }

        public Engine(Configuration configuration)
        {
            _configuration = configuration;
            _classes = new List<TsClass>();
        }

        public void Export()
        {
            foreach (var cls in GetClasses())
            {
                var tsClass = new TsClass(cls);
                _classes.Add(tsClass);
            }

            // It's necessary to prepare all first to get paths for dependencies
            foreach (var cls in _classes)
            {
                if (!_configuration.IsClassIgnored(cls.ClassName))
                {
                    ExportTsClass(cls);
                }
            }

        }

        private Type[] GetClasses()
        {
            var classes = _configuration.Assembly.GetTypes();
            var exceptions = _configuration.GetIgnoredClasses().ToList();
            classes = classes.Where(c => !exceptions.Contains(c.Name)).ToArray();
            return classes;
        }

        private void ExportTsClass(TsClass tsClass)
        {
            var content = string.Empty;
            if (tsClass.IsEnum)
            {
                content = GetTsEnumStringTemplate(tsClass);
            }
            else
            {
                content = GetTsClassStringTemplate(tsClass);
            }

            // Finally write.
            var directory = Path.Combine(_configuration.OutputDirectory, tsClass.FilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var path = Path.Combine(directory, tsClass.ClassName.ToCase(_configuration.FileNameCase) + ".ts");
            File.WriteAllText(path, content);

        }

        private string GetTsClassStringTemplate(TsClass tsClass)
        {
            var str = Lines(1);
 
            str += ImportStatementsForClass(tsClass);

            if (tsClass.IsInterface)
            {
                str += string.Format("export interface {0}", tsClass.ClassName.ToCase(_configuration.ClassNameCase));
            }
            else
            {
                str += string.Format("export class {0}", tsClass.ClassName.ToCase(_configuration.ClassNameCase));
                if (tsClass.BaseClass != null)
                {
                    str += " extends " + tsClass.BaseClass;
                }
                
            }
            str += " {";

            // Class Backet Start

            #region Class Properties
            foreach (var property in tsClass.Properties)
            {
                str += Lines(1) + Indentation;

                // See if the property is ignored
                if (!_configuration.IsPropertyIgnored(tsClass.ClassName, property.PropertyName))
                {

                    // See if the data type is overridden
                    var dataType = property.DataType;
                    if (_configuration.IsPropertyOverriden(tsClass.ClassName, property.PropertyName))
                    {
                        dataType = _configuration.GetOverriddenPropertyValue(tsClass.ClassName, property.PropertyName);
                    }

                    str += string.Format("{0}: {1}", property.PropertyName.ToCase(_configuration.PropertyCase), dataType);
                    str += property.IsEnumerable ? "[]" : "";
                    str += property.IsEnumerable && _configuration.InitializeEnumerables ? " = []" : "";
                    str += _configuration.UseSemicolons ? ";" : "";
                }
            }
            #endregion

            // Class Backet End
            str += Lines(1) + "}" + Lines(1);

            return str;
        }

        private string ImportStatementsForClass(TsClass tsClass)
        {
            var str = "";
            var dependencies = tsClass.GetDependencies().ToList();

            // Also add dependency for import.
            if (tsClass.BaseClass != null)
            {
                dependencies.Add(tsClass.BaseClass);
            }

            foreach (var className in dependencies)
            {
                var dependentClass = _classes.Where(c => c.ClassName == className).FirstOrDefault();

                if (dependentClass == null)
                {
                    Console.WriteLine("Dependency not met for {0}/{1}", tsClass.ClassName, className);
                }
                else
                {
                    var file = dependentClass.ClassName.ToCase(_configuration.FileNameCase);
                    var dependentFile = Path.Combine(_configuration.OutputDirectory, dependentClass.FilePath);
                    var thisFolder = Path.Combine(_configuration.OutputDirectory, tsClass.FilePath);

                    if (!Directory.Exists(thisFolder))
                    {
                        Directory.CreateDirectory(thisFolder);
                    }
                
                    var path = GetRelativePath(dependentFile, thisFolder);

                    path = path.Replace(Path.DirectorySeparatorChar, '/');

                    str += string.Format("import {{ {0} }} from '{1}/{2}'", className, path, file);
                    str += _configuration.UseSemicolons ? ";" : "";
                    str += Lines(1);
                }

            }
            str += Lines(1);
            return str;
        }

        private string GetRelativePath(string filespec, string folder)
        {
            Uri pathUri = new Uri(filespec);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        private string GetTsEnumStringTemplate(TsClass tsClass)
        {
            var str = Lines(1);

            str += string.Format("export enum {0}", tsClass.ClassName.ToCase(_configuration.ClassNameCase));
            str += " {";

            // Enum Backet Start

            #region Enum Properties
            foreach (var property in tsClass.GetEnumValues())
            {
                str += Lines(1) + Indentation;
                str += string.Format("{0} = {1}, ",
                    property.ToString().ToCase(_configuration.PropertyCase),
                    (int)property);

            }
            #endregion

            // Enum Backet End
            str += Lines(1) + "}" + Lines(1);

            return str;
        }

        public string Indentation
        {
            get
            {
                var str = "";
                if (_configuration.UseTabs)
                {
                    str += "\t";
                }
                else
                {
                    for (int i = 0; i < _configuration.SpacesPerTab; i++)
                    {
                        str += " ";
                    }
                }
                return str;

            }
        }

        public string Lines(int number)
        {
            var str = "";
            for (int i = 0; i < number; i++)
                str += Environment.NewLine;
            return str;
        }
        
    }
}
