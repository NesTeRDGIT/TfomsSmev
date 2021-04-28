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
namespace SmevAdapterService
{
    public partial class AdapterService : ServiceBase
    {
       
        private Configuration Configuration
        {
            get

            {
                return wi.Config;
            }
            set
            {
                wi.Config = value;
            }
        }


        private PingConfig Config_Ping
        {
            get

            {
                return wi.Config_Ping;
            }
            set
            {
                wi.Config_Ping = value;
            }
        }
        public ServiceHost _serviceHost = null;

        public AdapterService()
        {
            InitializeComponent();
        }
        string config_dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CONFIG");
        string config = "";
        private string PingConfigPath = "";
        protected override void OnStart(string[] args)
        {
            WcfServer.AddLog("Старт службы", EventLogEntryType.Information);
            if (!StartServer())
                Stop();
            try
            {
                config = Path.Combine(config_dir, "config.xml");
                if (!Directory.Exists(config_dir))
                {
                    Directory.CreateDirectory(config_dir);
                }

                if (File.Exists(config))
                {
                    Configuration = Configuration.LoadFromFile(config);
                }
                else
                {
                    Configuration = new Configuration();
                    Configuration.Check();
                    Configuration.SaveToFile(config, Configuration);
                }

                PingConfigPath = Path.Combine(config_dir, "config_ping.xml");
                if (File.Exists(PingConfigPath))
                {
                    Config_Ping = PingConfig.LoadFromFile(PingConfigPath);
                }
                else
                {
                    Config_Ping = new PingConfig();
                    PingConfig.SaveToFile(PingConfigPath, Config_Ping);
                }

                wi.onChangeConfig_PING += Wi_onChangeConfig_PING;
                wi.onChangeConfig += Wi_onChangeConfig;
            }
            catch (Exception ex)
            {
                WcfServer.AddLog($"Ошибка загрузки конфигурации: {ex.Message}", EventLogEntryType.Error);
            }
            WcfServer.AddLog("Запуск конфигурации", EventLogEntryType.Information);           
            StartProcess();
            WcfServer.AddLog("Конфигурация запущена", EventLogEntryType.Information);
            PingStart();
            WcfServer.AddLog("Сервис запущен", EventLogEntryType.Information);
        }

        private void Wi_onChangeConfig_PING()
        {
            PingConfig.SaveToFile(PingConfigPath, Config_Ping);
            WcfServer.AddLog("Перезапуск PING:", EventLogEntryType.Information);
            PingStart();
        }

        private void Wi_onChangeConfig()
        {
            Configuration.SaveToFile(config, Configuration);
            WcfServer.AddLog("Перезапуск Конфигурации(изменение)", EventLogEntryType.Information);
            StopProcess();
            StartProcess();
            WcfServer.AddLog("Конфигурация запущена", EventLogEntryType.Information);
        }

        WcfServer wi;
        public static ServiceHost WcfConection { set; get; }


        private bool StartServer()
        {
            try
            {
                WcfServer.AddLog("Старт WCF сервера", EventLogEntryType.Information);
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
                wi = new WcfServer(new Configuration());
               
                
                WcfConection = new ServiceHost(wi, new Uri(uri), new Uri(mexUri));
                var ep = WcfConection.AddServiceEndpoint(typeof(IWcfInterface), netTcpBinding, uri);
                ep.EndpointBehaviors.Add(new MessageServerBehavior.MessageServerBehavior());
               

                WcfConection.OpenTimeout = new TimeSpan(24, 0, 0);
                WcfConection.CloseTimeout = new TimeSpan(24, 0, 0);
                netTcpBinding.ReceiveTimeout = new TimeSpan(24, 0, 0);


                var smb = WcfConection.Description.Behaviors.Find<ServiceMetadataBehavior>() ?? new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                
                smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                WcfConection.Description.Behaviors.Add(smb);
                WcfConection.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexHttpBinding(), mexUri);

                WcfConection.Open();
                return true;
            }
            catch (Exception ex)
            {
                WcfServer.AddLog("Ошибка при запуске WCF: " + ex.Message, EventLogEntryType.Error);
                return false;
            }
        }

        

        private Dictionary<SMEV.WCFContract.VS, ProcessObr> CurrentWork => wi.CurrentWork;

        private Thread PingTh = null;


        public void PingStart()
        {
            PingStop();
            if (Config_Ping.IsEnabled)
            {
                WcfServer.AddLog("Запуск PING", EventLogEntryType.Information);
                PingTh = new Thread(PingThread) {IsBackground = true};
                PingTh.Start();
                WcfServer.AddLog("PING запущен", EventLogEntryType.Information);
            }
            else
            {
                WcfServer.AddLog("PING отключен", EventLogEntryType.Information);
            }
        }
        public void PingStop()
        {
            PingTh?.Abort();
        }


