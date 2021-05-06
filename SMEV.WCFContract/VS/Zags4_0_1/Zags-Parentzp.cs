using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using SMEV.VS.Zags;
using SmevAdapterService.VS;

namespace SMEV.VS.Zags4_0_1
{

    //https://smev3.gosuslugi.ru/portal/inquirytype_one.jsp?id=140725&zone=fed&page=1&dTest=false
    #region Запрос Установление отцовства

    [Serializable]
    [XmlRoot(ElementName = "Request", Namespace = "urn://x-artefacts-zags-parent/root/112-27/4.0.1", IsNullable = false)]
    public class Request_PARENTZP : IRequestMessage
    {
        [XmlNamespaceDeclarations]
        public static XmlSerializerNamespaces XmlnsClass = new XmlSerializerNamespaces(new[]{ new XmlQualifiedName( "ns2","urn://x-artefacts-zags-parent/root/112-27/4.0.1")});
       
        [XmlElement(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/directive/1.3")]
        public Registry Registry { get; set; }



        public IResponseMessage Answer(string connectionString)
        {
            var e = Registry.RegistryRecord[0].Record.RecordContent.Attribute("ИдСвед").Value;
            return new PARENTZPResponse
            {
                ИдСвед = e,
                КодОбр = PARENTZPResponseКодОбр.Item1
            };
        }
    }
    #endregion
    #region ОТВЕТ Установление отцовства

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-zags-parent/root/112-27/4.0.1")]
    [XmlRoot(Namespace = "urn://x-artefacts-zags-parent/root/112-27/4.0.1", IsNullable = false)]
    public class PARENTZPResponse : IResponseMessage
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces Xmlns = new XmlSerializerNamespaces(new[]{ new XmlQualifiedName( "","urn://x-artefacts-zags-parent/root/112-27/4.0.1") });

        [XmlAttribute]
        public string ИдСвед { get; set; }

        [XmlAttribute]
        public PARENTZPResponseКодОбр КодОбр { get; set; }

        XElement IResponseMessage.Serialize()
        {
            var xmlSerializer = new XmlSerializer(typeof(PARENTZPResponse));
            using (var memoryStream = new MemoryStream())
            {
                using (var xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8))
                {
                    xmlTextWriter.Formatting = Formatting.Indented;
                    xmlSerializer.Serialize(xmlTextWriter, this, Xmlns);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return XElement.Load(memoryStream);
                }
            }
          
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-zags-parent/root/112-27/4.0.1")]
    public enum PARENTZPResponseКодОбр
    {
        [XmlEnum("1")]
        Item1
    }

    #endregion
}
