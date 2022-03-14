using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using SMEV;
using SMEV.VS.MedicalCare.V1_0_0;
using SMEV.VS.Zags;
using SMEV.VS.Zags.V4_0_0;
using SMEV.WCFContract;
using SmevAdapterService.AdapterLayer.Integration;
using SmevAdapterService.AdapterLayer.XmlClasses;
using SmevAdapterService.CabinetService;
using SmevAdapterService.VS;

using InputData = SMEV.VS.MedicalCare.V1_0_0.InputData;
using OutputData = SMEV.VS.MedicalCare.V1_0_0.OutputData;

namespace SmevAdapterService
{
    public interface IProcesor
    {
        void StartProcess(Configuration conf);
        void StopProcess();
        Dictionary<SMEV.WCFContract.VS, IProcess> CurrentWork { get; }
        
    }


    
 
    public class ProcessWork : IProcesor
    {
        private Configuration Configuration { get; set; }

        private ILogger logger;
        public ProcessWork(ILogger logger)
        {
            this.logger = logger;
        }

        private void AddLog(string log, LogType type)
        {
            logger?.AddLog(log, type);
        }
        public Dictionary<SMEV.WCFContract.VS, IProcess> CurrentWork { get; } = new Dictionary<SMEV.WCFContract.VS, IProcess>();

        public void StartProcess(Configuration conf)
        {
            Configuration = conf;
            foreach (var t in Configuration.ListVS)
            {
                var param = new ProcessObrTaskParam(t, Configuration.TimeOut * 1000);
                IRepository repository = new FileIntegration(FileIntegrationConfig.Get(param.Config.FilesConfig));
                IMessageLogger messageLogger = new MessageLogger(Configuration.ConnectionString, t.ItSystem, logger);
                IProcess process;
                switch (t.VS)
                {
                    case SMEV.WCFContract.VS.MP:
                        var mPAnswer = new MPAnswer(t.ConnectionString);
                        process = new MPProcess(logger, repository, messageLogger, param, mPAnswer);
                        break;
                    case SMEV.WCFContract.VS.ZAGS:
                        process = new ZAGSProcess(logger, repository, messageLogger, param);
                        break;
                    case SMEV.WCFContract.VS.PFR:
                        process = new PFRProcess(logger, repository, messageLogger, param);
                        break;
                    case SMEV.WCFContract.VS.Cabinet:
                        mPAnswer = new MPAnswer(t.ConnectionString);
                        var informing = new Informing(t.ConnectionString2);
                        var register = new Register(t.ConnectionString2);
                        process = new CabinetProcess(param, logger, messageLogger, mPAnswer, informing, register);
                        break;
                    default:
                        AddLog($"Ошибка запуска конфигурации {t.VS} - нет обработчика", LogType.Error);
                        continue;
                }
                CurrentWork.Add(t.VS, process);
                if (t.isEnabled)
                    process.StartProcess();
            }
        }

        public void StopProcess()
        {
            foreach (var t in CurrentWork.Where(t => t.Value.IsRunning))
            {
                t.Value.StopProcess();
            }
            CurrentWork.Clear();
        }


    }



    public interface IProcess
    {
        void StartProcess();
        void StopProcess();
        void Resent(int ID);

        bool IsRunning { get; }
        string ItSystem { get; }
        SMEV.WCFContract.VS VS { get; }
        string Text { get; }
    }

  

    public class MPProcess:IProcess
    {
        private ILogger logger;
        private IRepository repository;
        private IMessageLogger messageLogger;
        private ProcessObrTaskParam param;
        private IMPAnswer mPAnswer;

        public MPProcess(ILogger logger, IRepository repository, IMessageLogger messageLogger, ProcessObrTaskParam param, IMPAnswer mPAnswer)
        {
            this.logger = logger;
            this.repository = repository;
            this.messageLogger = messageLogger;
            this.param = param;
            this.mPAnswer = mPAnswer;
        }
        private void Delay(int MS, CancellationToken cancel)
        {
            try
            {
                var t = Task.Delay(MS, cancel);
                t.Wait(cancel);
            }
            catch (OperationCanceledException) { }
        }
        private void AddLog(string log, LogType type)
        {
            logger?.AddLog(log, type);
        }