        public void PingThread()
        {
            try
            {
                while (true)
                {
                    var res = wi.PingAdress();
                    if(!res.Result)
                        wi.SEND_PING_RESULT(res);
                    Thread.Sleep(Config_Ping.TimeOut * 60*1000);
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception e)
            {
                WcfServer.AddLog($"Ошибка в PingThread:{e.Message}", EventLogEntryType.Error);
            }
        }

        void StartProcess()
        {
          
            foreach (var t in Configuration.ListVS)
            {
                Thread th ;
                var cancel = new CancellationTokenSource();
                switch (t.VS)
                {
                    case SMEV.WCFContract.VS.MP:    
                        th = new Thread(Medpom);
                        th.IsBackground = true;                        
                        break;
                    case SMEV.WCFContract.VS.ZAGS:
                        th = new Thread(ZAGS);   
                        th.IsBackground = true;                       
                        break;
                    case SMEV.WCFContract.VS.PFR:
                        th = new Thread(PFR);
                        th.IsBackground = true;
                        break;
                    default:
                        WcfServer.AddLog($"Ошибка запуска конфигурации {t.VS.ToString()} - нет обработчика", EventLogEntryType.Error);
                        continue;                       
                }

                var po = new ProcessObr(th, cancel, t, Configuration.TimeOut*1000);
                if(t.isEnabled)
                    th.Start(po);
                CurrentWork.Add(t.VS, po);
            }
        }
        void StopProcess()
        {
            foreach (var t in CurrentWork)
            {
                if (t.Value.Thread.IsAlive)
                    t.Value.Cancel.Cancel();
            }
            CurrentWork.Clear();
        }
      
        protected override void OnStop()
        {
            WcfServer.AddLog("Остановка конфигурации", EventLogEntryType.Information);
            StopProcess();
            WcfServer.AddLog("Конфигурация остановлена", EventLogEntryType.Information);
        }
        public void Medpom(object obj)
        {
            try
            {
                var po = (ProcessObr)obj;
                var TimeOut = po.TimeOut;
                po.Text = "Начало работы";
                var token = po.Cancel.Token;
                var Config_VS = po.Config;
               
                var mlog = new MessageLogger(Configuration.ConnectionString, Config_VS.ItSystem);

                IRepository fi = null;
                var ConnectionString = Config_VS.ConnectionString;// @"DATA SOURCE=localhost:1521/orcl;PERSIST SECURITY INFO=True;USER ID=asu12;PASSWORD=asu12;CONNECTION TIMEOUT=60";
                po.Text = "Запуск интеграции";
                switch (Config_VS.Integration)
                {
                    case Integration.FileSystem:
                        var fic = FileIntegrationConfig.Get(Config_VS.FilesConfig);
                        fi = new FileIntegration(fic); break;
                    case Integration.DataBase:
                        fi = new DataBaseIntegration(Config_VS.DataBaseConfig.ConnectionString); break;
                }
              
                while (!token.IsCancellationRequested)
                {
                    var mess = fi.GetMessage();
                    foreach (var mes in mess)
                    {
                        po.Text = "Обработка сообщения";
                        try
                        {
                            var adapterInMessage = SeDeserializer<QueryResult>.DeserializeFromXDocument(mes.Content);
                            var MessageId = adapterInMessage.smevMetadata.MessageId;
                            //Если запрос
                            if (adapterInMessage.Message is RequestMessageType)
                            {

                                var rmt = adapterInMessage.Message as RequestMessageType;
                                var ns = rmt.RequestContent.content.MessagePrimaryContent.Name.Namespace;
                                var replyToClientId = rmt.RequestMetadata.clientId;

                                //Определяем тип сообщения

                                if (InputData.XmlnsClass.ToArray().Count(x => x.Namespace == ns.NamespaceName) != 0)
                                {
                                    var val = SeDeserializer<InputData>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    ProcessInputData(fi, mes, mlog, val, MessageId, replyToClientId, adapterInMessage.smevMetadata.Recipient, ConnectionString);
                                    continue;
                                }

                                if (SMEV.VS.MedicalCare.newV1_0_0.ListOfMedicalServicesProvided.InputData.XmlnsClass.ToArray().Count(x => x.Namespace == ns.NamespaceName) != 0)
                                {
                                    var val = SeDeserializer<SMEV.VS.MedicalCare.newV1_0_0.ListOfMedicalServicesProvided.InputData>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    ProcessInputData_V2(fi, mes, mlog, val, MessageId, replyToClientId, adapterInMessage.smevMetadata.Recipient, ConnectionString);
                                    continue;
                                }


                                if (SMEV.VS.MedicalCare.newV1_0_0.FeedbackOnMedicalService.InputData.XmlnsClass.ToArray().Count(x => x.Namespace == ns.NamespaceName) != 0)
                                {
                                    var val = SeDeserializer<SMEV.VS.MedicalCare.newV1_0_0.FeedbackOnMedicalService.InputData>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    ProcessFeedbackOnMedicalService(fi, mes, mlog, val, MessageId, replyToClientId, adapterInMessage.smevMetadata.Recipient, ConnectionString);
                                    continue;
                                }

                                throw new Exception($"Неизвестный тип запроса [{mes.Key}]");
                               
                               
                               

                                /*
                                //Определяем тип сообщения
                                if (InputData.XmlnsClass.ToArray().Count(x => x.Namespace == ns.NamespaceName) != 0)
                                {
                                    inputdate = SeDeserializer<InputData>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    NameType = MessageLoggerVS.InputData;ProcessInputData(fi, mes, mlog, inputdate, MessageId, replyToClientId,adapterInMessage.smevMetadata.Recipient, ConnectionString);
                                }

                                if (SMEV.VS.MedicalCare.newV1_0_0.ListOfMedicalServicesProvided.InputData.XmlnsClass.ToArray().Count(x => x.Namespace == ns.NamespaceName) != 0)
                                {
                                    inputdate = SeDeserializer<SMEV.VS.MedicalCare.newV1_0_0.ListOfMedicalServicesProvided.InputData>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    NameType = MessageLoggerVS.InputData;
                                }




                                if (inputdate == null)
                                {
                                    throw new Exception($"Неизвестный тип запроса [{mes.Key}]");
                                }

                                var idlog = mlog.AddInputMessage(NameType, MessageId, MessageLoggerStatus.INPUT);
                                var replyToClientId = rmt.RequestMetadata.clientId;
                                var clientId = mlog.GetGuidOut();
                                var ms_out = new MessageIntegration { ID = idlog, Key =clientId};
                                mes.ID = idlog;

                                if (inputdate is InputData)
                                {
                                    var req = ((InputData)inputdate).InsuredRenderingListRequest;
                                    mlog.SetMedpomDataIn(idlog, req.FamilyName, req.FirstName, req.Patronymic, req.BirthDate, req.DateFrom, req.DateTo, req.UnitedPolicyNumber);
                                }

                                if (inputdate is SMEV.VS.MedicalCare.newV1_0_0.ListOfMedicalServicesProvided.InputData)
                                {
                                    var req = (SMEV.VS.MedicalCare.newV1_0_0.ListOfMedicalServicesProvided.InputData) inputdate;
                                    mlog.SetMedpomDataIn(idlog, req.FamilyName, req.FirstName, req.Patronymic, req.BirthDate, req.DateFrom, req.DateTo, req.UnitedPolicyNumber);
                                }

                              
                                //Получить ответ
                                var outdate = inputdate.Answer(ConnectionString);


                                if (outdate is OutputData)
                                {
                                    var ou = (OutputData) outdate;
                                    mlog.SetMedpomDataOut(idlog, ou.InsuredRenderingList.Select(x => new Tuple<bool, decimal, decimal, decimal?>(x.IsMTR, x.SLUCH_Z_ID, x.SLUCH_ID, x.USL_ID)).ToList());
                                }

                                if (outdate is SMEV.VS.MedicalCare.newV1_0_0.ListOfMedicalServicesProvided.OutputData)
                                {
                                    var ou = (SMEV.VS.MedicalCare.newV1_0_0.ListOfMedicalServicesProvided.OutputData)outdate;
                                    mlog.SetMedpomDataOut(idlog, ou.InsuredRenderingList.Select(x => new Tuple<bool, decimal, decimal, decimal?>(x.IsMTR, x.SLUCH_Z_ID, x.SLUCH_ID, x.USL_ID)).ToList());
                                    ou.orderId = clientId;
                                }


                                //Отправка ответа  

                                ms_out.Content = AdapterMessageCreator.GenerateAddapterSendRequest(outdate, adapterInMessage.smevMetadata.Recipient, replyToClientId, clientId); 
                                fi.SendMessage(ms_out);
                                fi.EndProcessMessage(mes);
                                mlog.UpdateStatusIN(idlog, MessageLoggerStatus.SUCCESS);
                                mlog.SetOutMessage(idlog, clientId, MessageLoggerStatus.OUTPUT);
                                continue;*/
                            }


                            if (adapterInMessage.Message is ResponseMessageType)
                            {
                                var resp = adapterInMessage.Message as ResponseMessageType;
                                var id = mlog.FindIDByMessageOut(resp.ResponseMetadata.replyToClientId);
                                if (!id.HasValue)
                                {
                                    throw new Exception($"Не удалось найти ID сообщения для [{mes.Key}]");
                                }

                                mes.ID = id.Value;
                                switch (adapterInMessage.Message.messageType)
                                {
                                    case "StatusMessage":
                                        var set = true;
                                        if (resp.ResponseContent.status.description?.ToUpper().Trim() ==
                                            "Сообщение отправлено в СМЭВ".ToUpper().Trim())
                                        {
                                            var st = mlog.GetSTATUS_OUT(id.Value);
                                            if (st != MessageLoggerStatus.OUTPUT)
                                            {
                                                WcfServer.AddLog($"Пропущен статус доставки в СМЭВ для ID = {id.Value}",
                                                    EventLogEntryType.Warning);
                                                set = false;
                                            }
                                        }

                                        if (set)
                                        {
                                            mlog.UpdateStatusOut(id.Value, MessageLoggerStatus.SUCCESS);
                                            mlog.UpdateCommentOut(id.Value, resp.ResponseContent.status.description);
                                        }
                                        fi.ReadMessage(mes);
                                        break;
                                    case "ErrorMessage":
                                        mlog.UpdateStatusOut(id.Value, MessageLoggerStatus.ERROR);
                                        mlog.UpdateCommentOut(id.Value, resp.ResponseContent.status.description);
                                        fi.ReadMessage(mes);
                                        break;
                                    case "RejectMessage":
                                        mlog.UpdateStatusOut(id.Value, MessageLoggerStatus.ERROR);
                                        mlog.UpdateCommentOut(id.Value, string.Join(",", resp.ResponseContent.rejects.Select(x => $"{x.code.ToString()}:{x.description}")));
                                        fi.ReadMessage(mes);
                                        break;
                                    default:

                                        throw new Exception($"Не верный messageType для ResponseMessageType для [{mes.Key}]");
                                }
                                continue;
                            }

                            if (adapterInMessage.Message is ErrorMessage)
                            {
                               
                                var err = adapterInMessage.Message as ErrorMessage;
                                WcfServer.AddLog($"Сообщение об ошибке из СМЭВ в потоке МП: {err.details}", EventLogEntryType.Error);
                                var id = mlog.FindIDByMessageOut(err.statusMetadata.originalClientId);
                                if (!id.HasValue)
                                {
                                    throw new Exception($"Не удалось найти ID сообщения для [{mes.Key}]");
                                }
                                mlog.UpdateStatusOut(id.Value, MessageLoggerStatus.ERROR);
                                fi.ReadMessage(mes);
                                continue;
                            }

                            throw new Exception($"Неизвестный тип сообщения[{mes.Key}]");
                        }
                        catch (Exception ex)
                        {
                            WcfServer.AddLog($"Ошибка потоке обработки МП: {ex.Message} ", EventLogEntryType.Error);
                            fi.ErrorMessage(mes);
                        }

                    }
                    po.Text = "Ожидание сообщения";
                    Thread.Sleep(TimeOut);
                }
            }
            catch(Exception ex)
            {
                WcfServer.AddLog($"Ошибка в потоке Medpom: {ex.Message}", EventLogEntryType.Error);
            }
        }

        public void ProcessInputData(IRepository fi, MessageIntegration mes, MessageLogger mlog, InputData inputdate, string MessageId, string replyToClientId,string ITSystem, string ConnectionString)
        {
            try
            {
                //Данные о человеке
                var idlog = mlog.AddInputMessage(MessageLoggerVS.InputData, MessageId, MessageLoggerStatus.INPUT, "", "");
                var req = inputdate.InsuredRenderingListRequest;
                mlog.SetMedpomDataIn(idlog, req.FamilyName, req.FirstName, req.Patronymic, req.BirthDate, req.DateFrom, req.DateTo, req.UnitedPolicyNumber, null);


                //Ответ из БД
                var outdate = inputdate.Answer(ConnectionString);
                if (outdate != null)
                {
                    var ou = (OutputData)outdate;
                    //Ответ в БД
                    mlog.SetMedpomDataOut(idlog, ou.InsuredRenderingList.Select(x => new MessageLogger.SLUCH_REF(x.IsMTR, x.SLUCH_Z_ID, x.SLUCH_ID, x.USL_ID)).ToList());
                }
                //Исходящие сообщение
                var clientId = mlog.GetGuidOut();
                var ms_out = new MessageIntegration { ID = idlog, Key = clientId };
                mes.ID = idlog;
                ms_out.Content = AdapterMessageCreator.GenerateAddapterSendRequest(outdate, ITSystem, replyToClientId, clientId);
                fi.SendMessage(ms_out);
                fi.EndProcessMessage(mes);
                mlog.UpdateStatusIN(idlog, MessageLoggerStatus.SUCCESS);
                mlog.SetOutMessage(idlog, clientId, MessageLoggerStatus.OUTPUT);
            }
            catch (Exception e)
            {
                throw new Exception($"Ошибка в ProcessInputData: {e.Message}",e);
            }
        }

        public void ProcessInputData_V2(IRepository fi, MessageIntegration mes, MessageLogger mlog,SMEV.VS.MedicalCare.newV1_0_0.ListOfMedicalServicesProvided.InputData inputdate, string MessageId,string replyToClientId, string ITSystem, string ConnectionString)
        {
            try
            {
                //Данные о человеке
                var idlog = mlog.AddInputMessage(MessageLoggerVS.InputData, MessageId, MessageLoggerStatus.INPUT,inputdate.orderId, "");
                var req = inputdate;

                mlog.SetMedpomDataIn(idlog, req.FamilyName, req.FirstName, req.Patronymic, req.BirthDate, req.DateFrom,req.DateTo, req.UnitedPolicyNumber, req.orderId);
                var clientId = mlog.GetGuidOut();
                //Ответ из БД
                var outdate = inputdate.Answer(ConnectionString);
                if (outdate != null)
                {
                    var ou = (SMEV.VS.MedicalCare.newV1_0_0.ListOfMedicalServicesProvided.OutputData) outdate;
                    ou.orderId = req.orderId;
                    //Ответ в БД
                    mlog.SetMedpomDataOut(idlog,
                        ou.InsuredRenderingList.Select(x =>new MessageLogger.SLUCH_REF(x.IsMTR, x.SLUCH_Z_ID, x.SLUCH_ID, x.USL_ID)).ToList());
                }

                //Исходящие сообщение
                var ms_out = new MessageIntegration {ID = idlog, Key = clientId};
                mes.ID = idlog;
                ms_out.Content =AdapterMessageCreator.GenerateAddapterSendRequest(outdate, ITSystem, replyToClientId, clientId);
                fi.SendMessage(ms_out);
                fi.EndProcessMessage(mes);
                mlog.UpdateStatusIN(idlog, MessageLoggerStatus.SUCCESS);
                mlog.SetOutMessage(idlog, clientId, MessageLoggerStatus.OUTPUT);
            }
            catch (Exception e)
            {
                throw new Exception($"Ошибка в ProcessInputData_V2: {e.Message}", e);
            }
        }

        public void ProcessFeedbackOnMedicalService(IRepository fi, MessageIntegration mes, MessageLogger mlog, SMEV.VS.MedicalCare.newV1_0_0.FeedbackOnMedicalService.InputData inputdate, string MessageId, string replyToClientId, string ITSystem, string ConnectionString)
        {
            //Данные о человеке
            var idlog = mlog.AddInputMessage(MessageLoggerVS.FeedbackOnMedicalService, MessageId, MessageLoggerStatus.INPUT, inputdate.orderId,inputdate.ApplicationID);
            mlog.SetFeedbackINFO(inputdate, idlog);
            fi.EndProcessMessage(mes);
            mlog.UpdateStatusIN(idlog, MessageLoggerStatus.SUCCESS);
        }

        public void ZAGS(object obj)
        {
            try
            {
                var po = (ProcessObr)obj;
                var TimeOut = po.TimeOut;
                po.Text = "Начало работы";
                var token = po.Cancel.Token;
                var Config_VS = po.Config;
                var mlog = new MessageLogger(Configuration.ConnectionString, Config_VS.ItSystem);

                IRepository fi = null;

                po.Text = "Запуск интеграции";
                switch (Config_VS.Integration)
                {
                    case Integration.FileSystem:
                        var fic = FileIntegrationConfig.Get(Config_VS.FilesConfig);
                        fi = new FileIntegration(fic); break;
                    case Integration.DataBase:
                        fic = new FileIntegrationConfig();
                        fi = new FileIntegration(fic); break;
                }
                while (!token.IsCancellationRequested)
                {
                 
                    var mess = fi.GetMessage();
                    foreach (var mes in mess)
                    {
                        po.Text = "Обработка сообщения";
                        try
                        {
                            var adapterInMessage = SeDeserializer<QueryResult>.DeserializeFromXDocument(mes.Content);
                            //Если запрос
                            if (adapterInMessage.Message is RequestMessageType)
                            {
                                var rmt = adapterInMessage.Message as RequestMessageType;
                                var ns = rmt.RequestContent.content.MessagePrimaryContent.Name.Namespace;
                                IRequestMessage inputdate = null;
                                var NameType = MessageLoggerVS.UNKNOW;
                                //Определяем тип сообщения
                                if (Request_BRAKZRZP.XmlnsClass.ToArray().Count(x => x.Namespace == ns.NamespaceName) != 0)
                                {
                                    inputdate = SeDeserializer<Request_BRAKZRZP>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    NameType = MessageLoggerVS.Request_BRAKZRZP;
                                }
                                if (Request_BRAKRZP.XmlnsClass.ToArray().Count(x => x.Namespace == ns.NamespaceName) != 0)
                                {
                                    inputdate = SeDeserializer<Request_BRAKRZP>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    NameType = MessageLoggerVS.Request_BRAKRZP;
                                }
                                if (Request_BRAKZZP.XmlnsClass.ToArray().Count(x => x.Namespace == ns.NamespaceName) != 0)
                                {
                                    inputdate = SeDeserializer<Request_BRAKZZP>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    NameType = MessageLoggerVS.Request_BRAKZZP;
                                }
                                if (Request_FATALZP.XmlnsClass.ToArray().Count(x => x.Namespace == ns.NamespaceName) != 0)
                                {
                                    inputdate = SeDeserializer<Request_FATALZP>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    NameType = MessageLoggerVS.Request_FATALZP;
                                }
                                if (Zags4_0_1.Request_FATALZP.XmlnsClass.ToArray().Count(x => x.Namespace == ns.NamespaceName) != 0)
                                {
                                    inputdate = SeDeserializer<Zags4_0_1.Request_FATALZP>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    NameType = MessageLoggerVS.Request_FATALZP4_0_1;
                                }
                                if (Request_PARENTZP.XmlnsClass.ToArray().Count(x => x.Namespace == ns.NamespaceName) != 0)
                                {
                                    inputdate = SeDeserializer<Request_PARENTZP>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    NameType = MessageLoggerVS.Request_PARENTZP;
                                }
                                if (Zags4_0_1.Request_PARENTZP.XmlnsClass.ToArray().Count(x => x.Namespace == ns.NamespaceName) != 0)
                                {
                                    inputdate = SeDeserializer<Zags4_0_1.Request_PARENTZP>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    NameType = MessageLoggerVS.Request_PARENTZP4_0_1;
                                }
                                if (Request_PERNAMEZP.XmlnsClass.ToArray().Count(x => x.Namespace == ns.NamespaceName) != 0)
                                {
                                    inputdate = SeDeserializer<Request_PERNAMEZP>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    NameType = MessageLoggerVS.Request_PERNAMEZP;
                                }
                                if (Zags4_0_1.Request_PERNAMEZP.XmlnsClass.ToArray().Count(x => x.Namespace == ns.NamespaceName) != 0)
                                {
                                    inputdate = SeDeserializer<Zags4_0_1.Request_PERNAMEZP>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    NameType = MessageLoggerVS.Request_PERNAMEZP4_0_1;
                                }
                                if (Request_ROGDZP.XmlnsClass.ToArray().Count(x => x.Namespace == ns.NamespaceName) != 0)
                                {
                                    inputdate = SeDeserializer<Request_ROGDZP>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    NameType = MessageLoggerVS.Request_ROGDZP;
                                }
                                if (Zags4_0_1.Request_ROGDZP.XmlnsClass.ToArray().Count(x => x.Namespace == ns.NamespaceName) != 0)
                                {
                                    inputdate = SeDeserializer<Zags4_0_1.Request_ROGDZP>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    NameType = MessageLoggerVS.Request_ROGDZP4_0_1;
                                }
                                //Если не известный тип
                                if (inputdate == null)
                                {
                                    throw new Exception($"Неизвестный тип запроса [{mes.Key}]");
                                }
                                var MessageId = adapterInMessage.smevMetadata.MessageId;
                                var idlog = mlog.AddInputMessage(NameType, MessageId, MessageLoggerStatus.INPUT,"","");
                                mes.ID = idlog;
                                var replyToClientId = rmt.RequestMetadata.clientId;
                                var clientId = mlog.GetGuidOut();
                                var ms_out = new MessageIntegration { ID = idlog, Key = clientId };

                                if (!string.IsNullOrEmpty(Config_VS.TranspotrMessage))
                                {
                                    var Dir = Path.Combine(Config_VS.TranspotrMessage, NameType.ToString(), DateTime.Now.ToString("yyyyMMdd"));
                                    if (!Directory.Exists(Dir)) Directory.CreateDirectory(Dir);
                                    var path = Path.Combine(Dir,$"[{mes.Key}][REQ][RAW][{DateTime.Now.Hour.ToString()}-{DateTime.Now.Minute.ToString()}].xml");
                                    rmt.RequestContent.content.MessagePrimaryContent.Save(path);
                                    mlog.UpdateStatusIN(idlog, MessageLoggerStatus.INPUT);
                                    mlog.UpdateComentIN(idlog, "SAVE: " + path);
                                }
                                //Получить ответ
                                var outdate = inputdate.Answer(Config_VS.ConnectionString);

                                ms_out.Content = AdapterMessageCreator.GenerateAddapterSendRequest(outdate, adapterInMessage.smevMetadata.Recipient, replyToClientId, clientId);
                                //Отправка ответа  
                                fi.SendMessage(ms_out);
                                fi.EndProcessMessage(mes);
                                mlog.UpdateStatusIN(idlog, MessageLoggerStatus.SUCCESS);
                                mlog.SetOutMessage(idlog, clientId, MessageLoggerStatus.OUTPUT);
                                continue;
                            }
                            if (adapterInMessage.Message is ResponseMessageType)
                            {

                                var resp = adapterInMessage.Message as ResponseMessageType;
                                if (string.IsNullOrEmpty(resp.ResponseMetadata.replyToClientId) && resp.ResponseContent.status.description?.ToUpper().Trim() == "Successfully queued".ToUpper().Trim())
                                {
                                    WcfServer.AddLog($"Косяк ЗАГС в СМЭВ нет replyToClientId", EventLogEntryType.Warning);
                                    fi.ErrorMessage(mes, "_NOTreply");
                                    continue;
                                }
                                var id = mlog.FindIDByMessageOut(resp.ResponseMetadata.replyToClientId);
                                if (!id.HasValue)
                                {
                                    throw new Exception($"Не удалось найти ID сообщения для [{mes.Key}]");
                                }
                                mes.ID = id.Value;
                               
                                switch (adapterInMessage.Message.messageType)
                                {
                                    case "StatusMessage":
                                        var set = true;
                                        if (resp.ResponseContent.status.description?.ToUpper().Trim() == "Сообщение отправлено в СМЭВ".ToUpper().Trim())
                                        {
                                            var st = mlog.GetSTATUS_OUT(id.Value);
                                            if (st != MessageLoggerStatus.OUTPUT)
                                            {
                                                WcfServer.AddLog($"Пропущен статус доставки в СМЭВ для ID = {id.Value}",EventLogEntryType.Warning);
                                                set = false;
                                            }
                                        }

                                        if (set)
                                        {
                                            mlog.UpdateStatusOut(id.Value, MessageLoggerStatus.SUCCESS);
                                            mlog.UpdateCommentOut(id.Value, resp.ResponseContent.status.description);
                                        }
                                        fi.ReadMessage(mes);
                                        break;
                                    case "ErrorMessage":
                                        mlog.UpdateStatusOut(id.Value, MessageLoggerStatus.ERROR);
                                        mlog.UpdateCommentOut(id.Value, resp.ResponseContent.status.description);
                                        fi.ReadMessage(mes);
                                        break;
                                    case "RejectMessage":
                                        mlog.UpdateStatusOut(id.Value, MessageLoggerStatus.ERROR);
                                        mlog.UpdateCommentOut(id.Value, string.Join(",",resp.ResponseContent.rejects.Select(x =>$"{x.code.ToString()}:{x.description}"))) ;
                                        fi.ReadMessage(mes);
                                        break;
                                    default:
                                        throw new Exception($"Не верный messageType для ResponseMessageType для [{mes.Key}]");
                                }
                                continue;
                            }

                            if (adapterInMessage.Message is ErrorMessage)
                            {
                                var err = adapterInMessage.Message as ErrorMessage;
                                WcfServer.AddLog($"Сообщение об ошибке из СМЭВ в потоке ЗАГС: {err.details}", EventLogEntryType.Error);
                                var id = mlog.FindIDByMessageOut(err.statusMetadata.originalClientId);
                                if (!id.HasValue)
                                {
                                    throw new Exception($"Не удалось найти ID сообщения для [{mes.Key}]");
                                }
                                mlog.UpdateStatusOut(id.Value, MessageLoggerStatus.ERROR);
                                fi.ReadMessage(mes);
                                continue;
                            }
                            throw new Exception($"Неизвестный тип сообщения [{mes.Key}]");
                        }
                        catch (Exception ex)
                        {
                            WcfServer.AddLog($"Ошибка потоке обработки ЗАГС: {ex.Message} ", EventLogEntryType.Error);
                            fi.ErrorMessage(mes);
                        }

                    }
                    po.Text = "Ожидание сообщения";
                    Thread.Sleep(TimeOut);
                }
            }
            catch (Exception ex)
            {
                WcfServer.AddLog($"Ошибка в потоке ЗАГС: {ex.Message}", EventLogEntryType.Error);
            }
        }
        public void PFR(object obj)
        {
            try
            {
                var po = (ProcessObr)obj;
                var TimeOut = po.TimeOut;
                po.Text = "Начало работы";
                var token = po.Cancel.Token;
                var Config_VS = po.Config;
                var mlog = new MessageLogger(Configuration.ConnectionString, Config_VS.ItSystem);
                IRepository fi = null;
                po.Text = "Запуск интеграции";
                switch (Config_VS.Integration)
                {
                    case Integration.FileSystem:
                        var fic = FileIntegrationConfig.Get(Config_VS.FilesConfig);
                        fi = new FileIntegration(fic); break;
                    case Integration.DataBase:
                        fic = new FileIntegrationConfig();
                        fi = new FileIntegration(fic); break;
                }

                while (!token.IsCancellationRequested)
                {
                    foreach (var file in GetMessagePFPOut(Config_VS.UserOutMessage))
                    {
                        var clientId = mlog.GetGuidOut();
                        var send = CreatePFRData(file, Config_VS.ItSystem, clientId);
                        var id = mlog.AddInputMessage(MessageLoggerVS.PFR_SNILS, "", MessageLoggerStatus.NONE, "","");
                        mlog.SetOutMessage(id, clientId, MessageLoggerStatus.OUTPUT);
                        mlog.UpdateCommentOut(id, $"FILE: {file}");
                        var ms = new MessageIntegration {Key = clientId, ID = id, Content = send.SerializeToX()};
                        fi.SendMessage(ms);
                        var dirArc = Path.Combine(Config_VS.FilesConfig.ArchiveFolder, DateTime.Now.ToString("yyyy_MM_dd"), "UserOut");
                        if (!Directory.Exists(dirArc))
                            Directory.CreateDirectory(dirArc);

                        File.Move(file, Path.Combine(dirArc, $"{id}_{Path.GetFileName(file)}"));
                    }

                    var mess = fi.GetMessage();
                    foreach (var mes in mess)
                    {
                        po.Text = "Обработка сообщения";
                        try
                        {

                            var adapterInMessage = SeDeserializer<QueryResult>.DeserializeFromXDocument(mes.Content);
                            //Если запрос
                            if (adapterInMessage.Message is ResponseMessageType)
                            {
                                var resp = adapterInMessage.Message as ResponseMessageType;
                                var id = mlog.FindIDByMessageOut(resp.ResponseMetadata.replyToClientId);
                                if (!id.HasValue)
                                {
                                    throw new Exception($"Не удалось найти ID сообщения для [{mes.Key}]");
                                }
                                mes.ID = id.Value;
                                var Dir = Path.Combine(Config_VS.TranspotrMessage, DateTime.Now.ToString("yyyyMMdd"));
                                if (!Directory.Exists(Dir)) Directory.CreateDirectory(Dir);

                                var rmt = adapterInMessage.Message as ResponseMessageType;

                                switch (adapterInMessage.Message.messageType)
                                {
                                    case "StatusMessage":
                                        var set = true;
                                        if (resp.ResponseContent.status.description?.ToUpper().Trim() ==
                                            "Сообщение отправлено в СМЭВ".ToUpper().Trim())
                                        {
                                            var st = mlog.GetSTATUS_OUT(id.Value);
                                            if (st != MessageLoggerStatus.OUTPUT)
                                            {
                                                WcfServer.AddLog($"Пропущен статус доставки в СМЭВ для ID = {id.Value}",
                                                    EventLogEntryType.Warning);
                                                set = false;
                                            }
                                        }
                                        if (set)
                                        {
                                            mlog.UpdateStatusOut(id.Value, MessageLoggerStatus.SUCCESS);
                                            mlog.UpdateCommentOut(id.Value, resp.ResponseContent.status.description);
                                        }

                                        fi.ReadMessage(mes);
                                        break;
                                    case "ErrorMessage":
                                        mlog.UpdateStatusOut(id.Value, MessageLoggerStatus.ERROR);
                                        mlog.UpdateCommentOut(id.Value, resp.ResponseContent.status.description);
                                        var path = Path.Combine(Dir, $"[{mes.Key}][ERR][RAW][{DateTime.Now.Hour.ToString()}-{DateTime.Now.Minute.ToString()}].xml");
                                        rmt.ResponseContent.SerializeToX().Save(path);
                                        fi.ReadMessage(mes);
                                        break;
                                    case "RejectMessage":
                                        mlog.UpdateStatusOut(id.Value, MessageLoggerStatus.ERROR);
                                        mlog.UpdateCommentOut(id.Value, string.Join(",", resp.ResponseContent.rejects.Select(x => $"{x.code.ToString()}:{x.description}")));
                                        path = Path.Combine(Dir, $"[{mes.Key}][REJ][RAW][{DateTime.Now.Hour.ToString()}-{DateTime.Now.Minute.ToString()}].xml");
                                        rmt.ResponseContent.SerializeToX().Save(path);
                                        fi.ReadMessage(mes);
                                        break;
                                    case "PrimaryMessage":
                                        mlog.SetINMessage(id.Value, mes.Key, MessageLoggerStatus.INPUT);
                                         path = Path.Combine(Dir, $"[{mes.Key}][RES][RAW][{DateTime.Now.Hour.ToString()}-{DateTime.Now.Minute.ToString()}].xml");
                                        rmt.ResponseContent.content.MessagePrimaryContent.Save(path);
                                        mlog.UpdateStatusIN(id.Value, MessageLoggerStatus.SUCCESS);
                                        mlog.UpdateComentIN(id.Value, "SAVE: " + path);
                                        fi.EndProcessMessage(mes);
                                        break;
                                    default:
                                        throw new Exception($"Не верный messageType для ResponseMessageType для [{mes.Key}]");
                                }
                                continue;
                            }

                            if (adapterInMessage.Message is ErrorMessage)
                            {
                                var err = adapterInMessage.Message as ErrorMessage;
                                WcfServer.AddLog($"Сообщение об ошибке из СМЭВ в потоке ПФР: {err.details}", EventLogEntryType.Error);
                                var id = mlog.FindIDByMessageOut(err.statusMetadata.originalClientId);
                                if (!id.HasValue)
                                {
                                    throw new Exception($"Не удалось найти ID сообщения для [{mes.Key}]");
                                }
                                mlog.UpdateStatusOut(id.Value, MessageLoggerStatus.ERROR);
                                fi.ReadMessage(mes);
                                continue;
                            }
                            throw new Exception($"Неизвестный тип сообщения [{mes.Key}]");
                        }
                        catch (Exception ex)
                        {
                            WcfServer.AddLog($"Ошибка потоке обработки ПФР: {ex.Message} ", EventLogEntryType.Error);
                            fi.ErrorMessage(mes);
                        }

                    }
                    po.Text = "Ожидание сообщения";
                    Thread.Sleep(TimeOut);
                }
            }
            catch (Exception ex)
            {
                WcfServer.AddLog($"Ошибка в потоке ПФР: {ex.Message}", EventLogEntryType.Error);
            }
        }

       
        public List<string> GetMessagePFPOut(string DIR)
        {
            try
            {
              return  Directory.GetFiles(DIR, "*.xml").ToList();
           
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка в GetMessagePFPOut: {ex.Message}", ex);
            }
        }

        private SendRequest CreatePFRData(string Path,string ITSYSTEM, string reply)
        {
            var root = XDocument.Load(Path);
            var ad = new SendRequest()
            {
                itSystem = ITSYSTEM,

                RequestMessage = new RequestMessageType()
                {
                    RequestMetadata = new RequestMetadataType()
                    {
                        clientId = reply
                    },
                    RequestContent = new RequestContentType()
                    {
                        content = new Content()
                        {
                            MessagePrimaryContent = root.Root
                        }
                    }
                }
            };
            return ad;
        }
    }

    


