using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SMEV.WCFContract
{


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
