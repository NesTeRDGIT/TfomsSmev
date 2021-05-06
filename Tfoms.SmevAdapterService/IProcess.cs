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
using SMEV.VS.Zags;
using SMEV.WCFContract;
using SmevAdapterService.AdapterLayer.Integration;
using SmevAdapterService.AdapterLayer.XmlClasses;
using SmevAdapterService.VS;

using InputData = SMEV.VS.MedicalCare.V1_0_0.InputData;
using OutputData = SMEV.VS.MedicalCare.V1_0_0.OutputData;
using Zags4_0_1 = SMEV.VS.Zags4_0_1;

namespace SmevAdapterService
{
    public interface IProcess
    {
        void StartProcess(Configuration conf);
        void StopProcess();
        Dictionary<SMEV.WCFContract.VS, ProcessObrTask> CurrentWork { get; }
    }
    public class ProcessWork :IProcess
    {
        private Configuration Configuration;

        private ILogger logger;
        public ProcessWork(ILogger logger)
        {
            this.logger = logger;
        }

        private void AddLog(string log, LogType type)
        {
            logger?.AddLog(log, type);
        }
        public Dictionary<SMEV.WCFContract.VS, ProcessObrTask> CurrentWork { get; } = new Dictionary<SMEV.WCFContract.VS, ProcessObrTask>();

        public void StartProcess(Configuration conf)
        {
            Configuration = conf;
            foreach (var t in Configuration.ListVS)
            {
                Task task;
                var param = new ProcessObrTaskParam(t, Configuration.TimeOut * 1000);
                var CTS = new CancellationTokenSource();
                switch (t.VS)
                {
                    case SMEV.WCFContract.VS.MP:
                        task = new Task(() => Medpom(param, CTS.Token));
                        break;
                    case SMEV.WCFContract.VS.ZAGS:
                        task = new Task(() => ZAGS(param, CTS.Token));
                        break;
                    case SMEV.WCFContract.VS.PFR:
                        task = new Task(() => PFR(param, CTS.Token));
                        break;
                    default:
                        AddLog($"Ошибка запуска конфигурации {t.VS.ToString()} - нет обработчика", LogType.Error);
                        continue;
                }
                var po = new ProcessObrTask(task, CTS, param);

                CurrentWork.Add(t.VS, po);
                if (t.isEnabled)
                    task.Start();
            }
        }

        private void Delay(int MS)
        {
            var t = Task.Delay(MS);
            t.Wait();
        }

        private void Medpom(ProcessObrTaskParam param, CancellationToken cancel)
        {
            try
            {
                param.Text = "Начало работы";
                var Config_VS = param.Config;
                var mlog = new MessageLogger(Configuration.ConnectionString, Config_VS.ItSystem, logger);

                var ConnectionString = Config_VS.ConnectionString;
                param.Text = "Запуск интеграции";

                IRepository fi = null;
                switch (Config_VS.Integration)
                {
                    case Integration.FileSystem:
                        var fic = FileIntegrationConfig.Get(Config_VS.FilesConfig);
                        fi = new FileIntegration(fic); break;
                    case Integration.DataBase:
                        fi = new DataBaseIntegration(Config_VS.DataBaseConfig.ConnectionString); break;
                }
                while (!cancel.IsCancellationRequested)
                {
                    var mess = fi.GetMessage();
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
                            }

                            var resp = adapterInMessage.Message as ResponseMessageType;
                            if (resp != null)
                            {
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
                                                AddLog($"Пропущен статус доставки в СМЭВ для ID = {id.Value}", LogType.Warning);
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

                            var err = adapterInMessage.Message as ErrorMessage;
                            if (err != null)
                            {
                                AddLog($"Сообщение об ошибке из СМЭВ в потоке МП: {err.details}", LogType.Error);
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
                            AddLog($"Ошибка потоке обработки МП: {ex.Message} ", LogType.Error);
                            fi.ErrorMessage(mes);
                        }
                    }
                    param.Text = "Ожидание сообщения";
                    Delay(param.TimeOut);
                }
            }
            catch (Exception ex)
            {
                AddLog($"Ошибка в потоке Medpom: {ex.Message}", LogType.Error);
            }
        }