    public class MessageLogger
    {
        string ItSystem;
        string ConnectionString;
        public MessageLogger(string ConnectionString, string ItSystem)
        {
            this.ItSystem = ItSystem;
            this.ConnectionString = ConnectionString;
        }
        /// <summary>
        /// Дабавить входящее сообщение
        /// </summary>
        /// <param name="VS"></param>
        /// <param name="ID_MESSAGE"></param>
        /// <param name="status"></param>
        /// <param name="Comment"></param>
        /// <returns></returns>
        public int AddInputMessage(MessageLoggerVS VS, string id_message_in, MessageLoggerStatus status_in, string orderid, string applicationid,  string comment_in ="")
        {
            try
            {
                using (var con = new NpgsqlConnection(ConnectionString))
                {
                    using (var cmd = new NpgsqlCommand(@"insert INTO  log_service
                                                    (id_message_in, itsystem, vs, status_in, comment_in, date_in, orderid, applicationid)
                                                    values
                                                    (@id_message_in, @itsystem, @vs, @status_in, @comment_in, @date_in, @orderid, @applicationid) RETURNING id", con))
                    {
                        

                        cmd.Parameters.Add(new NpgsqlParameter("id_message_in", id_message_in));
                        cmd.Parameters.Add(new NpgsqlParameter("itsystem", ItSystem));
                        cmd.Parameters.Add(new NpgsqlParameter("vs", (int)VS));
                        cmd.Parameters.Add(new NpgsqlParameter("status_in",(int)status_in));
                        cmd.Parameters.Add(new NpgsqlParameter("comment_in", comment_in));
                        cmd.Parameters.Add(new NpgsqlParameter("date_in", DateTime.Now));
                        cmd.Parameters.Add(new NpgsqlParameter("orderid", orderid));
                        cmd.Parameters.Add(new NpgsqlParameter("applicationid", applicationid));
                        con.Open();
                        var res = cmd.ExecuteScalar();
                        con.Close();
                        var id = Convert.ToInt32(res);
                        return id;
                    }
                }
            }
            catch (Exception ex)
            {
                WcfServer.AddLog(
                    $"Ошибка вставки истории AddInputMessage[ItSystem[{ItSystem}],VS[{VS}],ID_MESSAGE[{id_message_in}]] : {ex.Message}", EventLogEntryType.Error);
                return -5;
            }
        }
        /// <summary>
        /// Ответ на сообщение
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="id_message_out"></param>
        public void SetOutMessage(int ID, string id_message_out, MessageLoggerStatus status_out)
        {

            try
            {
                var con = new NpgsqlConnection(ConnectionString);
                var cmd = new NpgsqlCommand(@"update log_service t set id_message_out =  @id_message_out, status_out = @status_out,date_out= @date_out
                                                   where t.ID = @ID", con);
                cmd.Parameters.Add(new NpgsqlParameter("id_message_out", id_message_out));
                cmd.Parameters.Add(new NpgsqlParameter("status_out", (int)status_out));
                cmd.Parameters.Add(new NpgsqlParameter("date_out", DateTime.Now));
                
                cmd.Parameters.Add(new NpgsqlParameter("ID", ID));
                con.Open();
                var x = cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                WcfServer.AddLog(
                    $"Ошибка обновления истории SetOutMessage[ID[{ID}],id_message_out[{id_message_out}]] : {ex.Message}", EventLogEntryType.Error);
            }
        }

        public void SetINMessage(int ID, string id_message_in, MessageLoggerStatus status_in)
        {

            try
            {
                var con = new NpgsqlConnection(ConnectionString);
                var cmd = new NpgsqlCommand(@"update log_service t set id_message_in =  @id_message_in, status_in = @status_in,date_in= @date_in
                                                   where t.ID = @ID", con);
                cmd.Parameters.Add(new NpgsqlParameter("id_message_in", id_message_in));
                cmd.Parameters.Add(new NpgsqlParameter("status_in", (int)status_in));
                cmd.Parameters.Add(new NpgsqlParameter("date_in", DateTime.Now));

                cmd.Parameters.Add(new NpgsqlParameter("ID", ID));
                con.Open();
                var x = cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                WcfServer.AddLog(
                    $"Ошибка обновления истории SetOutMessage[ID[{ID}],id_message_out[{status_in}]] : {ex.Message}", EventLogEntryType.Error);
            }
        }
        public void UpdateStatusIN(int ID, MessageLoggerStatus status_in)
        {          
            try
            {
                var con = new NpgsqlConnection(ConnectionString);

                var cmd = new NpgsqlCommand(@"update log_service t set status_in =  @status_in
                                                   where t.ID = @ID", con);

                cmd.Parameters.Add(new NpgsqlParameter("status_in", (int)status_in));    
                cmd.Parameters.Add(new NpgsqlParameter("ID", ID));
                con.Open();
                var x = cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                WcfServer.AddLog($"Ошибка обновления истории UpdateStatusMessage[ID[{ID}]] : {ex.Message}", EventLogEntryType.Error);
            }
        }
        public void UpdateComentIN(int ID, string comment_in)
        {
            try
            {
                var con = new NpgsqlConnection(ConnectionString);

                var cmd = new NpgsqlCommand(@"update log_service t set comment_in =  @comment_in 
                                                   where t.ID = @ID", con);
      
                cmd.Parameters.Add(new NpgsqlParameter("comment_in", comment_in));
                cmd.Parameters.Add(new NpgsqlParameter("ID", ID));
                con.Open();
                var x = cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                WcfServer.AddLog($"Ошибка обновления истории UpdateStatusMessage[ID[{ID}]] : {ex.Message}", EventLogEntryType.Error);
            }
        }

        public void UpdateStatusOut(int ID, MessageLoggerStatus status_out)
        {
            NpgsqlTransaction tran = null;
            try
            {
                var con = new NpgsqlConnection(ConnectionString);

                var cmd = new NpgsqlCommand(@"update log_service t set status_out =  @status_out where t.ID = @ID", con);
                cmd.Parameters.Add(new NpgsqlParameter("status_out", (int)status_out));
                cmd.Parameters.Add(new NpgsqlParameter("ID", ID));              
                con.Open();
                tran = con.BeginTransaction();
                var x = cmd.ExecuteNonQuery();

                if (x != 1)
                {
                    throw new Exception($"Попытка изменить {x} строк(и)");
                }
                tran.Commit();
                con.Close();
            }
            catch (Exception ex)
            {
                tran?.Rollback();
                WcfServer.AddLog($"Ошибка обновления истории UpdateStatusOutMessage[ID[{ID}]] : {ex.Message}", EventLogEntryType.Error);
            }
        }
        public void UpdateCommentOut(int ID, string comment_out)
        {
            NpgsqlTransaction tran = null;
            try
            {
                var con = new NpgsqlConnection(ConnectionString);

                var cmd = new NpgsqlCommand(@"update log_service t set comment_out =  @comment_out 
                                                   where t.ID = @ID", con);



                cmd.Parameters.Add(new NpgsqlParameter("comment_out", comment_out));
                cmd.Parameters.Add(new NpgsqlParameter("ID", ID));
                con.Open();
                tran = con.BeginTransaction();
                var x = cmd.ExecuteNonQuery();

                if (x != 1)
                {
                    throw new Exception(string.Format("Попытка изменить {0} строк(и)", x));
                }
                tran.Commit();
                con.Close();
            }
            catch (Exception ex)
            {
                tran?.Rollback();
                WcfServer.AddLog($"Ошибка обновления истории UpdateStatusOutMessage[ID[{ID}]] : {ex.Message}", EventLogEntryType.Error);
            }
        }

        public MessageLoggerStatus? GetSTATUS_OUT(int ID)
        {
            try
            {
                var con = new NpgsqlConnection(ConnectionString);
                var cmd = new NpgsqlDataAdapter(@"select status_out from log_service t where t.ID = @ID", con);
                cmd.SelectCommand.Parameters.Add(new NpgsqlParameter("ID", ID));
                var tbl = new DataTable();
                cmd.Fill(tbl);
                return tbl.Rows.Count != 0
                    ? (MessageLoggerStatus?) (MessageLoggerStatus) Convert.ToInt32(tbl.Rows[0][0])
                    : null;
            }
            catch (Exception ex)
            {
                WcfServer.AddLog($"Ошибка обновления истории GetSTATUS_OUT[ID[{ID}]] : {ex.Message}", EventLogEntryType.Error);
                return null;
            }
        }

        public string GetGuidOut()
        {
            try
            {
                using (var con = new NpgsqlConnection(ConnectionString))
                {
                    using (var cmd = new NpgsqlCommand(@"select count(*) from log_service t 
                                          where t.ID_MESSAGE_OUT = @ID_MESSAGE_OUT and t.ItSystem = @ItSystem", con))
                    {
                        cmd.Parameters.Add(new NpgsqlParameter("ID_MESSAGE_OUT", ""));
                        cmd.Parameters.Add(new NpgsqlParameter("ItSystem", ItSystem));
                        var t = "";
                        con.Open();
                        while (true)
                        {
                            t = Guid.NewGuid().ToString();
                            cmd.Parameters["ID_MESSAGE_OUT"].Value = t;
                            var c = cmd.ExecuteScalar();
                            if (Convert.ToInt32(c) == 0)
                                break;
                            ;
                        }
                        con.Close();
                        return t;
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Ошибка в GetGuidOut: {0}", ex.Message, ex));
            }



        }
       
        public int? FindIDByMessageOut(string id_message_out)
        {
            try
            {
                using (var con = new NpgsqlConnection(ConnectionString))
                {
                    using (var cmd = new NpgsqlCommand(@"select max(ID) from log_service t 
                                          where t.id_message_out = @ID_MESSAGE_OUT and t.ItSystem = @ItSystem", con))
                    {
                        cmd.Parameters.Add(new NpgsqlParameter("ID_MESSAGE_OUT", id_message_out));
                        cmd.Parameters.Add(new NpgsqlParameter("ItSystem", ItSystem));
                        con.Open();
                        var c = cmd.ExecuteScalar();
                        con.Close();
                        if (c != DBNull.Value)
                            return Convert.ToInt32(c);
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Ошибка в FindIDByMessageOut: {0}", ex.Message, ex));
            }

        }

        public void SetMedpomDataIn(int log_service_id, string familyname, string firstname, string patronymic, DateTime birthdate, DateTime datefrom, DateTime dateto, string unitedpolicynumber,string orderId)
        {
            try
            {
                using (var con = new NpgsqlConnection(ConnectionString))
                {
                    using (var cmd = new NpgsqlCommand(@"INSERT INTO  public.medpom_data_in
(log_service_id,  familyname,  firstname,  patronymic,  birthdate,  datefrom,  dateto,  unitedpolicynumber,orderId)
VALUES (@log_service_id,@familyname, @firstname, @patronymic, @birthdate, @datefrom, @dateto, @unitedpolicynumber,@orderId)", con))
                    {
                        cmd.Parameters.Add(new NpgsqlParameter("log_service_id", log_service_id));
                        cmd.Parameters.Add(new NpgsqlParameter("familyname", string.IsNullOrEmpty(familyname) ? (object)DBNull.Value : familyname));
                        cmd.Parameters.Add(new NpgsqlParameter("firstname", string.IsNullOrEmpty(firstname) ? (object)DBNull.Value : firstname));
                        cmd.Parameters.Add(new NpgsqlParameter("patronymic", string.IsNullOrEmpty(patronymic) ? (object)DBNull.Value : patronymic));
                        cmd.Parameters.Add(new NpgsqlParameter("birthdate", birthdate));
                        cmd.Parameters.Add(new NpgsqlParameter("datefrom", datefrom));
                        cmd.Parameters.Add(new NpgsqlParameter("dateto", dateto));
                        cmd.Parameters.Add(new NpgsqlParameter("unitedpolicynumber", string.IsNullOrEmpty(unitedpolicynumber) ? (object)DBNull.Value : unitedpolicynumber));
                        cmd.Parameters.Add(new NpgsqlParameter("orderId", string.IsNullOrEmpty(orderId) ? (object)DBNull.Value : orderId));

                        con.Open();
                        var c = cmd.ExecuteNonQuery();
                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Ошибка в SetMedpomDataIn: {0}", ex.Message, ex));
            }
        }

        public void SetFeedbackINFO(SMEV.VS.MedicalCare.newV1_0_0.FeedbackOnMedicalService.InputData InputData, int log_service_id)
        {
            try
            {
                using (var con = new NpgsqlConnection(ConnectionString))
                {
                    using (var cmd = new NpgsqlCommand(@"INSERT INTO public.FeedbackINFO(AppealNumber,LOG_SERVICE_ID,AppealTopic,AppealDetail,MedServicesID, AppealTopicCode,CareRegimen,DateRenderingFrom,
                        DateRenderingTo,RegionCode, CareType,MedServicesSum)
                    VALUES(@AppealNumber,@LOG_SERVICE_ID,@AppealTopic,@AppealDetail,@MedServicesID,@AppealTopicCode,@CareRegimen,@DateRenderingFrom,
                           @DateRenderingTo, @RegionCode,@CareType,@MedServicesSum)", con))
                    {
                        cmd.Parameters.Add(new NpgsqlParameter("AppealNumber", NpgsqlDbType.Varchar));
                        cmd.Parameters.Add(new NpgsqlParameter("LOG_SERVICE_ID", log_service_id));
                        cmd.Parameters.Add(new NpgsqlParameter("AppealTopic",NpgsqlDbType.Varchar));
                        cmd.Parameters.Add(new NpgsqlParameter("AppealDetail", NpgsqlDbType.Varchar));
                        cmd.Parameters.Add(new NpgsqlParameter("MedServicesID", NpgsqlDbType.Varchar));
                        cmd.Parameters.Add(new NpgsqlParameter("AppealTopicCode", NpgsqlDbType.Integer));
                        cmd.Parameters.Add(new NpgsqlParameter("CareRegimen", NpgsqlDbType.Varchar));
                        cmd.Parameters.Add(new NpgsqlParameter("DateRenderingFrom", NpgsqlDbType.Date));
                        cmd.Parameters.Add(new NpgsqlParameter("DateRenderingTo", NpgsqlDbType.Date));
                        cmd.Parameters.Add(new NpgsqlParameter("RegionCode", NpgsqlDbType.Varchar));
                        cmd.Parameters.Add(new NpgsqlParameter("CareType", NpgsqlDbType.Varchar));
                        cmd.Parameters.Add(new NpgsqlParameter("MedServicesSum", NpgsqlDbType.Numeric));
        

                        con.Open();
                        foreach (var item in InputData.InsuredAppealList)
                        {
                            cmd.Parameters["AppealNumber"].Value = item.AppealNumber;
                            cmd.Parameters["AppealTopic"].Value = item.AppealTopic;
                            cmd.Parameters["AppealDetail"].Value = item.AppealDetail;
                            cmd.Parameters["MedServicesID"].Value = item.MedServicesID;
                            cmd.Parameters["AppealTopicCode"].Value = item.AppealTopicCode;
                            cmd.Parameters["CareRegimen"].Value = item.CareRegimen;
                            cmd.Parameters["DateRenderingFrom"].Value = item.DateRenderingFrom;
                            cmd.Parameters["DateRenderingTo"].Value = item.DateRenderingTo;
                            cmd.Parameters["RegionCode"].Value = item.RegionCode;
                            cmd.Parameters["CareType"].Value = item.CareType;
                            cmd.Parameters["MedServicesSum"].Value = item.MedServicesSum;
                            cmd.ExecuteNonQuery();
                        }
                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка в SetFeedbackINFO: {ex.Message}");
            }
        }

        public class SLUCH_REF
        {
            public SLUCH_REF(bool _IsMTR, int _SLUCH_Z_ID, int _SLUCH_ID, int? _USL_ID)
            {
                IsMTR = _IsMTR;
                SLUCH_Z_ID = _SLUCH_Z_ID;
                SLUCH_ID = _SLUCH_ID;
                USL_ID = _USL_ID;

            }
            public bool IsMTR { get; set; }
            public int SLUCH_Z_ID { get; set; }
            public int SLUCH_ID { get; set; }
            public int? USL_ID { get; set; }
        }

        public void SetMedpomDataOut(int log_service_id, List<SLUCH_REF> IDs)
        {
            try
            {
                using (var con = new NpgsqlConnection(ConnectionString))
                {
                    using (var cmd = new NpgsqlCommand(@"INSERT INTO public.medpom_data_out
(log_service_id,  sluch_z_id,  sluch_id,  usl_id, ismtr)
VALUES 
(@log_service_id,@sluch_z_id,@sluch_id,@usl_id, @ismtr)", con))
                    {
                        cmd.Parameters.Add(new NpgsqlParameter("log_service_id", log_service_id));
                        cmd.Parameters.Add(new NpgsqlParameter("sluch_z_id", 1));
                        cmd.Parameters.Add(new NpgsqlParameter("sluch_id", 1));
                        cmd.Parameters.Add(new NpgsqlParameter("usl_id", DBNull.Value));
                        cmd.Parameters.Add(new NpgsqlParameter("ismtr", DBNull.Value));

                        con.Open();
                        foreach (var id in IDs)
                        {
                            cmd.Parameters["ismtr"].Value = id.IsMTR;
                            cmd.Parameters["sluch_z_id"].Value = id.SLUCH_Z_ID;
                            cmd.Parameters["sluch_id"].Value = id.SLUCH_ID;
                            cmd.Parameters["usl_id"].Value = id.USL_ID ?? (object)DBNull.Value;
                            var c = cmd.ExecuteNonQuery();
                        }



                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Ошибка в SetMedpomDataOut: {0}", ex.Message, ex));
            }
        }

    }
}
