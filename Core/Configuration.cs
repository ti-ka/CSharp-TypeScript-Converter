using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Utils.TsExport.Utilities.String;

namespace Utils.TsExport.Core
{
    public class Configuration
    {
        public Assembly Assembly;

        public string OutputDirectory;
        public bool UseTabs = false;
        public int SpacesPerTab = 4;
        public bool UseSemicolons = true;

        public Case FileNameCase = Case.KebabCase;
        public Case ClassNameCase = Case.PascalCase;
        public Case PropertyCase = Case.CammelCase;

        public bool ExportToOneFile = false;
        public bool CreateFoldersForModule = false;
        public bool OverrideFiles = true;

        public bool InitializeEnumerables = true;

        public Dictionary<string, Dictionary<string, string>> _propertyOverrides = new Dictionary<string, Dictionary<string, string>>();
        public Dictionary<string, IEnumerable<string>> _ignoredProperties = new Dictionary<string, IEnumerable<string>>();
        public IEnumerable<string> _ignoredClasses = new List<string>();

        public Configuration IgnoreProperty(string className, string property)
        {

            if (!_ignoredProperties.ContainsKey(className))
            {
                _ignoredProperties[className] = new List<string>();
            }

            _ignoredProperties[className].Append(property);
            return this;
        }

        public Configuration OverrideProperties(string className, string property, string value)
        {
            if (!_propertyOverrides.ContainsKey(className))
            {
                _propertyOverrides[className] = new Dictionary<string, string>();
            }

            _propertyOverrides[className][property] = value; 
            return this;
        }

        public IEnumerable<string> GetIgnoredPropertiesByClass(string className)
        {
            return _ignoredProperties.Where(item => item.Key == className)
                .SelectMany(item => item.Value);
        }

        public Dictionary<string, string> GetOverridenPropertiesByClass(string className)
        {
            return _propertyOverrides[className];
        }

        public bool IsPropertyIgnored(string className, string property)
        {
            return GetIgnoredPropertiesByClass(className).Contains(property);
        }

        public bool IsPropertyOverriden(string className, string property)
        {
            return GetIgnoredPropertiesByClass(className).Contains(property);
        }

        public string GetOverriddenPropertyValue(string className, string property)
        {
            return _propertyOverrides[className][property];
        }

        public Configuration IgnoreClass(string className)
        {
            _ignoredClasses.Append(className);
            return this;
        }

        public IEnumerable<string> GetIgnoredClasses()
        {
            return _ignoredClasses;
        }

        public bool IsClassIgnored(string className)
        {
            return _ignoredClasses.Contains(className);
        }

    }

}
