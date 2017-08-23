using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Xml;
using CSharpFunctionalExtensions;

namespace ConfigNet
{
    public class ConfigExtensions
    {
        const string SettingNodeName = "add";
        const string KeyAttributeName = "key";
        const string ValueAttributeName = "value";

        public static Result AddMissingValues(string path, List<KeyValuePair<string, Type>> missingValues, Func<string, Type, string> missingValueRetriever)
        {
            if (!missingValues.Any()) return Result.Ok();
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(path);

            foreach (var mv in missingValues)
            {
                var mvn = xmlDoc.CreateElement(SettingNodeName, xmlDoc.NamespaceURI);
                var keyAttr = xmlDoc.CreateAttribute(KeyAttributeName);
                var valueAttr = xmlDoc.CreateAttribute(ValueAttributeName);

                //Retrieve missing value
                var value = missingValueRetriever.Invoke(mv.Key, mv.Value);


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
                if (!File.Exists(path)||string.IsNullOrEmpty(File.ReadAllText(path)))
                {
                    var emptyConfigFile = "<appSettings><!--Empty--></appSettings>";
                    File.WriteAllText(path, emptyConfigFile);
                }

                var xmlNew = new XmlDocument();
                xmlNew.Load(path);

                var nameValueCollectionNode = xmlNew.FirstChild.ChildNodes.OfType<XmlNode>().Where(n => n.Name.Equals(SettingNodeName, StringComparison.InvariantCultureIgnoreCase));

                var fields = typeof(T).GetFields();
                if (fields.Length == 0)
                {
                    throw new InvalidOperationException($"Type:{typeof(T).Name} must contain public fields to fill in configuration values");
                }
                foreach (var propertyInfo in fields)
                {
                    var propType = propertyInfo.FieldType;
                    var propName = propertyInfo.Name;
                    if (!nameValueCollectionNode.Any(n => n.Attributes[KeyAttributeName]?.Value != null && n.Attributes[KeyAttributeName].Value.Equals(propName, StringComparison.InvariantCultureIgnoreCase)))
                        missingValues.Add(new KeyValuePair<string, Type>(propName, propType));
                }
            }
            catch (Exception e)
            {
                return Result.Fail<List<KeyValuePair<string, Type>>>(e.Message);
            }

            return Result.Ok(missingValues);
        }


        public static Result<T> ParseConfig<T>(string newConfigPath) where T : class
        {
            T config = null;
            try
            {
                var xmlNew = new XmlDocument();
                xmlNew.Load(newConfigPath);
                var nameValueCollectionNode = xmlNew.FirstChild.ChildNodes.OfType<XmlNode>().Where(n => n.Name == SettingNodeName);

                NameValueCollection nvc = new NameValueCollection();
                foreach (var nod in nameValueCollectionNode)
                {
                    if (nod.Attributes?[KeyAttributeName] != null && nod.Attributes[ValueAttributeName] != null)
                        nvc.Add(nod.Attributes[KeyAttributeName].Value, nod.Attributes[ValueAttributeName].Value);
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