        private void Medpom(CancellationToken cancel)
        {
            try
            {
                param.Text = "Начало работы";
                var Config_VS = param.Config;            
                var mlog = messageLogger;
                param.Text = "Запуск интеграции";

                while (!cancel.IsCancellationRequested)
                {
                    var mess = repository.GetMessage();
                    foreach (var mes in mess)
                    {
                        param.Text = "Обработка сообщения";
                        try
                        {
                            var adapterInMessage = SeDeserializer<QueryResult>.DeserializeFromXDocument(mes.Content);
                            var MessageId = adapterInMessage.smevMetadata.MessageId;
                            //Если запрос
                            var rmt = adapterInMessage.Message as RequestMessageType;
                            if (rmt != null)
                            {
                                var ns = rmt.RequestContent.content.MessagePrimaryContent.Name.Namespace;
                                var replyToClientId = rmt.RequestMetadata.clientId;
                                //Определяем тип сообщения
                                if (InputData.XmlnsClass.ToArray().Count(x => x.Namespace == ns.NamespaceName) != 0)
                                {
                                    var val = SeDeserializer<InputData>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    ProcessInputData(mes, val, MessageId, replyToClientId, adapterInMessage.smevMetadata.Recipient);
                                    continue;
                                }

                                if (SMEV.VS.MedicalCare.newV1_0_0.ListOfMedicalServicesProvided.InputData.XmlnsClass.ToArray().Count(x => x.Namespace == ns.NamespaceName) != 0)
                                {
                                    var val = SeDeserializer<SMEV.VS.MedicalCare.newV1_0_0.ListOfMedicalServicesProvided.InputData>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    ProcessInputData_V2(mes,  val, MessageId, replyToClientId, adapterInMessage.smevMetadata.Recipient);
                                    continue;
                                }
                                if (SMEV.VS.MedicalCare.newV1_0_0.FeedbackOnMedicalService.InputData.XmlnsClass.ToArray().Count(x => x.Namespace == ns.NamespaceName) != 0)
                                {
                                    var val = SeDeserializer<SMEV.VS.MedicalCare.newV1_0_0.FeedbackOnMedicalService.InputData>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    ProcessFeedbackOnMedicalService(mes,  val, MessageId, replyToClientId, adapterInMessage.smevMetadata.Recipient);
                                    continue;
                                }
                                throw new Exception($"Неизвестный тип запроса [{mes.Key}]");
                            }

                            if (adapterInMessage.Message is ResponseMessageType resp)
                            {
                                var id = mlog.FindIDByMessageOut(resp.ResponseMetadata.replyToClientId);
                                if (!id.HasValue)
                                {
                                    throw new Exception($"Не удалось найти ID сообщения для [{mes.Key}]");
                                }
                                mes.ID = id.Value;
                                MessageLoggerStatus status;
                                var message = resp.ResponseContent?.status?.description;
                                switch (adapterInMessage.Message.messageType)
                                {
                                    case "StatusMessage":
                                        status = MessageLoggerStatus.SUCCESS;
                                        break;
                                    case "ErrorMessage":
                                        status = MessageLoggerStatus.ERROR;
                                        break;
                                    case "RejectMessage":
                                        status = MessageLoggerStatus.ERROR;
                                        message = string.Join(",", resp.ResponseContent.rejects.Select(x => $"{x.code}:{x.description}"));
                                        break;
                                    default:
                                        throw new Exception($"Не верный messageType для ResponseMessageType для [{mes.Key}]");
                                }
                                mlog.InsertStatusOut(id.Value, status, message);
                                repository.ReadMessage(mes);
                                continue;
                            }

                            if (adapterInMessage.Message is ErrorMessage err)
                            {
                                AddLog($"Сообщение об ошибке из СМЭВ в потоке МП: {err.details}", LogType.Error);
                                var id = mlog.FindIDByMessageOut(err.statusMetadata.originalClientId);
                                if (!id.HasValue)
                                {
                                    throw new Exception($"Не удалось найти ID сообщения для [{mes.Key}]");
                                }
                                mlog.InsertStatusOut(id.Value, MessageLoggerStatus.ERROR, err.details);
                                repository.ReadMessage(mes);
                                continue;
                            }
                            throw new Exception($"Неизвестный тип сообщения[{mes.Key}]");
                        }
                        catch (Exception ex)
                        {
                            AddLog($"Ошибка потоке обработки МП({mes.Key}): {ex.Message}{ex.StackTrace}", LogType.Error);
                            repository.ErrorMessage(mes);
                        }
                    }
                    param.Text = "Ожидание сообщения";
                    Delay(param.TimeOut,cancel);
                }
            }
            catch (Exception ex)
            {
                AddLog($"Ошибка в потоке Medpom: {ex.Message}{ex.StackTrace}", LogType.Error);
            }
        }
        private void ProcessInputData(MessageIntegration mes,  InputData inputdate, string MessageId, string replyToClientId, string ITSystem)
        {
            try
            {
                //Данные о человеке
                var idlog = messageLogger.AddInputMessage(MessageLoggerVS.InputData, MessageId, MessageLoggerStatus.INPUT, "", "");
                var req = inputdate.InsuredRenderingListRequest;
                messageLogger.SetMedpomDataIn(idlog, req.FamilyName, req.FirstName, req.Patronymic, req.BirthDate, req.DateFrom, req.DateTo, req.UnitedPolicyNumber, null);

                //Ответ из БД
                var result = GetAnswer(inputdate);
                var outdate = ConvetToOutput(result);

                if (result.Count != 0)
                {
                    messageLogger.SetMedpomDataOut(idlog, result.Select(x => new SLUCH_REF(x.isMTR, x.SLUCH_Z_ID, x.SLUCH_ID, x.USL_ID)).ToList());
                }
                //Исходящие сообщение
                var clientId = messageLogger.GetNewGuidOut();
                var ms_out = new MessageIntegration { ID = idlog, Key = clientId };
                mes.ID = idlog;
                ms_out.Content = AdapterMessageCreator.GenerateAddapterSendRequest(outdate, ITSystem, replyToClientId, clientId);
                repository.SendMessage(ms_out);
                repository.EndProcessMessage(mes);
                messageLogger.UpdateStatusIN(idlog, MessageLoggerStatus.SUCCESS);
                messageLogger.SetOutMessage(idlog, clientId, MessageLoggerStatus.OUTPUT);
            }
            catch (Exception e)
            {
                throw new Exception($"Ошибка в ProcessInputData: {e.Message}", e);
            }
        }
        public void Resent(int ID)
        {
            var guids = messageLogger.GetGuids(ID);
            if(guids==null)
                throw new Exception("Сервер не вернул сообщение");
            if (string.IsNullOrEmpty(guids.GUID_IN) || string.IsNullOrEmpty(guids.GUID_OUT))
                throw new Exception("Отстутсвуеют реквизиты сообщений");
            var SLUCH_REF = messageLogger.GetMedpomDataOut(ID);          
            var result = mPAnswer.GetData(SLUCH_REF.Where(x=>!x.IsMTR).Select(x=>x.SLUCH_ID).ToArray(), SLUCH_REF.Where(x => x.IsMTR).Select(x => x.SLUCH_ID).ToArray());
            if(result.Count!= SLUCH_REF.Count)
                throw new Exception("Первичные данные отличаются от повторных");

            var outdate = ConvetToOutput(result);
            var ms_out = new MessageIntegration { ID = ID, Key = guids.GUID_OUT };
            ms_out.Content = AdapterMessageCreator.GenerateAddapterSendRequest(outdate, ItSystem, guids.GUID_IN, guids.GUID_OUT);
            repository.SendMessage(ms_out);
        }


