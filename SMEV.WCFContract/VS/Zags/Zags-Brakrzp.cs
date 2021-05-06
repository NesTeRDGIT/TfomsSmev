using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using SmevAdapterService.VS;

namespace SMEV.VS.Zags
{
    //https://smev3.gosuslugi.ru/portal/inquirytype_one.jsp?id=140682&zone=fed&page=1&dTest=false
    #region Запрос о регистрации расторжения брака
    [Serializable]
    [XmlRoot(ElementName = "Request", Namespace = "urn://x-artefacts-zags-brakrzp/root/112-49/4.0.0", IsNullable = false)]
    public class Request_BRAKRZP : IRequestMessage
    {
        [XmlNamespaceDeclarations]
        public static XmlSerializerNamespaces XmlnsClass = new XmlSerializerNamespaces(new[]{ new XmlQualifiedName( "ns2","urn://x-artefacts-zags-brakrzp/root/112-49/4.0.0")});

        [XmlNamespaceDeclarations] public XmlSerializerNamespaces Xmlns = new XmlSerializerNamespaces(new[]
        {
            new XmlQualifiedName("ns2", "urn://x-artefacts-zags-brakrzp/root/112-49/4.0.0"),
            new XmlQualifiedName("ns4", "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/directive/1.3"),
            new XmlQualifiedName("ns3", "urn://x-artefacts-zags-brakrzp/types/4.0.0"),
            new XmlQualifiedName("ds", "http://www.w3.org/2000/09/xmldsig#"),
            new XmlQualifiedName("S", "http://schemas.xmlsoap.org/soap/envelope/"),
            new XmlQualifiedName("SOAP-ENV", "http://schemas.xmlsoap.org/soap/envelope/"),
            new XmlQualifiedName("ns0", "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/1.3"),
            new XmlQualifiedName("ns1", "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3")
        });

        [XmlElement(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/directive/1.3")]
        public Registry Registry { get; set; }


        public IResponseMessage Answer(string connectionString)
        {
            // var e = req.Registry.RegistryRecord[0].Record.RecordContent.BRAKZRZPRequest.ИдСвед;
            var e = Registry.RegistryRecord[0].Record.RecordContent.Attribute("ИдСвед")?.Value;
            return new BRAKRZPResponse
            {
                ИдСвед = e,
                КодОбр = BRAKRZPResponseКодОбр.Item1
            };
        }
    }


    #endregion
    #region ОТВЕТ Регистрация брака
    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-zags-brakrzp/root/112-49/4.0.0")]
    [XmlRoot(Namespace = "urn://x-artefacts-zags-brakrzp/root/112-49/4.0.0", IsNullable = false)]
    public class BRAKRZPResponse : IResponseMessage
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces Xmlns = new XmlSerializerNamespaces(new[]{ new XmlQualifiedName( "","urn://x-artefacts-zags-brakrzp/root/112-49/4.0.0") });

        [XmlAttribute]
        public string ИдСвед { get; set; }
        [XmlAttribute]
        public BRAKRZPResponseКодОбр КодОбр { get; set; }

        public XElement Serialize()
        {
            var xmlSerializer = new XmlSerializer(typeof(BRAKRZPResponse));
            var memoryStream = new MemoryStream();
            var xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8) {Formatting = Formatting.Indented};
            xmlSerializer.Serialize(xmlTextWriter, this, Xmlns);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return XElement.Load(memoryStream);
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-zags-brakrzp/root/112-49/4.0.0")]
    public enum BRAKRZPResponseКодОбр
    {
        [XmlEnum("01")]
        Item1
    }
    #endregion



    public interface IAnswer
    {

    }

}
