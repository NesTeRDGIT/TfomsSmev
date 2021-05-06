using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Npgsql;
using NpgsqlTypes;
using SMEV.WCFContract;
using SmevAdapterService.AdapterLayer.Integration;
using SmevAdapterService.AdapterLayer.XmlClasses;
using SmevAdapterService.VS;
using SMEV.VS.MedicalCare;
using SMEV.VS.MedicalCare.newV1_0_0.FeedbackOnMedicalService;
using SMEV.VS.Zags;
using InputData = SMEV.VS.MedicalCare.V1_0_0.InputData;
using OutputData = SMEV.VS.MedicalCare.V1_0_0.OutputData;
using Zags4_0_1 = SMEV.VS.Zags4_0_1;
using SMEV;

namespace SmevAdapterService
{
    public partial class AdapterService : ServiceBase
    {
        private ILogger logger = new LoggerEventLog("SMEV_Service");
        private IProcess process;
        private IConfigurationManager ConfigurationManager;
        private IPingManager pingManager;
        private WcfServer wi;
        public ServiceHost WcfConnection { set; get; }


        public AdapterService()
        {
            InitializeComponent();
            var config_dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CONFIG");
            var pathconfig = Path.Combine(config_dir, "config.xml");
            var PingConfigPath = Path.Combine(config_dir, "config_ping.xml");
            if (!Directory.Exists(config_dir))
            {
                Directory.CreateDirectory(config_dir);
            }
            ConfigurationManager = new ConfigurationManager(pathconfig, logger);
            pingManager = new PingManager(PingConfigPath, OnResultPing, logger );
            process = new ProcessWork(logger);
    }

        private void OnResultPing(PingResult obj)
        {
           wi.SEND_PING_RESULT(obj);
        }
        protected override void OnStart(string[] args)
        {
            AddLog("Старт службы", LogType.Information);
            AddLog("Запуск WCF", LogType.Information);
            if (!StartServer())
            {
                Stop();
                return;
            }
            try
            {
                AddLog("Загрузка конфигурации", LogType.Information);
                ConfigurationManager.Load();
                pingManager.LoadConfig();
            }
            catch (Exception ex)
            {
                AddLog($"Ошибка загрузки конфигурации: {ex.Message}", LogType.Error);
            }
            AddLog("Запуск конфигурации", LogType.Information);
            process.StartProcess(ConfigurationManager.config);
            AddLog("Конфигурация запущена", LogType.Information);
            pingManager.Start();
            AddLog("Сервис запущен", LogType.Information);
        }

        private void AddLog(string log, LogType type)
        {
            logger?.AddLog(log, type);
        }

     


        private bool StartServer()
        {
            try
            {
                const string uri = @"net.tcp://localhost:50505/TFOMS_SMEV.svc"; // Адрес, который будет прослушивать сервер
                const string mexUri = @"http://localhost/TFOMS_SMEV.svc";

                var netTcpBinding = new NetTcpBinding
                {
                    ReaderQuotas =
                    {
                        MaxArrayLength = int.MaxValue,
                        MaxBytesPerRead = int.MaxValue,
                        MaxStringContentLength = int.MaxValue
                    },
                    MaxBufferPoolSize = long.MaxValue,
                    MaxReceivedMessageSize = int.MaxValue
                };

                //netTcpBinding.ReliableSession.Enabled = true;
                netTcpBinding.MaxBufferPoolSize = int.MaxValue;
                netTcpBinding.OpenTimeout = new TimeSpan(24, 0, 0);
                netTcpBinding.ReceiveTimeout = new TimeSpan(24, 0, 0);
                netTcpBinding.SendTimeout = new TimeSpan(24, 0, 0);
                wi = new WcfServer(logger, process, ConfigurationManager, pingManager);


                WcfConnection = new ServiceHost(wi, new Uri(uri), new Uri(mexUri));
                var ep = WcfConnection.AddServiceEndpoint(typeof(IWcfInterface), netTcpBinding, uri);
                ep.EndpointBehaviors.Add(new MessageServerBehavior.MessageServerBehavior());


                WcfConnection.OpenTimeout = new TimeSpan(24, 0, 0);
                WcfConnection.CloseTimeout = new TimeSpan(24, 0, 0);
                netTcpBinding.ReceiveTimeout = new TimeSpan(24, 0, 0);


                var smb = WcfConnection.Description.Behaviors.Find<ServiceMetadataBehavior>() ?? new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                
                smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                WcfConnection.Description.Behaviors.Add(smb);
                WcfConnection.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexHttpBinding(), mexUri);

                WcfConnection.Open();
                return true;
            }
            catch (Exception ex)
            {
                AddLog($"Ошибка при запуске WCF: {ex.Message}", LogType.Error);
                return false;
            }
        }
        protected override void OnStop()
        {
            AddLog("Остановка конфигурации", LogType.Information);
            process.StopProcess();
            pingManager.Stop();
            AddLog("Конфигурация остановлена", LogType.Information);
        }
    }

    


 
}
