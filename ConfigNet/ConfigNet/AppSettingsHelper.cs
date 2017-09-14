using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigNet
{
    /// <summary>
    /// NameValueCollection must be unique
    /// </summary>
    public class AppSettingsHelper
    {
        public static int ConfigParseInt(string configKey, NameValueCollection settingsSource)
        {
            var stringValue = settingsSource[configKey];
            int configValue;
            if (!int.TryParse(stringValue, out configValue))
                throw new ConfigurationErrorsException($"Couldn't Parse int value of config key:{configKey}");
            return configValue;
        }
        public static bool ConfigParseBool(string configKey, NameValueCollection settingsSource)
        {
            var stringValue = settingsSource[configKey];
            bool configValue;

            if (!bool.TryParse(stringValue, out configValue))
                throw new ConfigurationErrorsException($"Couldn't Parse bool value of config key:{configKey}");
            return configValue;
        }

        public static string ConfigParseString(string configKey, NameValueCollection settingsSource)
        {
            var stringValue = settingsSource[configKey];
            if (string.IsNullOrEmpty(stringValue))
                throw new ConfigurationErrorsException($"Couldn't Parse string value of config key:{configKey}");
            return stringValue;
        }
    }
}
