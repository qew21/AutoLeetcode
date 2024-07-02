using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoLeetcode
{
    public class Configuration
    {
        public string BaseUrl { get; set; } = "https://dashscope.aliyuncs.com/compatible-mode";
        public string ApiKey { get; set; } = "API_KEY";
        public string Model { get; set; } = "qwen-turbo";
        public string Level { get; set; } = "medium";  // Choose difficulty
        public string Skip { get; set; } = "1";  // Skip completed questions

    }

    public static class ConfigManager
    {
        private static readonly string _filePath = "config.json";

        public static Configuration CurrentConfig { get; private set; }

        static ConfigManager()
        {
            CurrentConfig = LoadConfig();
        }

        public static Configuration LoadConfig()
        {
            if (File.Exists(_filePath))
            {
                string json = File.ReadAllText(_filePath);
                return JsonConvert.DeserializeObject<Configuration>(json);
            }
            else
            {
                var config = new Configuration();
                SaveConfig(config);
                return config;
            }
        }

        public static void SaveConfig(Configuration config)
        {
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }

        public static void UpdateConfig(Action<Configuration> updateAction)
        {
            string serializedConfig = JsonConvert.SerializeObject(CurrentConfig);
            Configuration tempConfig = JsonConvert.DeserializeObject<Configuration>(serializedConfig);
            updateAction(tempConfig);
            SaveConfig(tempConfig);
            CurrentConfig = tempConfig;
        }
    }
}
