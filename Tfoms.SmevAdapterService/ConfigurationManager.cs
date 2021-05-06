using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMEV;
using SMEV.WCFContract;

namespace SmevAdapterService
{
    public class ConfigurationManager:IConfigurationManager
    {
        private readonly ILogger logger;
        private readonly string config_path;
        private void AddLog(string log, LogType type)
        {
            logger?.AddLog(log, type);
        }

        public ConfigurationManager(string config_path, ILogger logger)
        {
            this.logger = logger;
            this.config_path = config_path;
        }
        public Configuration config { get; set; }
        public void Save()
        {
            try
            {
                AddLog("Загрузка конфигурации", LogType.Information);
                if (File.Exists(config_path))
                {
                    config = Configuration.LoadFromFile(config_path);
                }
                else
                {
                    config = new Configuration();
                    config.Check();
                    Configuration.SaveToFile(config_path, config);
                }
            }
            catch (Exception ex)
            {
                AddLog($"Ошибка загрузки конфигурации: {ex.Message}", LogType.Error);
            }
        }

        public void Load()
        {
            try
            {
                AddLog("Загрузка конфигурации", LogType.Information);
                if (File.Exists(config_path))
                {
                    config = Configuration.LoadFromFile(config_path);
                }
                else
                {
                    config = new Configuration();
                    config.Check();
                    Configuration.SaveToFile(config_path, config);
                }
            }
            catch (Exception ex)
            {
                AddLog($"Ошибка загрузки конфигурации: {ex.Message}", LogType.Error);
            }
        }
    }
}
