using CabinetContract;
using SMEV.WCFContract;
using SmevAdapterService.CabinetService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace SmevAdapterService
{
    public class CabinetProcess : IProcess
    {
        ILogger logger;
        ProcessObrTaskParam param;
        IMessageLogger messageLogger;
        IMPAnswer mPAnswer;
        IInforming informing;
        IRegister register;

        public CabinetProcess(ProcessObrTaskParam param, ILogger logger, IMessageLogger messageLogger, IMPAnswer mPAnswer, IInforming informing, IRegister register)
        {
            this.param = param;
            this.logger = logger;
            this.messageLogger = messageLogger;
            this.mPAnswer = mPAnswer;
            this.informing = informing;
            this.register = register;
        }

        public bool IsRunning => host?.State == CommunicationState.Opened;

        public string ItSystem => param.Config.ItSystem;

        public SMEV.WCFContract.VS VS => param.Config.VS;

        public string Text => param.Text;

        public void Resent(int ID)
        {
            throw new NotImplementedException();
        }

        public void StartProcess()
        {
            StartServer();
        }

        ICabinetService wcf;
        ServiceHost host;
        private bool StartServer()
        {
            try
            {
                param.Text = "Запуск WCF";
                const string uri = @"http://localhost:8081/CabinetService.svc"; // Адрес, который будет прослушивать сервер
                var binding = new WSHttpBinding(SecurityMode.None)
                {
                    OpenTimeout = new TimeSpan(24, 0, 0),
                    ReceiveTimeout = new TimeSpan(24, 0, 0),
                    SendTimeout = new TimeSpan(24, 0, 0)
                };
                binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                binding.Security.Message.EstablishSecurityContext = false;
                wcf = new CabinetService.CabinetService(messageLogger, mPAnswer, informing, register, logger);
                host = new ServiceHost(wcf, new Uri(uri)) {OpenTimeout = new TimeSpan(24, 0, 0), CloseTimeout = new TimeSpan(24, 0, 0)};

                #region МЕТАДАННЫЕ
                var smb = host.Description.Behaviors.Find<ServiceMetadataBehavior>() ?? new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                host.Description.Behaviors.Add(smb);
                host.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexHttpBinding(), $"mex");
                #endregion
                host.AddServiceEndpoint(typeof(ICabinetService), binding, new Uri(uri));
                host.Open();
                param.Text = "WCF запущен";
                return true;
            }
            catch (Exception ex)
            {
                logger.AddLog($"Ошибка при запуске WCF: {ex.Message}", LogType.Error);
                param.Text = "Ошибка при запуске WCF";
                return false;
            }
        }

        public void StopProcess()
        {
            param.Text = "Остановка WCF";
            host?.Abort();
            param.Text = "WCF остановлен";
        }
    }
}
