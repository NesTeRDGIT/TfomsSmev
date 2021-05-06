using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml.Serialization;

namespace SMEV.WCFContract
{
    [DataContract]
    public enum Integration
    {
        [EnumMember]
        FileSystem,
        [EnumMember]
        DataBase
    }
    /// <summary>
    /// Вид сведений
    /// </summary>
    [DataContract]
    public enum VS
    {
        [EnumMember]
        MP = 0,
        [EnumMember]
        ZAGS = 1,
        [EnumMember]
        PFR = 2
    }
    public class VSWorkProcess
    {
        public VS VS { get; set; }
        public bool Activ { get; set; }
        public string ItSystem { get; set; }
        public string Text { get; set; }
    }


    public interface IConfigurationManager
    {
        Configuration config { get; set; }
        void Save();
        void Load();
    }


    public class Configuration : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        string _ConnectionString;
        public string ConnectionString { get { return _ConnectionString; } set { _ConnectionString = value; OnPropertyChanged(); } }
        int _TimeOut = 1;
        public int TimeOut { get { return _TimeOut; } set { _TimeOut = value; OnPropertyChanged(); } }

        List<Config_VS> _ListVS = new List<Config_VS>();
        public List<Config_VS> ListVS { get { return _ListVS; } set { _ListVS = value; OnPropertyChanged(); } }

        public void Check()
        {
            foreach (var ft in (VS[])Enum.GetValues(typeof(VS)))
            {
                if (ListVS.Count(x => x.VS == ft) == 0)
                {
                    ListVS.Add(new Config_VS { VS = ft, isEnabled = false });
                }
            }
        }

