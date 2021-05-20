using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using SmevAdapterService.VS;

namespace SMEV.VS.Zags.V4_0_0
{
    #region ЗАПРОС Регистрация рождения


    [Serializable]
    [XmlRoot(ElementName = "Request", Namespace = "urn://x-artefacts-zags-rogdzp/root/112-23/4.0.0", IsNullable = false)]
    public class Request_ROGDZP : IRequestMessage
    {
        [XmlElement(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/directive/1.3")]
        public Registry Registry { get; set; }

        public IResponseMessage Answer(string connectionString)
        {
            var e = Registry.RegistryRecord[0].Record.RecordContent.Attribute("ИдСвед")?.Value;
            return new ROGDZPResponse
            {
                ИдСвед = e
            };
        }

    }
    #endregion

    #region ОТВЕТ Регистрация рождения

    [Serializable]
    [XmlRoot(Namespace = "urn://x-artefacts-zags-rogdzp/root/112-23/4.0.0", IsNullable = false)]
    public class ROGDZPResponse : IResponseMessage
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces Xmlns = new XmlSerializerNamespaces(new[]{ new XmlQualifiedName("","urn://x-artefacts-zags-rogdzp/root/112-23/4.0.0") });
        [XmlAttribute] public string ИдСвед { get; set; }
        [XmlAttribute] public string КодОбр { get; set; } = "1";

        XElement IResponseMessage.Serialize()
        {
            var xmlSerializer = new XmlSerializer(typeof(ROGDZPResponse));
            var memoryStream = new MemoryStream();
            var xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            xmlTextWriter.Formatting = Formatting.Indented;
            xmlSerializer.Serialize(xmlTextWriter, this, Xmlns);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return XElement.Load(memoryStream);
        }
    }
    #endregion
}