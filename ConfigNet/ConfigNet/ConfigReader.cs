using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Runtime.Serialization;

namespace ConfigNet
{
    public static class ConfigReader
    {
        public static T ReadFromSettings<T>(NameValueCollection settingsSource = null) where T : class
        {
            if (settingsSource == null)
                settingsSource = ConfigurationManager.AppSettings;
            var fields = typeof(T).GetFields();
            if (fields.Length == 0)
            {
                throw new InvalidOperationException($"Type:{typeof(T).Name} must contain public fields to fill in configuration values");
            }
            var tObject = (T)FormatterServices.GetUninitializedObject(typeof(T));
            foreach (var propertyInfo in fields)
            {
                object configValue = null;
                var propType = propertyInfo.FieldType;
                var propName = propertyInfo.Name;

                if (propType == typeof(int))
                    configValue = AppSettingsHelper.ConfigParseInt(propName, settingsSource);

                else if (propType == typeof(string))
                    configValue = AppSettingsHelper.ConfigParseString(propName, settingsSource);

                else if (propType == typeof(bool))
                    configValue = AppSettingsHelper.ConfigParseBool(propName, settingsSource);
                else throw new NotSupportedException($"Parsing configuartion value for:{propName} failed because parsing config value of type: {propType.Name} is not supported");
                propertyInfo.SetValue(tObject, configValue);
            }
            return tObject;
        }
    }
}