        public static Configuration LoadFromFile(string Path)
        {
            using (Stream st = File.OpenRead(Path))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Configuration));
                var config = (Configuration)xmlSerializer.Deserialize(st);
                config.Check();
                return config;
            }
        }

        public static void SaveToFile(string Path, Configuration config)
        {
            using (Stream st = File.Create(Path))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Configuration));
                xmlSerializer.Serialize(st, config);
            }
        }
    }    
    public class Config_VS : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        bool _isEnabled;
        /// <summary>
        /// Включена обработка или нет
        /// </summary>
        public bool isEnabled { get { return _isEnabled; } set { _isEnabled = value; OnPropertyChanged(); } }  
        string _ItSystem;
        /// <summary>
        /// Мнемоника
        /// </summary>
        public string ItSystem { get { return _ItSystem ?? ""; } set { _ItSystem = value; OnPropertyChanged(); } }
        /// <summary> 
        string _ConnectionString;
        /// <summary>
        /// Строка подключения
        /// </summary>
        public string ConnectionString { get { return _ConnectionString ?? ""; } set { _ConnectionString = value; OnPropertyChanged(); } }
        VS _VS;
        /// <summary> 
        /// <summary>
        /// Вид сведений
        /// </summary>
        public VS VS { get { return _VS; } set { _VS = value; OnPropertyChanged(); } }
        Integration _Integration;
        /// <summary>
        /// Интеграция
        /// </summary>
        public Integration Integration { get { return _Integration; } set { _Integration = value; OnPropertyChanged(); } }
    /// Конфиг файловой интеграции
    /// </summary>
    /// 
    FileIntegrationSet _FilesConfig = new FileIntegrationSet();
        public FileIntegrationSet FilesConfig { get { return _FilesConfig ?? new FileIntegrationSet(); } set { _FilesConfig = value; OnPropertyChanged(); } }
        DataBaseIntegrationSet _DataBaseConfig = new DataBaseIntegrationSet();
        public DataBaseIntegrationSet DataBaseConfig { get { return _DataBaseConfig ?? new DataBaseIntegrationSet(); } set { _DataBaseConfig = value; OnPropertyChanged(); } }

        string _TranspotrMessage = "";
        /// <summary>
        /// ПУть для сохранения сообщения СМЭВ(MessagePrimaryContent)
        /// </summary>
        public string TranspotrMessage { get { return _TranspotrMessage ?? ""; } set { _TranspotrMessage = value; OnPropertyChanged(); } }

        string _UserOutMessage = "";
        /// <summary>
        /// ПУть для поиска отправляемых в СМЭВ сообщений пользователей
        /// </summary>
        public string UserOutMessage { get { return _UserOutMessage ?? ""; } set { _UserOutMessage = value; OnPropertyChanged(); } }
    }
    public class FileIntegrationSet : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        string _InputFolder;
        /// <summary>
        /// Папка входящих файлов
        /// </summary>
        public string InputFolder { get { return _InputFolder; } set { _InputFolder = value; OnPropertyChanged(); } }
        string _OutputFolder;
        /// <summary>
        /// Папка исходящих файлов
        /// </summary>
        /// 
        public string OutputFolder { get { return _OutputFolder; } set { _OutputFolder = value; OnPropertyChanged(); } }
        string _PoccessFolder;
        /// <summary>
        /// Папка обработки
        /// </summary>
        public string PoccessFolder { get { return _PoccessFolder; } set { _PoccessFolder = value; OnPropertyChanged(); } }
        string _ArchiveFolder;
        /// <summary>
        /// Папка архива
        /// </summary>
        public string ArchiveFolder { get { return _ArchiveFolder; } set { _ArchiveFolder = value; OnPropertyChanged(); } }

    }
    public class DataBaseIntegrationSet : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        string _ConnectionString;
        /// <summary>
        /// Строка подключения
        /// </summary>
        public string ConnectionString { get { return _ConnectionString; } set { _ConnectionString = value; OnPropertyChanged(); } }
        

    }
    [DataContract]
    public enum MessageLoggerStatus
    {
        [EnumMember]
        NONE = 0,
        [EnumMember]
        INPUT = 1,
        [EnumMember]
        OUTPUT = 2,
        [EnumMember]
        SUCCESS = 3,
        [EnumMember]
        ERROR = 4
       
    }
    [DataContract]
    public enum MessageLoggerVS
    {
        [EnumMember]
        UNKNOW = 0,
        [EnumMember]
        InputData = 1,
        [EnumMember]
        Request_BRAKZRZP = 2,
        [EnumMember]
        Request_FATALZP = 3,
        [EnumMember]
        Request_PARENTZP = 4,
        [EnumMember]
        Request_PERNAMEZP =5,
        [EnumMember]
        Request_ROGDZP = 6,
        [EnumMember]
        Request_FATALZP4_0_1 = 7,
        [EnumMember]
        Request_PARENTZP4_0_1 = 8,
        [EnumMember]
        Request_PERNAMEZP4_0_1 = 9,
        [EnumMember]
        Request_ROGDZP4_0_1 = 10,
        [EnumMember]
        Request_BRAKRZP = 11,
        [EnumMember]
        Request_BRAKZZP = 12,
        [EnumMember]
        PFR_SNILS = 13,
        [EnumMember]
        FeedbackOnMedicalService = 14
    }
    public class LogRow
    {
        public static List<LogRow> Get(IEnumerable<DataRow> rows)
        {
            return rows.Select(Get).ToList();
        }

        public static LogRow Get(DataRow row)
        {
            try
            {
                var item = new LogRow();
                item.ID = Convert.ToInt32(row["ID"]);
                item.ITSYSTEM = Convert.ToString(row["ITSYSTEM"]);
                item.VS = (MessageLoggerVS)Convert.ToInt32(row["VS"]);
                item.ID_MESSAGE_IN = Convert.ToString(row["ID_MESSAGE_IN"]);
                if (row["STATUS_IN"] != DBNull.Value)
                    item.STATUS_IN = MessageLoggerStatusFromString(Convert.ToInt32(row["STATUS_IN"]));
                item.COMMENT_IN = Convert.ToString(row["COMMENT_IN"]);
                item.ID_MESSAGE_OUT = Convert.ToString(row["ID_MESSAGE_OUT"]);
                if(row["STATUS_OUT"]!=DBNull.Value)
                    item.STATUS_OUT = MessageLoggerStatusFromString(Convert.ToInt32(row["STATUS_OUT"]));
                item.COMMENT_OUT = Convert.ToString(row["COMMENT_OUT"]);
                if(row["DATE_IN"]!=DBNull.Value)
                    item.DATE_IN = Convert.ToDateTime(row["DATE_IN"]);
                if (row["DATE_OUT"] != DBNull.Value)
                    item.DATE_OUT = Convert.ToDateTime(row["DATE_OUT"]);

                item.OrderId = Convert.ToString(row["OrderId"]);
                item.ApplicationId = Convert.ToString(row["ApplicationId"]);
                return item;
            }
            catch(Exception ex)
            {
                throw new Exception($"Ошибка получения LogRow: {ex.Message}", ex);
            }
        }

        private static MessageLoggerStatus? MessageLoggerStatusFromString(int value)
        {
            return (MessageLoggerStatus)value;
        }

        /// <summary>
        /// Ид сообщения в БД
        /// </summary>
        public int ID { get; set; }    
        /// <summary>
        /// Мнемоника
        /// </summary>
        public string ITSYSTEM { get; set; } = "";
        /// <summary>
        /// Вид сведений
        /// </summary>
        public MessageLoggerVS VS { get; set; } = MessageLoggerVS.UNKNOW;
        /// <summary>
        /// Ид входящего сообщения СМЭВ
        /// </summary>
        public string ID_MESSAGE_IN { get; set; } = "";
        /// <summary>
        /// Статус входящего сообщения СМЭВ
        /// </summary>
        public MessageLoggerStatus? STATUS_IN { get; set; }
        /// <summary>
        /// Коментарий сообщения СМЭВ
        /// </summary>
        public string COMMENT_IN { get; set; } = "";
        /// <summary>
        /// Ид исходящего сообщения СМЭВ
        /// </summary>
        public string ID_MESSAGE_OUT { get; set; } = "";
        /// <summary>
        /// Статус исходящего сообщения СМЭВ
        /// </summary>
        public MessageLoggerStatus? STATUS_OUT { get; set; } 
        /// <summary>
        /// Коментарий исходящего сообщения СМЭВ
        /// </summary>
        public string COMMENT_OUT { get; set; } = "";
        public DateTime? DATE_IN { get; set; }
        public DateTime? DATE_OUT { get; set; }

        public string OrderId { get; set; } = "";
        public string ApplicationId { get; set; } = "";


    }
    [ServiceContract(CallbackContract = typeof(IWcfInterfaceCallback), SessionMode = SessionMode.Required)]
    public interface IWcfInterface
    {
        /// <summary>
        /// Получить список дириктории
        /// </summary>
        /// <param name="path">Путь</param>
        /// <returns>Список директорий</returns>
        [OperationContract]
        string[] GetFolderLocal(string path);
        /// <summary>
        /// Получиться список локальных дисков
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        string[] GetLocalDisk();

        [OperationContract]
        Configuration GetConfig();
        [OperationContract]
        void SetConfig(Configuration config);
        [OperationContract]
        List<EntriesMy> GetEventLogEntry(int Count,bool HideWarning);
        [OperationContract]
        List<VSWorkProcess> GetDoWork();
        [OperationContract]
        List<LogRow> GetLog(int Count, MessageLoggerVS[] VS, DateTime? DATE_B, DateTime? DATE_E);
        [OperationContract]
        MedpomData GetMedpomData(int ID);
        [OperationContract]
        FeedBackData GetFeedBackData(int ID);
        [OperationContract]
        List<ReportRow> GetReport(DateTime DATE_B, DateTime DATE_E);

        [OperationContract]
        bool Ping();

        [OperationContract]
        PingResult PingAdress();
        [OperationContract]
        void PingParamSet(PingConfig PC);
        [OperationContract]
        PingConfig PingParamGet();
        [OperationContract]
        void Register();
        [OperationContract]
        void DeleteLog(int[] IDs);
        [OperationContract]
        void ChangeActivProcess(VS vS, bool v);
    }
    [ServiceContract]
    public interface IWcfInterfaceCallback
    {
        [OperationContract(IsOneWay = true)]
        void PingResult(PingResult PR);

        [OperationContract(IsOneWay = true)]
        void Ping();
    }
    [DataContract]
    public class MedpomData
    {
        public MedpomData(MedpomInData IN, List<MedpomOutData> OUT)
        {
            this.IN = IN;
            this.OUT = OUT;
        }
        [DataMember]
        public MedpomInData IN { get; set; }
        [DataMember]
        public List<MedpomOutData> OUT { get; set; }
    }
    [DataContract]
    public class MedpomInData
    {
        public static MedpomInData Get(DataRow row)
        {
            MedpomInData item = new MedpomInData();
            try
            {
                item.log_service_id = Convert.ToInt32(row["log_service_id"]);
                item.familyname = Convert.ToString(row["familyname"]);
                item.firstname = Convert.ToString(row["firstname"]);
                item.patronymic = Convert.ToString(row["patronymic"]);
                item.birthdate = Convert.ToDateTime(row["birthdate"]);
                item.datefrom = Convert.ToDateTime(row["datefrom"]);
                item.dateto = Convert.ToDateTime(row["dateto"]);
                item.unitedpolicynumber = Convert.ToString(row["unitedpolicynumber"]);
                return item;
            }
            catch(Exception ex)
            {
                throw new Exception($"Ошибка получения MedpomInData: {ex.Message}", ex);
            }
        }
        [DataMember]
        public int log_service_id { get; set; }
        [DataMember]
        public string familyname { get; set; }
        [DataMember]
        public string firstname { get; set; }
        [DataMember]
        public string patronymic { get; set; }
        [DataMember]
        public DateTime birthdate { get; set; }
        [DataMember]
        public DateTime datefrom { get; set; }
        [DataMember]
        public DateTime dateto { get; set; }
        [DataMember]
        public string unitedpolicynumber { get; set; }
    }
    [DataContract]
    public class MedpomOutData
    {
        public static List<MedpomOutData> Get(IEnumerable<DataRow> rows)
        {
            return rows.Select(Get).ToList();
        }

        public static MedpomOutData Get(DataRow row)
        {
            MedpomOutData item = new MedpomOutData();
            try
            {
                item.IsMTR = Convert.ToBoolean(row["IsMTR"]);
                item.SLUCH_Z_ID = Convert.ToInt32(row["SLUCH_Z_ID"]);
                item.SLUCH_ID = Convert.ToInt32(row["SLUCH_ID"]);
                if(row["USL_ID"]!=DBNull.Value)
                    item.USL_ID = Convert.ToInt32(row["USL_ID"]);                
                return item;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Ошибка получения MedpomOutData: {0}", ex.Message), ex);
            }
        }
        [DataMember]
        public bool IsMTR { get; set; }
        [DataMember]
        public int SLUCH_Z_ID { get; set; }
        [DataMember]
        public int SLUCH_ID { get; set; }
        [DataMember]
        public int? USL_ID { get; set; }        
    }

    [DataContract]
    public class ReportRow
    {
        public static ReportRow Get(DataRow row)
        {
            try
            {
                var item = new ReportRow();
                if (row["dt"] != DBNull.Value)
                    item.dt = Convert.ToDateTime(row["dt"]);
                item.Count = Convert.ToInt32(row["Count"]);
                item.People = Convert.ToInt32(row["People"]);
                item.USL = Convert.ToInt32(row["USL"]);
                item.Answer = Convert.ToInt32(row["Answer"]);
                item.Error = Convert.ToInt32(row["Error"]);
                item.noAnswer = Convert.ToInt32(row["noAnswer"]);
                
                return item;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка получения ReportRow: {ex.Message}", ex);
            }
        }
        [DataMember]
        public DateTime? dt { get; set; }
        [DataMember]
        public int Count { get; set; }
        [DataMember]
        public int People { get; set; }
        [DataMember]
        public int USL { get; set; }
        [DataMember]
        public int Answer { get; set; }
        [DataMember]
        public int Error { get; set; }
        [DataMember]
        public int noAnswer { get; set; }
    }



    [DataContract]
    public class FeedBackData
    {
        public FeedBackData(List<FeedBackDataIN> IN)
        {
            this.IN = IN;
        }
        [DataMember]
        public List<FeedBackDataIN> IN { get; set; }
     
    }


    [DataContract]
    public class FeedBackDataIN
    {
        public static List<FeedBackDataIN> Get(IEnumerable<DataRow> rows)
        {
            return rows.Select(Get).ToList();
        }

        public static FeedBackDataIN Get(DataRow row)
        {
            var item = new FeedBackDataIN();
            try
            {
                item.AppealNumber = Convert.ToString(row["AppealNumber"]);
                item.FeedbackInfo_Id = Convert.ToInt32(row["FeedbackInfo_Id"]);
                item.Log_Service_Id = Convert.ToInt32(row["Log_Service_Id"]);
                item.AppealTopic = Convert.ToString(row["AppealTopic"]);
                item.AppealDetail = Convert.ToString(row["AppealDetail"]);
                item.MedservicesId = Convert.ToString(row["MedservicesId"]);
                item.AppealTopicCode = Convert.ToInt32(row["AppealTopicCode"]);
                item.CareRegimen = Convert.ToString(row["CareRegimen"]);
                item.DateRenderingFrom = Convert.ToDateTime(row["DateRenderingFrom"]);
                item.DateRenderingTo = Convert.ToDateTime(row["DateRenderingTo"]);
                item.RegionCode = Convert.ToString(row["RegionCode"]);
                item.CareType = Convert.ToString(row["CareType"]);
                item.MedservicesSum = Convert.ToDecimal(row["MedservicesSum"]);
                return item;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка получения FeedBackDataIN: {ex.Message}", ex);
            }
        }
        [DataMember]
        public string AppealNumber { get; set; }
        [DataMember]
        public int FeedbackInfo_Id { get; set; }
        [DataMember]
        public int Log_Service_Id { get; set; }
        [DataMember]
        public string AppealTopic { get; set; }
        [DataMember]
        public string AppealDetail { get; set; }
        [DataMember]
        public string MedservicesId { get; set; }
        [DataMember]
        public int AppealTopicCode { get; set; }
        [DataMember]
        public string CareRegimen { get; set; }
        [DataMember]
        public DateTime DateRenderingFrom { get; set; }
        [DataMember]
        public DateTime DateRenderingTo { get; set; }
        [DataMember]
        public string RegionCode { get; set; }
        [DataMember]
        public string CareType { get; set; }
        [DataMember]
        public decimal MedservicesSum { get; set; }
     
    }


    public interface IPingManager
    {
        PingConfig config { get; set; }
        void Start();
        void Stop();
        void LoadConfig();
        void SaveConfig();
        PingResult Ping();
    }


    [DataContract]
    public class PingConfig
    {
        [DataMember]
        public string Adress { get; set; } = "";
        [DataMember]
        public bool IsEnabled { get; set; }
        [DataMember]
        public int TimeOut { get; set; }
        [DataMember]
        public string[] Process { get; set; } 
        public static PingConfig LoadFromFile(string Path)
        {
            using (Stream st = File.OpenRead(Path))
            {
                var xmlSerializer = new XmlSerializer(typeof(PingConfig));
                var config = (PingConfig)xmlSerializer.Deserialize(st);
                return config;
            }
        }

        public static void SaveToFile(string Path, PingConfig config)
        {
            using (Stream st = File.Create(Path))
            {
                var xmlSerializer = new XmlSerializer(typeof(PingConfig));
                xmlSerializer.Serialize(st, config);
            }
        }
    }

    [DataContract]
    public class PingResult
    {
        [DataMember]
        public string Adress { get; set; } = "";
        [DataMember]
        public bool Result { get; set; }
        [DataMember]
        public string Text { get; set; }
        [DataMember]
        public DateTime DT { get; set; } = DateTime.Now;

    }


}
