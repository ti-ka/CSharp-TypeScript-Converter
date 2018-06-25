using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Utils.TsExport.Utilities.String;

namespace Utils.TsExport.Core
{
    public class TsClass
    {
        public bool IsPublic { get; set; }
        public string ClassName { get; set; }
        public string Namespace { get; set; }
        public virtual List<TsProperty> Properties { get; set; }
        public bool IsEnum { get; set; }
        public bool IsInterface { get; set; }
        public string BaseClass { get; set; }

        private Type _type;

        public TsClass(Type t)
        {
            _type = t;

            ClassName = t.Name;
            Namespace = t.Namespace;
            IsPublic = t.IsPublic;
            IsEnum = t.IsEnum;
            IsInterface = t.IsInterface;
            BaseClass = t.BaseType?.Name == "Object" ? null : t.BaseType?.Name;

            Properties = new List<TsProperty> { };

            foreach (var member in t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                var property = new TsProperty(member);
                Properties.Add(property);
            }
        }
        public IEnumerable<string> GetDependencies()
        {
            var deps = Properties.Select(prop => prop.DataType).Distinct();
            deps = deps.Where(prop => !TsProperty.DefinedProperties.Contains(prop));
            return deps;
        }

        public Array GetEnumValues()
        {
            return Enum.GetValues(_type);
        }

        public string FilePath {
            get
            {
                var parts = Namespace.Split(".").Select(w => w.ToCase(Case.KebabCase));
                return String.Join(Path.DirectorySeparatorChar, parts);
            }
        }
    }




}
