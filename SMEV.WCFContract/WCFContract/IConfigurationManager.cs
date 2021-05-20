using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SMEV.WCFContract
{
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
        PFR = 2,
        [EnumMember]
        Cabinet = 3
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
        public string ConnectionString
        {
            get => _ConnectionString;
            set { _ConnectionString = value; OnPropertyChanged(); }
        }
        int _TimeOut = 1;
        public int TimeOut
        {
            get => _TimeOut;
            set { _TimeOut = value; OnPropertyChanged(); }
        }

        List<Config_VS> _ListVS = new List<Config_VS>();
        public List<Config_VS> ListVS
        {
            get => _ListVS;
            set { _ListVS = value; OnPropertyChanged(); }
        }

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
        public bool isEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }

        string _ItSystem;

        /// <summary>
        /// Мнемоника
        /// </summary>
        public string ItSystem
        {
            get => _ItSystem ?? "";
            set
            {
                _ItSystem = value;
                OnPropertyChanged();
            }
        }

        /// <summary> 
        string _ConnectionString;

        /// <summary>
        /// Строка подключения
        /// </summary>
        public string ConnectionString
        {
            get => _ConnectionString ?? "";
            set
            {
                _ConnectionString = value;
                OnPropertyChanged();
            }
        }

        string _ConnectionString2;

        /// <summary>
        /// Строка подключения
        /// </summary>
        public string ConnectionString2
        {
            get => _ConnectionString2 ?? "";
            set
            {
                _ConnectionString2 = value;
                OnPropertyChanged();
            }
        }

        VS _VS;

        /// <summary> 
        /// <summary>
        /// Вид сведений
        /// </summary>
        public VS VS
        {
            get => _VS;
            set
            {
                _VS = value;
                OnPropertyChanged();
            }
        }

        /// Конфиг файловой интеграции
        /// </summary>
        /// 
        FileIntegrationSet _FilesConfig = new FileIntegrationSet();

        public FileIntegrationSet FilesConfig
        {
            get => _FilesConfig ?? new FileIntegrationSet();
            set
            {
                _FilesConfig = value;
                OnPropertyChanged();
            }
        }


        string _TranspotrMessage = "";

        /// <summary>
        /// ПУть для сохранения сообщения СМЭВ(MessagePrimaryContent)
        /// </summary>
        public string TranspotrMessage
        {
            get => _TranspotrMessage ?? "";
            set
            {
                _TranspotrMessage = value;
                OnPropertyChanged();
            }
        }

        string _UserOutMessage = "";

        /// <summary>
        /// ПУть для поиска отправляемых в СМЭВ сообщений пользователей
        /// </summary>
        public string UserOutMessage
        {
            get => _UserOutMessage ?? "";
            set
            {
                _UserOutMessage = value;
                OnPropertyChanged();
            }
        }
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
        public string InputFolder
        {
            get => _InputFolder;
            set { _InputFolder = value; OnPropertyChanged(); }
        }
        string _OutputFolder;
        /// <summary>
        /// Папка исходящих файлов
        /// </summary>
        /// 
        public string OutputFolder
        {
            get => _OutputFolder;
            set { _OutputFolder = value; OnPropertyChanged(); }
        }
        string _PoccessFolder;
        /// <summary>
        /// Папка обработки
        /// </summary>
        public string PoccessFolder
        {
            get => _PoccessFolder;
            set { _PoccessFolder = value; OnPropertyChanged(); }
        }
        string _ArchiveFolder;
        /// <summary>
        /// Папка архива
        /// </summary>
        public string ArchiveFolder
        {
            get => _ArchiveFolder;
            set { _ArchiveFolder = value; OnPropertyChanged(); }
        }

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
        Request_PERNAMEZP = 5,
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
        FeedbackOnMedicalService = 14,
        [EnumMember]
        InputDataSiteTFOMS = 15,
    }
}
