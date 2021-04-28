
using System.Xml.Serialization;
using System;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Text;
using SmevAdapterService.VS;
namespace SmevAdapterService.VS.Zags
{
    #region ЗАПРОС Регистрация смерти

    [Serializable()]
    [XmlRoot(ElementName = "Request", Namespace = "urn://x-artefacts-zags-fatalzp/root/112-25/4.0.0", IsNullable = false)]
    public partial class Request_FATALZP : IRequestMessage
    {
        [XmlNamespaceDeclarations()]
        public static XmlSerializerNamespaces XmlnsClass = new XmlSerializerNamespaces(new XmlQualifiedName[]
{ new XmlQualifiedName( "ns2","urn://x-artefacts-zags-fatalzp/root/112-25/4.0.0"),

});

        private Registry registry;

        [XmlElement(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/directive/1.3")]
        public Registry Registry
        {
            get
            {
                return registry;
            }

            set
            {
                registry = value;
            }
        }

        public IResponseMessage Answer(string connectionString)
        {
            var e = Registry.RegistryRecord[0].Record.RecordContent.Attribute("ИдСвед").Value;
            return new FATALZPResponse()
            {
                ИдСвед = e,
                КодОбр = FATALZPResponseКодОбр.Item1
            };
        }

    }

    #endregion
    #region ОТВЕТ Регистрация смерти
    [Serializable()]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-zags-fatalzp/root/112-25/4.0.0")]
    [XmlRoot(Namespace = "urn://x-artefacts-zags-fatalzp/root/112-25/4.0.0", IsNullable = false)]
    public partial class FATALZPResponse : IResponseMessage
    {
        [XmlNamespaceDeclarations()]
        public XmlSerializerNamespaces Xmlns = new XmlSerializerNamespaces(new XmlQualifiedName[]
  { new XmlQualifiedName( "","urn://x-artefacts-zags-fatalzp/root/112-25/4.0.0") });

        private string идСведField;

        private FATALZPResponseКодОбр кодОбрField;

        [XmlAttribute()]
        public string ИдСвед
        {
            get
            {
                return идСведField;
            }
            set
            {
                идСведField = value;
            }
        }


        [XmlAttribute()]
        public FATALZPResponseКодОбр КодОбр
        {
            get
            {
                return кодОбрField;
            }
            set
            {
                кодОбрField = value;
            }
        }

        XElement IResponseMessage.Serialize()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(FATALZPResponse));
            MemoryStream memoryStream = new MemoryStream();
            XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            xmlTextWriter.Formatting = Formatting.Indented;
            xmlSerializer.Serialize(xmlTextWriter, this, this.Xmlns);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return XElement.Load(memoryStream);
        }
    }

    [Serializable()]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-zags-fatalzp/root/112-25/4.0.0")]
    public enum FATALZPResponseКодОбр
    {


        [XmlEnum("1")]
        Item1
    }
}
    #endregion
