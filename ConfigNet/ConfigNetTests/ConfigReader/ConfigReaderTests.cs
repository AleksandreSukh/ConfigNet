using Microsoft.VisualStudio.TestTools.UnitTesting;
using ConfigNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigNet.Tests
{
    [TestClass()]
    public class ConfigExtensionsTests
    {
        public void CheckCorrect(Conf conf)
        {
            Assert.AreEqual(conf.ConfigVal1, "someText");
            Assert.AreEqual(conf.ConfigVal2, 15);
        }

        static readonly string configDir = @"ConfigReader";
        static readonly string configFull = Path.Combine(configDir, "App.config");
        static readonly string configFullLinked = Path.Combine(configDir, "App2.config");
        static readonly string configAppSettings = Path.Combine(configDir, "Settings.config");


        [TestMethod()]
        public void CanReadFromAppSettingsFile()
        {
            var conf = ConfigExtensions.ParseConfig<Conf>(configAppSettings);
            CheckCorrect(conf.Value);
        }
        [TestMethod()]
        public void CanReadFromAppConfigFile()
        {
            var conf = ConfigExtensions.ParseConfig<Conf>(configFull);
            CheckCorrect(conf.Value);
        }
        [TestMethod()]
        public void CanReadFromAppConfigFileWithLinkedSettingsFile()
        {
            var conf = ConfigExtensions.ParseConfig<Conf>(configFullLinked);
            CheckCorrect(conf.Value);
        }
    }

    public sealed class Conf
    {
        public readonly string ConfigVal1;
        public readonly int ConfigVal2;

        public Conf(string configVal1, int configVal2)
        {
            this.ConfigVal1 = configVal1;
            ConfigVal2 = configVal2;
        }
    }
}