        private OutputData ConvetToOutput(List<V_MEDPOM_SMEV3Row> data)
        {
            var result = new OutputData();
            foreach(var item in data)
            {
                var medicalCare = new InsuredRenderingInfo
                {
                    DateRenderingFrom = item.DATE_IN,
                    DateRenderingTo = item.DATE_OUT,
                    CareRegimen = item.USL_OK_NAME,
                    CareType = item.VIDPOM_NAME,
                    Name = item.NAME_USL,
                    MedServicesSum = item.SUMP_USL,
                    ClinicName = item.NAM_MOK,
                    RegionName = item.TF_NAME
                };
                result.InsuredRenderingList.Add(medicalCare);
            }
            return result.InsuredRenderingList.Count == 0 ? null : result;
        }
        private List<V_MEDPOM_SMEV3Row> GetAnswer(InputData inputdate)
        {
            var req = inputdate.InsuredRenderingListRequest;
            return mPAnswer.GetData(req.FamilyName, req.FirstName, req.Patronymic, req.BirthDate, req.UnitedPolicyNumber, req.DateFrom, req.DateTo);
        }

        private void ProcessInputData_V2(MessageIntegration mes, SMEV.VS.MedicalCare.newV1_0_0.ListOfMedicalServicesProvided.InputData inputdate, string MessageId, string replyToClientId, string ITSystem)
        {
            try
            {
                //Данные о человеке
                var idlog = messageLogger.AddInputMessage(MessageLoggerVS.InputData, MessageId, MessageLoggerStatus.INPUT, inputdate.orderId, "");
                var req = inputdate;
                messageLogger.SetMedpomDataIn(idlog, req.FamilyName, req.FirstName, req.Patronymic, req.BirthDate, req.DateFrom, req.DateTo, req.UnitedPolicyNumber, req.orderId);
                var clientId = messageLogger.GetNewGuidOut();
                //Ответ из БД
                var result = GetAnswer(inputdate);
                var outdate = ConvetToOutputNew(result);
                if (outdate != null)
                {
                    outdate.orderId = req.orderId;
                    messageLogger.SetMedpomDataOut(idlog, result.Select(x => new SLUCH_REF(x.isMTR, x.SLUCH_Z_ID, x.SLUCH_ID, x.USL_ID)).ToList());
                }
                //Исходящие сообщение
                var ms_out = new MessageIntegration { ID = idlog, Key = clientId };
                mes.ID = idlog;
                ms_out.Content = AdapterMessageCreator.GenerateAddapterSendRequest(outdate, ITSystem, replyToClientId, clientId);
                repository.SendMessage(ms_out);
                repository.EndProcessMessage(mes);
                messageLogger.UpdateStatusIN(idlog, MessageLoggerStatus.SUCCESS);
                messageLogger.SetOutMessage(idlog, clientId, MessageLoggerStatus.OUTPUT);
            }
            catch (Exception e)
            {
                throw new Exception($"Ошибка в ProcessInputData_V2: {e.Message}", e);
            }
        }


