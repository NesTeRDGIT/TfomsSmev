using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceProcess;
using System.Threading;
using SMEV.WCFContract;
using System.IO;

using SmevAdapterService.AdapterLayer.XmlClasses;
using SmevAdapterService.AdapterLayer.Integration;
using SmevAdapterService.VS.Zags;
using SmevAdapterService.VS;
using SmevAdapterService.VS.MedicalCare;
using Npgsql;
using System.ServiceModel.Description;

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
        public ServiceHost _serviceHost = null;

        public AdapterService()
        {
            InitializeComponent();
        }
        string config_dir = Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "CONFIG");
        string config = "";
        protected override void OnStart(string[] args)
        {
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
                wi.onChangeConfig += Wi_onChangeConfig;
            }
            catch (Exception ex)
            {
                WcfServer.AddLog(string.Format("Ошибка загрузки конфига: {0}", ex.Message), EventLogEntryType.Error);
            }
            StartProcess();
        }

        private void Wi_onChangeConfig()
        {
           
            Configuration.SaveToFile(config, Configuration);
            StopProcess();
            StartProcess();
        }

        WcfServer wi;
        public static ServiceHost WcfConection { set; get; }


        private bool StartServer()
        {
            try
            {
                const string uri = @"http://localhost:50505/TFOMS_SMEV.svc"; // Адрес, который будет прослушивать сервер
                const string mexUri = @"http://localhost/TFOMS_SMEV.svc/mex";

                var netTcpBinding = new WSDualHttpBinding();
                netTcpBinding.Security.Mode = WSDualHttpSecurityMode.None;
                //netTcpBinding.ReliableSession.Enabled = true;
                netTcpBinding.ReaderQuotas.MaxArrayLength = int.MaxValue;
                netTcpBinding.ReaderQuotas.MaxBytesPerRead = int.MaxValue;
                netTcpBinding.ReaderQuotas.MaxStringContentLength = int.MaxValue;
                netTcpBinding.MaxBufferPoolSize = long.MaxValue;
                netTcpBinding.MaxReceivedMessageSize = int.MaxValue;
                netTcpBinding.MaxBufferPoolSize = int.MaxValue;
                netTcpBinding.OpenTimeout = new TimeSpan(24, 0, 0);
                netTcpBinding.ReceiveTimeout = new TimeSpan(24, 0, 0);
                netTcpBinding.SendTimeout = new TimeSpan(24, 0, 0);
                wi = new WcfServer(new Configuration());
               
                WcfConection = new ServiceHost(wi, new Uri(uri)); // Запускаем прослушивание

        
                var ep = WcfConection.AddServiceEndpoint(typeof(IWcfInterface), netTcpBinding, "");
                ep.Address = new EndpointAddress(new Uri(uri));

                ep.EndpointBehaviors.Add(new MessageServerBehavior.MessageServerBehavior());
               

                WcfConection.OpenTimeout = new TimeSpan(24, 0, 0);
                WcfConection.CloseTimeout = new TimeSpan(24, 0, 0);
                netTcpBinding.ReceiveTimeout = new TimeSpan(24, 0, 0);


                ServiceMetadataBehavior smb = WcfConection.Description.Behaviors.Find<ServiceMetadataBehavior>();
                //  If not, add one
                if (smb == null) smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;             
                smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                WcfConection.Description.Behaviors.Add(smb);
                WcfConection.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexHttpBinding(), mexUri);

                // Add MEX endpoint
                
                WcfConection.Open();
                return true;
            }
            catch (Exception ex)
            {
                WcfServer.AddLog("Ошибка при запуске WCF: " + ex.Message, EventLogEntryType.Error);
                return false;
            }
        }

        

        private Dictionary<SMEV.WCFContract.VS, ProcessObr> CurrentWork
        {
            get

            {
                return wi.CurrentWork;
            }
            set
            {
                wi.CurrentWork = value;
            }
        }


        void StartProcess()
        {
            foreach (var t in Configuration.ListVS)
            {
                Thread th = null;
                CancellationTokenSource cancel = new CancellationTokenSource();
                switch (t.VS)
                {
                    case SMEV.WCFContract.VS.MP:    
                        th = new Thread(new ParameterizedThreadStart(Medpom));
                        th.IsBackground = true;                        
                        break;
                    case SMEV.WCFContract.VS.ZAGS:
                        th = new Thread(new ParameterizedThreadStart(ZAGS));   
                        th.IsBackground = true;                       
                        break;
                    default:
                        WcfServer.AddLog(string.Format("Ошибка запуска конфигурации {0} - нет обработчика", t.VS.ToString()), EventLogEntryType.Error);
                        continue;                       
                }

                ProcessObr po = new ProcessObr(th, cancel, t, Configuration.TimeOut*1000);
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
            StopProcess();
        }
        public void Medpom(object obj)
        {
            try
            {
                ProcessObr po = (ProcessObr)obj;
                int TimeOut = po.TimeOut;
                po.Text = "Начало работы";
                CancellationToken token = po.Cancel.Token;
                Config_VS Config_VS = po.Config;
               
                MessageLogger mlog = new MessageLogger(Configuration.ConnectionString, Config_VS.ItSystem);

                IRepository fi = null;
                string ConnectionString = Config_VS.ConnectionString;// @"DATA SOURCE=localhost:1521/orcl;PERSIST SECURITY INFO=True;USER ID=asu12;PASSWORD=asu12;CONNECTION TIMEOUT=60";
                po.Text = "Запуск интеграции";
                switch (Config_VS.Integration)
                {
                    case Integration.FileSystem:
                        FileIntegrationConfig fic = FileIntegrationConfig.Get(Config_VS.FilesConfig);
                        fi = new FileIntegration(fic); break;
                    case Integration.DataBase:
                        fi = new DataBaseIntegration(Config_VS.DataBaseConfig.ConnectionString); break;
                }
                string NameType = "InputData";
                while (!token.IsCancellationRequested)
                {
                    var mess = fi.GetMessage();
                    foreach (var mes in mess)
                    {
                        po.Text = "Обработка сообщения";
                        try
                        {
                            var adapterInMessage = SeDeserializer<AdapterMessage>.DeserializeFromXDocument(mes.Content);
                            var MessageId = adapterInMessage.smevMetadata.MessageId;
                            //Если запрос
                            if (adapterInMessage.Message is RequestMessageType)
                            {
                                var idlog = mlog.AddInputMessage(NameType, MessageId, MessageLoggerStatus.INPUT);

                                var rmt = adapterInMessage.Message as RequestMessageType;

                                string replyToClientId = rmt.RequestMetadata.clientId;
                                string clientId = mlog.GetGuidOut();
                                MessageIntegration ms_out = new MessageIntegration() { ID = idlog, Key =clientId};
                                mes.ID = idlog;
                                var inputdate = SeDeserializer<InputData>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);

                                ///Получить ответ
                                var outdate = inputdate.Answer(ConnectionString);
                                ///Отправка ответа  

                                ms_out.Content = AdapterMessageCreator.GenerateAddapterSendRequest(outdate, adapterInMessage.smevMetadata.Recipient, replyToClientId, clientId); ;
                                fi.SendMessage(ms_out);
                                fi.EndProcessMessage(mes);
                                mlog.UpdateStatusIN(idlog, MessageLoggerStatus.SUCCESS);
                                mlog.SetOutMesssage(idlog, clientId, MessageLoggerStatus.OUTPUT);
                                continue;
                            }
                            if (adapterInMessage.Message is ResponseMessageType)
                            {
                                var resp = adapterInMessage.Message as ResponseMessageType;
                                var id = (mlog.FindIDByMessageOut(resp.ResponseMetadata.replyToClientId));
                                if (!id.HasValue)
                                {
                                    throw new Exception(string.Format("Не удалось найти ID сообщения для [{0}]", mes.Key));
                                }
                                mes.ID = id.Value;
                                switch (adapterInMessage.Message.messageType)
                                {
                                    case "StatusMessage":
                                        mlog.UpdateStatusOut(id.Value, MessageLoggerStatus.SUCCESS);
                                        mlog.UpdateCommentOut(id.Value, resp.ResponseContent.status.description);
                                        fi.ReadMessage(mes);
                                        break;
                                    case "ErrorMessage":
                                        mlog.UpdateStatusOut(id.Value, MessageLoggerStatus.ERROR);
                                        mlog.UpdateCommentOut(id.Value, resp.ResponseContent.status.description);
                                        fi.ReadMessage(mes);
                                        break;
                                    default:

                                        throw new Exception(string.Format("Не верный messageType для ResponseMessageType для [{0}]", mes.Key));
                                }
                                continue;
                            }

                            throw new Exception(string.Format("Неизвестный тип сообщения[{0}]", mes.Key));
                        }
                        catch (Exception ex)
                        {
                            WcfServer.AddLog(string.Format("Ошибка потоке обработки МП: {0} ", ex.Message), EventLogEntryType.Error);
                            fi.ErrorMessage(mes);
                        }

                    }
                    po.Text = "Ожидание сообщения";
                    Thread.Sleep(TimeOut);
                }
            }
            catch(Exception ex)
            {
                WcfServer.AddLog(string.Format("Ошибка в потоке {0}: {1}", "Medpom", ex.Message), EventLogEntryType.Error);
            }
        }

        public void ZAGS(object obj)
        {
            try
            {
                ProcessObr po = (ProcessObr)obj;
                int TimeOut = po.TimeOut;
                po.Text = "Начало работы";
                CancellationToken token = po.Cancel.Token;
                Config_VS Config_VS = po.Config;
                MessageLogger mlog = new MessageLogger(Configuration.ConnectionString, Config_VS.ItSystem);

                IRepository fi = null;



                string ConnectionString = Config_VS.ConnectionString;// @"DATA SOURCE=localhost:1521/orcl;PERSIST SECURITY INFO=True;USER ID=asu12;PASSWORD=asu12;CONNECTION TIMEOUT=60";
                po.Text = "Запуск интеграции";
                switch (Config_VS.Integration)
                {
                    case Integration.FileSystem:
                        FileIntegrationConfig fic = FileIntegrationConfig.Get(Config_VS.FilesConfig);
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

                            var adapterInMessage = SeDeserializer<AdapterMessage>.DeserializeFromXDocument(mes.Content);
                          
                            //Если запрос
                            if (adapterInMessage.Message is RequestMessageType)
                            {
                                var rmt = adapterInMessage.Message as RequestMessageType;
                                var ns = rmt.RequestContent.content.MessagePrimaryContent.Name.Namespace;
                                IRequestMessage inputdate = null;
                                string NameType = "";
                                //Определяем тип сообщения
                                if (Request_BRAKZRZP.XmlnsClass.ToArray().Where(x => x.Namespace == ns.NamespaceName).Count() != 0)
                                {
                                    inputdate = SeDeserializer<Request_BRAKZRZP>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    NameType = "Request_BRAKZRZP";
                                }
                                if (Request_FATALZP.XmlnsClass.ToArray().Where(x => x.Namespace == ns.NamespaceName).Count() != 0)
                                {
                                    inputdate = SeDeserializer<Request_FATALZP>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    NameType = "Request_FATALZP";
                                }
                                if (Request_PARENTZP.XmlnsClass.ToArray().Where(x => x.Namespace == ns.NamespaceName).Count() != 0)
                                {
                                    inputdate = SeDeserializer<Request_PARENTZP>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    NameType = "Request_PARENTZP";
                                }
                                if (Request_PERNAMEZP.XmlnsClass.ToArray().Where(x => x.Namespace == ns.NamespaceName).Count() != 0)
                                {
                                    inputdate = SeDeserializer<Request_PERNAMEZP>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    NameType = "Request_PERNAMEZP";
                                }
                                if (Request_ROGDZP.XmlnsClass.ToArray().Where(x => x.Namespace == ns.NamespaceName).Count() != 0)
                                {
                                    inputdate = SeDeserializer<Request_ROGDZP>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    NameType = "Request_ROGDZP";
                                }


                                ///Если не известный тип
                                if (inputdate == null)
                                {
                                    throw new Exception(string.Format("Неизвестный тип запроса [{0}]", mes.Key));
                                }
                                var MessageId = adapterInMessage.smevMetadata.MessageId;
                                var idlog = mlog.AddInputMessage(NameType, MessageId, MessageLoggerStatus.INPUT);
                                mes.ID = idlog;
                                string replyToClientId = rmt.RequestMetadata.clientId;
                                string clientId = mlog.GetGuidOut();
                                MessageIntegration ms_out = new MessageIntegration() { ID = idlog, Key = clientId };

                                if (Config_VS.TranspotrMessage != "")
                                {
                                    string Dir = Path.Combine(Config_VS.TranspotrMessage, NameType, DateTime.Now.ToString("yyyyMMdd"));
                                    if (!Directory.Exists(Dir)) Directory.CreateDirectory(Dir);
                                    string path = Path.Combine(Dir, string.Format("{{{0}}}.xml", mes.Key));
                                    rmt.RequestContent.content.MessagePrimaryContent.Save(path);
                                    ///inputdate.Serialize().Save(path);
                                    mlog.UpdateStatusIN(idlog, MessageLoggerStatus.INPUT);
                                    mlog.UpdateComentIN(idlog, "SAVE: " + path);
                                }
                                ///Получить ответ
                                var outdate = inputdate.Answer(Config_VS.ConnectionString);

                                ms_out.Content = AdapterMessageCreator.GenerateAddapterSendRequest(outdate, adapterInMessage.smevMetadata.Recipient, replyToClientId, MessageId);
                                ///Отправка ответа  
                                fi.SendMessage(ms_out);
                                fi.EndProcessMessage(mes);
                                mlog.UpdateStatusIN(idlog, MessageLoggerStatus.SUCCESS);
                                mlog.SetOutMesssage(idlog, clientId, MessageLoggerStatus.OUTPUT);
                                continue;
                            }

                            if (adapterInMessage.Message is ResponseMessageType)
                            {

                                var resp = adapterInMessage.Message as ResponseMessageType;
                                var id = mlog.FindIDByMessageOut(resp.ResponseMetadata.replyToClientId);
                                if (!id.HasValue)
                                {
                                    throw new Exception(string.Format("Не удалось найти ID сообщения для [{0}]", mes.Key));
                                }

                                switch (adapterInMessage.Message.messageType)
                                {
                                    case "StatusMessage":
                                        mlog.UpdateStatusOut(id.Value, MessageLoggerStatus.SUCCESS);
                                        mlog.UpdateCommentOut(id.Value, resp.ResponseContent.status.description);
                                        fi.ReadMessage(mes);
                                        break;
                                    case "ErrorMessage":
                                        mlog.UpdateStatusOut(id.Value, MessageLoggerStatus.ERROR);
                                        mlog.UpdateCommentOut(id.Value, resp.ResponseContent.status.description);
                                        fi.ReadMessage(mes);
                                        break;
                                    default:
                                        throw new Exception(string.Format("Не верный messageType для ResponseMessageType для [{0}]", mes.Key));
                                }
                                continue;
                            }
                            throw new Exception(string.Format("Неизвестный тип сообщения [{0}]", mes.Key));
                        }
                        catch (Exception ex)
                        {
                            WcfServer.AddLog(string.Format("Ошибка потоке обработки ЗАГС: {0} ", ex.Message), EventLogEntryType.Error);
                            fi.ErrorMessage(mes);
                        }

                    }
                    po.Text = "Ожидание сообщения";
                    Thread.Sleep(TimeOut);
                }
            }
            catch (Exception ex)
            {
                WcfServer.AddLog(string.Format("Ошибка в потоке {0}: {1}", "ЗАГС", ex.Message), EventLogEntryType.Error);
            }
        }
    }


    public class Logger
    {
        public Logger(string ConnectionString)
        {
            
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
        public int AddInputMessage(string VS, string id_message_in, MessageLoggerStatus status_in, string comment_in = "")
        {
            try
            {
                using (var con = new NpgsqlConnection(ConnectionString))
                {
                    using (NpgsqlCommand cmd = new NpgsqlCommand(@"insert INTO  log_service
                                                    (id_message_in, itsystem, vs, status_in, comment_in, date_in)
                                                    values
                                                    (@id_message_in, @itsystem, @vs, @status_in, @comment_in, @date_in) RETURNING id", con))
                    {
                        

                        cmd.Parameters.Add(new NpgsqlParameter("id_message_in", id_message_in));
                        cmd.Parameters.Add(new NpgsqlParameter("itsystem", ItSystem));
                        cmd.Parameters.Add(new NpgsqlParameter("vs", VS));
                        cmd.Parameters.Add(new NpgsqlParameter("status_in", status_in.ToString()));
                        cmd.Parameters.Add(new NpgsqlParameter("comment_in", comment_in));
                        cmd.Parameters.Add(new NpgsqlParameter("date_in", DateTime.Now));
                        con.Open();
                        var res = cmd.ExecuteScalar();
                        con.Close();
                        int id = Convert.ToInt32(res);
                        return id;
                    }
                }
            }
            catch (Exception ex)
            {
                WcfServer.AddLog(string.Format("Ошибка вставки истории AddInputMessage[ItSystem[{0}],VS[{1}],ID_MESSAGE[{2}]] : {3}", ItSystem, VS, id_message_in, ex.Message), EventLogEntryType.Error);
                return -5;
            }
        }
        /// <summary>
        /// Ответ на сообщение
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="id_message_out"></param>
        public void SetOutMesssage(int ID, string id_message_out, MessageLoggerStatus status_out)
        {

            try
            {
                var con = new NpgsqlConnection(ConnectionString);
                var cmd = new NpgsqlCommand(@"update log_service t set id_message_out =  @id_message_out, status_out = @status_out,date_out= @date_out
                                                   where t.ID = @ID", con);
                cmd.Parameters.Add(new NpgsqlParameter("id_message_out", id_message_out.ToString()));
                cmd.Parameters.Add(new NpgsqlParameter("status_out", status_out.ToString()));
                cmd.Parameters.Add(new NpgsqlParameter("date_out", DateTime.Now));
                
                cmd.Parameters.Add(new NpgsqlParameter("ID", ID));
                con.Open();
                var x = cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                WcfServer.AddLog(string.Format("Ошибка обновления истории SetOutMesssage[ID[{0}],id_message_out[{1}]] : {2}", ID, id_message_out, ex.Message), EventLogEntryType.Error);
            }
        }
        public void UpdateStatusIN(int ID, MessageLoggerStatus status_in)
        {          
            try
            {
                var con = new NpgsqlConnection(ConnectionString);

                var cmd = new NpgsqlCommand(@"update log_service t set status_in =  @status_in
                                                   where t.ID = @ID", con);

                cmd.Parameters.Add(new NpgsqlParameter("status_in", status_in.ToString()));
    
                cmd.Parameters.Add(new NpgsqlParameter("ID", ID));
                con.Open();
                var x = cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                WcfServer.AddLog(string.Format("Ошибка обновления истории UpdateStatusMessage[ID[{0}]] : {1}", ID, ex.Message), EventLogEntryType.Error);
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
                WcfServer.AddLog(string.Format("Ошибка обновления истории UpdateStatusMessage[ID[{0}]] : {1}", ID, ex.Message), EventLogEntryType.Error);
            }
        }

        public void UpdateStatusOut(int ID, MessageLoggerStatus status_out)
        {
            NpgsqlTransaction tran = null;
            try
            {
                var con = new NpgsqlConnection(ConnectionString);

                var cmd = new NpgsqlCommand(@"update log_service t set status_out =  @status_out 
                                                   where t.ID = @ID", con);


                cmd.Parameters.Add(new NpgsqlParameter("status_out", status_out.ToString()));

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
                if (tran != null)
                    tran.Rollback();
                WcfServer.AddLog(string.Format("Ошибка обновления истории UpdateStatusOutMessage[ID[{0}]] : {1}",  ID, ex.Message), EventLogEntryType.Error);
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
                if (tran != null)
                    tran.Rollback();
                WcfServer.AddLog(string.Format("Ошибка обновления истории UpdateStatusOutMessage[ID[{0}]] : {1}", ID, ex.Message), EventLogEntryType.Error);
            }
        }

        public string GetGuidOut()
        {
            using (var con = new NpgsqlConnection(ConnectionString))
            {
                using (var cmd = new NpgsqlCommand(@"select count(*) from log_service t 
                                          where t.ID_MESSAGE_OUT = @ID_MESSAGE_OUT and t.ItSystem = @ItSystem", con))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("ID_MESSAGE_OUT", ""));
                    cmd.Parameters.Add(new NpgsqlParameter("ItSystem", ItSystem));
                    string t = "";
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

        public int? FindIDByMessageOut(string id_message_out)
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
    }
}
