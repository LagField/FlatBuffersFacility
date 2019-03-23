using System;
using System.IO;
using Newtonsoft.Json;

namespace FlatBuffersFacility
{
    public static class AppData
    {
        private static string ConfigFilePath
        {
            get { return AppDomain.CurrentDomain.BaseDirectory + "config.json"; }
        }

        private static Config config;

        public static string FbsDirectory
        {
            get => config.fbsDirectory;
            set
            {
                if (config != null && config.fbsDirectory != value)
                {
                    config.fbsDirectory = value;
                    SaveConfig();
                }
            }
        }

        public static string CompilerPath
        {
            get => config.compilerPath;
            set
            {
                if (config.compilerPath != value)
                {
                    config.compilerPath = value;
                    SaveConfig();
                }
            }
        }

        public static string CsOutputDirectory
        {
            get => config.csOutputDirectory;
            set
            {
                if (config.csOutputDirectory != value)
                {
                    config.csOutputDirectory = value;
                    SaveConfig();
                }
            }
        }

        public static string TargetNamespace
        {
            get => config.targetNamespace;
            set => config.targetNamespace = value;
        }

        public static bool IsGeneratePoolVersion
        {
            get => config.isGeneratePoolVersion;
            set => config.isGeneratePoolVersion = value;
        }

        public static void Init()
        {
            if (File.Exists(ConfigFilePath))
            {
                string json = File.ReadAllText(ConfigFilePath);
                config = JsonConvert.DeserializeObject<Config>(json);
            }
            else
            {
                config = new Config();
            }
        }

        public static void SaveConfig()
        {
            if (config == null)
            {
                return;
            }

            string json = JsonConvert.SerializeObject(config);
            File.WriteAllText(ConfigFilePath, json);
        }


        [Serializable]
        private class Config
        {
            public string fbsDirectory;
            public string compilerPath;
            public string csOutputDirectory;
            public string targetNamespace;
            public bool isGeneratePoolVersion;
        }
    }
}