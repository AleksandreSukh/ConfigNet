using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Xml;
using CSharpFunctionalExtensions;

namespace ConfigNet
{
    public static class AppSettingsConstants
    {
        public const string SettingNodeName = "add";
        public const string KeyAttributeName = "key";
        public const string ValueAttributeName = "value";
        public const string AppSettingsNodeName = "appSettings";
        public const string ConfigSourceKeyName = "configSource";
    }
    public static class ConfigExtensions
    {

        public static Result AddMissingValues(string path, List<KeyValuePair<string, Type>> missingValues, Func<string, Type, string> missingValueRetriever)
        {
            if (!missingValues.Any()) return Result.Ok();
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(path);

            foreach (var mv in missingValues)
            {
                var mvn = xmlDoc.CreateElement(AppSettingsConstants.SettingNodeName, xmlDoc.NamespaceURI);
                var keyAttr = xmlDoc.CreateAttribute(AppSettingsConstants.KeyAttributeName);
                var valueAttr = xmlDoc.CreateAttribute(AppSettingsConstants.ValueAttributeName);

                //Retrieve missing value
                string value;
                try { value = missingValueRetriever.Invoke(mv.Key, mv.Value); }
                catch (Exception e) { return Result.Fail(e.Message); }

                keyAttr.Value = mv.Key;
                valueAttr.Value = value;
                mvn.Attributes.Append(keyAttr);
                mvn.Attributes.Append(valueAttr);


                xmlDoc.FirstChild.AppendChild(mvn);
            }

            xmlDoc.Save(path);
            return Result.Ok();
        }


        public static Result<List<KeyValuePair<string, Type>>> GetMissingValues<T>(string path) where T : class
        {
            List<KeyValuePair<string, Type>> missingValues = new List<KeyValuePair<string, Type>>();
            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                if (!File.Exists(path) || string.IsNullOrEmpty(File.ReadAllText(path)))
                {
                    var emptyConfigFile = "<appSettings><!--Empty--></appSettings>";
                    File.WriteAllText(path, emptyConfigFile);
                }

                var xmlNew = new XmlDocument();
                xmlNew.Load(path);

                var nameValueCollectionNode = xmlNew.FirstChild.ChildNodes.OfType<XmlNode>().Where(n => n.Name.Equals(AppSettingsConstants.SettingNodeName, StringComparison.InvariantCultureIgnoreCase));

                var fields = typeof(T).GetFields();
                if (fields.Length == 0)
                {
                    throw new InvalidOperationException($"Type:{typeof(T).Name} must contain public fields to fill in configuration values");
                }
                foreach (var propertyInfo in fields)
                {
                    var propType = propertyInfo.FieldType;
                    var propName = propertyInfo.Name;
                    if (!nameValueCollectionNode.Any(n => n.Attributes[AppSettingsConstants.KeyAttributeName]?.Value != null && n.Attributes[AppSettingsConstants.KeyAttributeName].Value.Equals(propName, StringComparison.InvariantCultureIgnoreCase)))
                        missingValues.Add(new KeyValuePair<string, Type>(propName, propType));
                }
            }
            catch (Exception e)
            {
                return Result.Fail<List<KeyValuePair<string, Type>>>(e.Message);
            }

            return Result.Ok(missingValues);
        }

        static XmlNode GetFirstDescentdantOrDefault(this XmlNode node, Predicate<XmlNode> filter)
        {
            if (filter(node)) return node;
            return node.ChildNodes.OfType<XmlNode>()
                .Select(cn => GetFirstDescentdantOrDefault(cn, filter))
                .FirstOrDefault(resultInChild => resultInChild != null);
        }
        public static Result<T> ParseConfig<T>(string path) where T : class
        {
            T config = null;
            try
            {
                var xmlNew = new XmlDocument();
                xmlNew.Load(path);

                //var appSettingsNode = xmlNew.ChildNodes.OfType<XmlNode>().FirstOrDefault(n => n.Name.Equals(AppSettingsConstants.AppSettingsNodeName));
                var appSettingsNode = xmlNew.GetFirstDescentdantOrDefault(n => n.Name.Equals(AppSettingsConstants.AppSettingsNodeName));

                if (appSettingsNode == null) throw new ConfigReaderException($"Couldn't find application settings node:{AppSettingsConstants.AppSettingsNodeName} in file:{path}");

                var appSettingsSourceKeyValue = appSettingsNode.Attributes?[AppSettingsConstants.ConfigSourceKeyName]?.Value;
                if (appSettingsSourceKeyValue != null)
                    return ParseConfig<T>(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(path)), appSettingsSourceKeyValue));

                var nameValueCollectionNode = appSettingsNode.ChildNodes.OfType<XmlNode>()
                    .Where(n => n.Name == AppSettingsConstants.SettingNodeName);

                NameValueCollection nvc = new NameValueCollection();
                foreach (var nod in nameValueCollectionNode)
                {
                    if (nod.Attributes?[AppSettingsConstants.KeyAttributeName] != null && nod.Attributes[AppSettingsConstants.ValueAttributeName] != null)
                        nvc.Add(nod.Attributes[AppSettingsConstants.KeyAttributeName].Value, nod.Attributes[AppSettingsConstants.ValueAttributeName].Value);
                }
                config = ConfigReader.ReadFromSettings<T>(nvc);

            }
            catch (Exception e)
            {
                return Result.Fail<T>(e.Message);
            }
            return Result.Ok(config);
        }

    }
}