        private SMEV.VS.MedicalCare.newV1_0_0.ListOfMedicalServicesProvided.OutputData ConvetToOutputNew(List<V_MEDPOM_SMEV3Row> data)
        {
            var result = new SMEV.VS.MedicalCare.newV1_0_0.ListOfMedicalServicesProvided.OutputData();
            foreach (var item in data)
            {
                var medicalCare = new SMEV.VS.MedicalCare.newV1_0_0.ListOfMedicalServicesProvided.InsuredRenderingInfo
                {
                    DateRenderingFrom = item.DATE_IN,
                    DateRenderingTo = item.DATE_OUT,
                    CareRegimen = item.USL_OK_NAME,
                    CareType = item.VIDPOM_NAME,
                    Name = item.NAME_USL,
                    MedServicesSum = item.SUMP_USL,
                    ClinicName = item.NAM_MOK,
                    RegionName = item.TF_NAME
                };
                result.InsuredRenderingList.Add(medicalCare);
            }
            return result.InsuredRenderingList.Count == 0 ? null : result;
        }
        private List<V_MEDPOM_SMEV3Row> GetAnswer(SMEV.VS.MedicalCare.newV1_0_0.ListOfMedicalServicesProvided.InputData inputdate)
        {
            var req = inputdate;
            return mPAnswer.GetData(req.FamilyName, req.FirstName, req.Patronymic, req.BirthDate, req.UnitedPolicyNumber, req.DateFrom, req.DateTo);
        }


        public void ProcessFeedbackOnMedicalService(MessageIntegration mes, SMEV.VS.MedicalCare.newV1_0_0.FeedbackOnMedicalService.InputData inputdate, string MessageId, string replyToClientId, string ITSystem)
        {
            //Данные о человеке
            var idlog = messageLogger.AddInputMessage(MessageLoggerVS.FeedbackOnMedicalService, MessageId, MessageLoggerStatus.INPUT, inputdate.orderId, inputdate.ApplicationID);
            messageLogger.SetFeedbackINFO(inputdate, idlog);
            repository.EndProcessMessage(mes);
            messageLogger.UpdateStatusIN(idlog, MessageLoggerStatus.SUCCESS);
        }

     
        private Task task;
        private CancellationTokenSource CTS;
        public void StartProcess()
        {
            CTS = new CancellationTokenSource();
            task = new Task(() => Medpom(CTS.Token));
            task.Start();
        }