        private void ProcessInputData(IRepository fi, MessageIntegration mes, MessageLogger mlog, InputData inputdate, string MessageId, string replyToClientId, string ITSystem, string ConnectionString)
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
                throw new Exception($"Ошибка в ProcessInputData: {e.Message}", e);
            }
        }

        private void ProcessInputData_V2(IRepository fi, MessageIntegration mes, MessageLogger mlog, SMEV.VS.MedicalCare.newV1_0_0.ListOfMedicalServicesProvided.InputData inputdate, string MessageId, string replyToClientId, string ITSystem, string ConnectionString)
        {
            try
            {
                //Данные о человеке
                var idlog = mlog.AddInputMessage(MessageLoggerVS.InputData, MessageId, MessageLoggerStatus.INPUT, inputdate.orderId, "");
                var req = inputdate;
                mlog.SetMedpomDataIn(idlog, req.FamilyName, req.FirstName, req.Patronymic, req.BirthDate, req.DateFrom, req.DateTo, req.UnitedPolicyNumber, req.orderId);
                var clientId = mlog.GetGuidOut();
                //Ответ из БД
                var outdate = inputdate.Answer(ConnectionString);
                if (outdate != null)
                {
                    var ou = (SMEV.VS.MedicalCare.newV1_0_0.ListOfMedicalServicesProvided.OutputData)outdate;
                    ou.orderId = req.orderId;
                    //Ответ в БД
                    mlog.SetMedpomDataOut(idlog,ou.InsuredRenderingList.Select(x => new MessageLogger.SLUCH_REF(x.IsMTR, x.SLUCH_Z_ID, x.SLUCH_ID, x.USL_ID)).ToList());
                }
                //Исходящие сообщение
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
                throw new Exception($"Ошибка в ProcessInputData_V2: {e.Message}", e);
            }
        }

        public void ProcessFeedbackOnMedicalService(IRepository fi, MessageIntegration mes, MessageLogger mlog, SMEV.VS.MedicalCare.newV1_0_0.FeedbackOnMedicalService.InputData inputdate, string MessageId, string replyToClientId, string ITSystem, string ConnectionString)
        {
            //Данные о человеке
            var idlog = mlog.AddInputMessage(MessageLoggerVS.FeedbackOnMedicalService, MessageId, MessageLoggerStatus.INPUT, inputdate.orderId, inputdate.ApplicationID);
            mlog.SetFeedbackINFO(inputdate, idlog);
            fi.EndProcessMessage(mes);
            mlog.UpdateStatusIN(idlog, MessageLoggerStatus.SUCCESS);
        }

        public void ZAGS(ProcessObrTaskParam param, CancellationToken cancel)
        {
            try
            {
                param.Text = "Начало работы";
                var Config_VS = param.Config;
                var mlog = new MessageLogger(Configuration.ConnectionString, Config_VS.ItSystem, logger);

                param.Text = "Запуск интеграции";
                IRepository fi = null;

                switch (Config_VS.Integration)
                {
                    case Integration.FileSystem:
                        var fic = FileIntegrationConfig.Get(Config_VS.FilesConfig);
                        fi = new FileIntegration(fic); break;
                    case Integration.DataBase:
                        fic = new FileIntegrationConfig();
                        fi = new FileIntegration(fic); break;
                }
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
                                var idlog = mlog.AddInputMessage(NameType, MessageId, MessageLoggerStatus.INPUT, "", "");
                                mes.ID = idlog;
                                var replyToClientId = rmt.RequestMetadata.clientId;
                                var clientId = mlog.GetGuidOut();
                                var ms_out = new MessageIntegration { ID = idlog, Key = clientId };

                                if (!string.IsNullOrEmpty(Config_VS.TranspotrMessage))
                                {
                                    var Dir = Path.Combine(Config_VS.TranspotrMessage, NameType.ToString(), DateTime.Now.ToString("yyyyMMdd"));
                                    if (!Directory.Exists(Dir)) Directory.CreateDirectory(Dir);
                                    var path = Path.Combine(Dir, $"[{mes.Key}][REQ][RAW][{DateTime.Now.Hour.ToString()}-{DateTime.Now.Minute.ToString()}].xml");
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

                            var resp = adapterInMessage.Message as ResponseMessageType;
                            if (resp != null)
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

                                switch (resp.messageType)
                                {
                                    case "StatusMessage":
                                        var set = true;
                                        if (resp.ResponseContent.status.description?.ToUpper().Trim() == "Сообщение отправлено в СМЭВ".ToUpper().Trim())
                                        {
                                            var st = mlog.GetSTATUS_OUT(id.Value);
                                            if (st != MessageLoggerStatus.OUTPUT)
                                            {
                                                AddLog($"Пропущен статус доставки в СМЭВ для ID = {id.Value}", LogType.Warning);
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

                            var err = adapterInMessage.Message as ErrorMessage;
                            if (err != null)
                            {
                                AddLog($"Сообщение об ошибке из СМЭВ в потоке ЗАГС: {err.details}", LogType.Error);
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
                            AddLog($"Ошибка потоке обработки ЗАГС: {ex.Message} ", LogType.Error);
                            fi.ErrorMessage(mes);
                        }

                    }
                    param.Text = "Ожидание сообщения";
                    Delay(param.TimeOut);
                }
            }
            catch (Exception ex)
            {
                AddLog($"Ошибка в потоке ЗАГС: {ex.Message}", LogType.Error);
            }
        }
        public void PFR(ProcessObrTaskParam param, CancellationToken cancel)
        {
            try
            {
                param.Text = "Начало работы";
                var Config_VS = param.Config;
                var mlog = new MessageLogger(Configuration.ConnectionString, Config_VS.ItSystem, logger);

                param.Text = "Запуск интеграции";
                IRepository fi = null;
                switch (Config_VS.Integration)
                {
                    case Integration.FileSystem:
                        var fic = FileIntegrationConfig.Get(Config_VS.FilesConfig);
                        fi = new FileIntegration(fic); break;
                    case Integration.DataBase:
                        fic = new FileIntegrationConfig();
                        fi = new FileIntegration(fic); break;
                }

                while (!cancel.IsCancellationRequested)
                {
                    foreach (var file in GetMessagePFPOut(Config_VS.UserOutMessage))
                    {
                        var clientId = mlog.GetGuidOut();
                        var send = CreatePFRData(file, Config_VS.ItSystem, clientId);
                        var id = mlog.AddInputMessage(MessageLoggerVS.PFR_SNILS, "", MessageLoggerStatus.NONE, "", "");
                        mlog.SetOutMessage(id, clientId, MessageLoggerStatus.OUTPUT);
                        mlog.UpdateCommentOut(id, $"FILE: {file}");
                        var ms = new MessageIntegration { Key = clientId, ID = id, Content = send.SerializeToX() };
                        fi.SendMessage(ms);
                        var dirArc = Path.Combine(Config_VS.FilesConfig.ArchiveFolder, DateTime.Now.ToString("yyyy_MM_dd"), "UserOut");
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
                                                AddLog($"Пропущен статус доставки в СМЭВ для ID = {id.Value}", LogType.Warning);
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
                                AddLog($"Сообщение об ошибке из СМЭВ в потоке ПФР: {err.details}", LogType.Error);
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
                            AddLog($"Ошибка потоке обработки ПФР: {ex.Message} ", LogType.Error);
                            fi.ErrorMessage(mes);
                        }

                    }
                    param.Text = "Ожидание сообщения";
                    Delay(param.TimeOut);
                }
            }
            catch (Exception ex)
            {
                AddLog($"Ошибка в потоке ПФР: {ex.Message}", LogType.Error);
            }
        }

        public List<string> GetMessagePFPOut(string DIR)
        {
            try
            {
                return Directory.GetFiles(DIR, "*.xml").ToList();

            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка в GetMessagePFPOut: {ex.Message}", ex);
            }
        }

        private SendRequest CreatePFRData(string Path, string ITSYSTEM, string reply)
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

        public void StopProcess()
        {
            foreach (var t in CurrentWork)
            {
                if (t.Value.Task?.Status == TaskStatus.Running)
                    t.Value.Cancel.Cancel();
            }
            CurrentWork.Clear();
        }


    }

}