        public void StopProcess()
        {
            CTS?.Cancel();
        }

        public bool IsRunning => task?.Status == TaskStatus.Running;

        public string ItSystem => param.Config.ItSystem;

        public SMEV.WCFContract.VS VS => param.Config.VS;

        public string Text => param.Text;
    }

    public class ZAGSProcess : IProcess
    {
        private ILogger logger;
        private IRepository repository;
        private IMessageLogger messageLogger;
        private ProcessObrTaskParam param;
        public ZAGSProcess(ILogger logger, IRepository repository, IMessageLogger messageLogger, ProcessObrTaskParam param)
        {
            this.logger = logger;
            this.repository = repository;
            this.messageLogger = messageLogger;
            this.param = param;
        }
        private void Delay(int MS, CancellationToken cancel)
        {
            try
            {
                var t = Task.Delay(MS, cancel);
                t.Wait(cancel);
            }
            catch (OperationCanceledException) { }
        }
        private void AddLog(string log, LogType type)
        {
            logger?.AddLog(log, type);
        }
        public void ZAGS(CancellationToken cancel)
        {
            try
            {
                param.Text = "Начало работы";
                var Config_VS = param.Config;
                var mlog = messageLogger;

                param.Text = "Запуск интеграции";
                var fi = repository;
                while (!cancel.IsCancellationRequested)
                {
                    var mess = fi.GetMessage();
                    foreach (var mes in mess)
                    {
                        param.Text = "Обработка сообщения";
                        try
                        {
                            var adapterInMessage = SeDeserializer<QueryResult>.DeserializeFromXDocument(mes.Content);
                            //Если запрос
                            if (adapterInMessage.Message is RequestMessageType rmt)
                            {
                                var ns = rmt.RequestContent.content.MessagePrimaryContent.Name.Namespace;
                                IRequestMessage inputdate = null;
                                var NameType = MessageLoggerVS.UNKNOW;
                                //Определяем тип сообщения
                                if (GetNameSpace<Request_BRAKZRZP>() == ns.NamespaceName)
                                {
                                    inputdate = SeDeserializer<Request_BRAKZRZP>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    NameType = MessageLoggerVS.Request_BRAKZRZP;
                                }
                                if (GetNameSpace<Request_BRAKRZP>() == ns.NamespaceName)
                                {
                                    inputdate = SeDeserializer<Request_BRAKRZP>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    NameType = MessageLoggerVS.Request_BRAKRZP;
                                }
                                if (GetNameSpace<Request_BRAKZZP>() == ns.NamespaceName)
                                {
                                    inputdate = SeDeserializer<Request_BRAKZZP>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    NameType = MessageLoggerVS.Request_BRAKZZP;
                                }
                                if (GetNameSpace<Request_FATALZP>() == ns.NamespaceName)
                                {
                                    inputdate = SeDeserializer<Request_FATALZP>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    NameType = MessageLoggerVS.Request_FATALZP;
                                }
                                if (GetNameSpace<SMEV.VS.Zags.V4_0_1.Request_FATALZP>() == ns.NamespaceName)
                                {
                                    inputdate = SeDeserializer<SMEV.VS.Zags.V4_0_1.Request_FATALZP>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    NameType = MessageLoggerVS.Request_FATALZP4_0_1;
                                }

                                if (GetNameSpace<Request_PARENTZP>() == ns.NamespaceName)
                                {
                                    inputdate = SeDeserializer<Request_PARENTZP>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    NameType = MessageLoggerVS.Request_PARENTZP;
                                }
                                if (GetNameSpace<SMEV.VS.Zags.V4_0_1.Request_PARENTZP>() == ns.NamespaceName)
                                {
                                    inputdate = SeDeserializer<SMEV.VS.Zags.V4_0_1.Request_PARENTZP>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    NameType = MessageLoggerVS.Request_PARENTZP4_0_1;
                                }
                                if (GetNameSpace<Request_PERNAMEZP>() == ns.NamespaceName)
                                {
                                    inputdate = SeDeserializer<Request_PERNAMEZP>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    NameType = MessageLoggerVS.Request_PERNAMEZP;
                                }
                                if (GetNameSpace<SMEV.VS.Zags.V4_0_1.Request_PERNAMEZP>() == ns.NamespaceName)
                                {
                                    inputdate = SeDeserializer<SMEV.VS.Zags.V4_0_1.Request_PERNAMEZP>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    NameType = MessageLoggerVS.Request_PERNAMEZP4_0_1;
                                }
                                if (GetNameSpace<Request_ROGDZP>() == ns.NamespaceName)
                                {
                                    inputdate = SeDeserializer<Request_ROGDZP>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    NameType = MessageLoggerVS.Request_ROGDZP;
                                }

                                if (GetNameSpace<SMEV.VS.Zags.V4_0_1.Request_ROGDZP>() == ns.NamespaceName)
                                {
                                    inputdate = SeDeserializer<SMEV.VS.Zags.V4_0_1.Request_ROGDZP>.DeserializeFromXDocument(rmt.RequestContent.content.MessagePrimaryContent);
                                    NameType = MessageLoggerVS.Request_ROGDZP4_0_1;
                                }

                                //Если не известный тип
                                if (inputdate == null)
                                {
                                    throw new Exception($"Неизвестный тип запроса [{mes.Key}]");
                                }
                                var MessageId = adapterInMessage.smevMetadata.MessageId;
                                var idlog = mlog.AddInputMessage(NameType, MessageId, MessageLoggerStatus.INPUT, "", "");
                                mes.ID = idlog;
                                var replyToClientId = rmt.RequestMetadata.clientId;
                                var clientId = mlog.GetNewGuidOut();
                                var ms_out = new MessageIntegration { ID = idlog, Key = clientId };

                                if (!string.IsNullOrEmpty(Config_VS.TranspotrMessage))
                                {
                                    var Dir = Path.Combine(Config_VS.TranspotrMessage, NameType.ToString(), DateTime.Now.ToString("yyyyMMdd"));
                                    if (!Directory.Exists(Dir)) Directory.CreateDirectory(Dir);
                                    var path = Path.Combine(Dir, $"[{mes.Key}][REQ][RAW][{DateTime.Now.Hour}-{DateTime.Now.Minute}].xml");
                                    rmt.RequestContent.content.MessagePrimaryContent.Save(path);
                                    mlog.UpdateStatusIN(idlog, MessageLoggerStatus.INPUT);
                                    mlog.UpdateCommentIN(idlog, $"SAVE: {path}");
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

                            if (adapterInMessage.Message is ResponseMessageType resp)
                            {
                                if (string.IsNullOrEmpty(resp.ResponseMetadata.replyToClientId) && resp.ResponseContent.status.description?.ToUpper().Trim() == "Successfully queued".ToUpper().Trim())
                                {
                                    AddLog($"Косяк ЗАГС в СМЭВ нет replyToClientId", LogType.Warning);
                                    fi.ErrorMessage(mes, "_NOTreply");
                                    continue;
                                }
                                var id = mlog.FindIDByMessageOut(resp.ResponseMetadata.replyToClientId);
                                if (!id.HasValue)
                                {
                                    throw new Exception($"Не удалось найти ID сообщения для [{mes.Key}]");
                                }
                                mes.ID = id.Value;
                                MessageLoggerStatus status;
                                var message = resp.ResponseContent.status.description;
                                switch (resp.messageType)
                                {
                                    case "StatusMessage":
                                        status = MessageLoggerStatus.SUCCESS;
                                        break;
                                    case "ErrorMessage":
                                        status = MessageLoggerStatus.ERROR;
                                        break;
                                    case "RejectMessage":
                                        message = string.Join(",", resp.ResponseContent.rejects.Select(x => $"{x.code.ToString()}:{x.description}"));
                                        status = MessageLoggerStatus.SUCCESS;
                                        break;
                                    default:
                                        throw new Exception($"Не верный messageType для ResponseMessageType для [{mes.Key}]");
                                }
                                mlog.InsertStatusOut(id.Value, status, message);
                                fi.ReadMessage(mes);
                                continue;
                            }

                            if (adapterInMessage.Message is ErrorMessage err)
                            {
                                AddLog($"Сообщение об ошибке из СМЭВ в потоке ЗАГС: {err.details}", LogType.Error);
                                var id = mlog.FindIDByMessageOut(err.statusMetadata.originalClientId);
                                if (!id.HasValue)
                                {
                                    throw new Exception($"Не удалось найти ID сообщения для [{mes.Key}]");
                                }
                                mlog.InsertStatusOut(id.Value, MessageLoggerStatus.ERROR, err.details);
                                fi.ReadMessage(mes);
                                continue;
                            }
                            throw new Exception($"Неизвестный тип сообщения [{mes.Key}]");
                        }
                        catch (Exception ex)
                        {
                            AddLog($"Ошибка потоке обработки ЗАГС: {ex.Message} ", LogType.Error);
                            fi.ErrorMessage(mes);
                        }

                    }
                    param.Text = "Ожидание сообщения";
                    Delay(param.TimeOut, cancel);
                }
            }
            catch (Exception ex)
            {
                AddLog($"Ошибка в потоке ЗАГС: {ex.Message}", LogType.Error);
            }
        }

        private string GetNameSpace<T>()
        {
            return SeDeserializer<T>.Namespace;
        }

        private Task task;
        private CancellationTokenSource CTS;
        public void StartProcess()
        {
            CTS = new CancellationTokenSource();
            task = new Task(() => ZAGS( CTS.Token));
            task.Start();
        }

        public void StopProcess()
        {
            CTS?.Cancel();
        }

        public void Resent(int ID)
        {
            throw new NotImplementedException();
        }

        public bool IsRunning => task?.Status == TaskStatus.Running;
        public string ItSystem => param.Config.ItSystem;

        public SMEV.WCFContract.VS VS => param.Config.VS;

        public string Text => param.Text;

    }

    public class PFRProcess : IProcess
    {
        private ILogger logger;
        private IRepository repository;
        private IMessageLogger messageLogger;
        private ProcessObrTaskParam param;

        public PFRProcess(ILogger logger, IRepository repository, IMessageLogger messageLogger,  ProcessObrTaskParam param)
        {
            this.logger = logger;
            this.repository = repository;
            this.messageLogger = messageLogger;
            this.param = param;
        }
        private void Delay(int MS, CancellationToken cancel)
        {
            try
            {
                var t = Task.Delay(MS, cancel);
                t.Wait(cancel);
            }
            catch (OperationCanceledException) { }
        }
        private void AddLog(string log, LogType type)
        {
            logger?.AddLog(log, type);
        }
        private async Task PFRAsync(CancellationToken cancel)
        {
            try
            {
                param.Text = "Начало работы";
                var configVs = param.Config;
                var mlog = messageLogger;
                param.Text = "Запуск интеграции";
                var fi = repository;

                while (!cancel.IsCancellationRequested)
                {
                    var files = await GetMessagePFPOut(configVs.UserOutMessage);
                    foreach (var file in files)
                    {
                        var clientId = mlog.GetNewGuidOut();
                        var send = CreatePFRData(file, configVs.ItSystem, clientId);
                        var id = mlog.AddInputMessage(MessageLoggerVS.PFR_SNILS, "", MessageLoggerStatus.NONE, "", "");
                        mlog.InsertStatusOut(id, MessageLoggerStatus.OUTPUT, $"FILE: {file}");
                        mlog.SetOutMessage(id, clientId, MessageLoggerStatus.OUTPUT);
                        var ms = new MessageIntegration { Key = clientId, ID = id, Content = send.SerializeToX() };
                        fi.SendMessage(ms);
                        var dirArc = Path.Combine(configVs.FilesConfig.ArchiveFolder, DateTime.Now.ToString("yyyy_MM_dd"), "UserOut");
                        if (!Directory.Exists(dirArc))
                            Directory.CreateDirectory(dirArc);

                        File.Move(file, Path.Combine(dirArc, $"{id}_{Path.GetFileName(file)}"));
                    }

                    var mess = fi.GetMessage();
                    foreach (var mes in mess)
                    {
                        param.Text = "Обработка сообщения";
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
                                var Dir = Path.Combine(configVs.TranspotrMessage, DateTime.Now.ToString("yyyyMMdd"));
                                if (!Directory.Exists(Dir)) Directory.CreateDirectory(Dir);

                                var rmt = adapterInMessage.Message as ResponseMessageType;

                                switch (adapterInMessage.Message.messageType)
                                {
                                    case "StatusMessage":
                                        mlog.InsertStatusOut(id.Value, MessageLoggerStatus.SUCCESS, resp.ResponseContent.status.description);
                                        fi.ReadMessage(mes);
                                        break;
                                    case "ErrorMessage":
                                        mlog.InsertStatusOut(id.Value, MessageLoggerStatus.ERROR, resp.ResponseContent.status.description);
                                        var path = Path.Combine(Dir, $"[{mes.Key}][ERR][RAW][{DateTime.Now.Hour}-{DateTime.Now.Minute}].xml");
                                        rmt.ResponseContent.SerializeToX().Save(path);
                                        fi.ReadMessage(mes);
                                        break;
                                    case "RejectMessage":
                                        mlog.InsertStatusOut(id.Value, MessageLoggerStatus.ERROR, string.Join(",", resp.ResponseContent.rejects.Select(x => $"{x.code}:{x.description}")));
                                        path = Path.Combine(Dir, $"[{mes.Key}][REJ][RAW][{DateTime.Now.Hour}-{DateTime.Now.Minute}].xml");
                                        rmt.ResponseContent.SerializeToX().Save(path);
                                        fi.ReadMessage(mes);
                                        break;
                                    case "PrimaryMessage":
                                        mlog.SetINMessage(id.Value, mes.Key, MessageLoggerStatus.INPUT);
                                        path = Path.Combine(Dir, $"[{mes.Key}][RES][RAW][{DateTime.Now.Hour}-{DateTime.Now.Minute}].xml");
                                        rmt.ResponseContent.content.MessagePrimaryContent.Save(path);
                                        mlog.UpdateStatusIN(id.Value, MessageLoggerStatus.SUCCESS);
                                        mlog.UpdateCommentIN(id.Value, $"SAVE: {path}");
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
                                AddLog($"Сообщение об ошибке из СМЭВ в потоке ПФР: {err.details}", LogType.Error);
                                var id = mlog.FindIDByMessageOut(err.statusMetadata.originalClientId);
                                if (!id.HasValue)
                                {
                                    throw new Exception($"Не удалось найти ID сообщения для [{mes.Key}]");
                                }
                                mlog.InsertStatusOut(id.Value, MessageLoggerStatus.ERROR, err.details);
                                fi.ReadMessage(mes);
                                continue;
                            }
                            throw new Exception($"Неизвестный тип сообщения [{mes.Key}]");
                        }
                        catch (Exception ex)
                        {
                            AddLog($"Ошибка потоке обработки ПФР: {ex.Message} ", LogType.Error);
                            fi.ErrorMessage(mes);
                        }
                    }
                    param.Text = "Ожидание сообщения";
                    Delay(param.TimeOut, cancel);
                }
            }
            catch (Exception ex)
            {
                AddLog($"Ошибка в потоке ПФР: {ex.Message}", LogType.Error);
            }
        }

        private async Task<List<string>> GetMessagePFPOut(string DIR)
        {
            try
            {

                var files = Directory.GetFiles(DIR, "*.xml").ToList();
                var removeItems = new List<string>();
                foreach (var file in files)
                {
                    if (await FileHelper.TryCheckFileAvAsync(file, 3, 3000))
                    {
                        removeItems.Add(file);
                    }
                }
                foreach (var file in removeItems)
                {
                    files.Remove(file);
                }
                return files;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка в GetMessagePFPOut: {ex.Message}", ex);
            }
        }

       

        private SendRequest CreatePFRData(string path, string itsystem, string reply)
        {
            var root = XDocument.Load(path);
            var ad = new SendRequest()
            {
                itSystem = itsystem,
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

        private Task task;
        private CancellationTokenSource CTS;
        public void StartProcess()
        {
            CTS = new CancellationTokenSource();
            _ = PFRAsync(CTS.Token);
        }

        public void StopProcess()
        {
            CTS?.Cancel();
        }

        public void Resent(int id)
        {
            throw new NotImplementedException();
        }

        public bool IsRunning => task?.Status == TaskStatus.Running;
        public string ItSystem => param.Config.ItSystem;

        public SMEV.WCFContract.VS VS => param.Config.VS;

        public string Text => param.Text;

    }


   

}
