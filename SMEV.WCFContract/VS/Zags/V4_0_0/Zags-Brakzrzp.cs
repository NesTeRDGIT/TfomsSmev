using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using SmevAdapterService.VS;

namespace SMEV.VS.Zags.V4_0_0
{
    //Старое
    /// <summary>
    /// https://smev3.gosuslugi.ru/portal/inquirytype_one.jsp?id=101136&zone=fed&page=1&dTest=false
    /// </summary>
    #region Запрос о регистрации брака
    [Serializable]
    [XmlRoot(ElementName = "Request", Namespace = "urn://x-artefacts-zags-brakzrzp/root/112-26/4.0.0", IsNullable = false)]
    public class Request_BRAKZRZP : IRequestMessage
    {
        [XmlElement(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/directive/1.3")]
        public Registry Registry { get; set; }

        public IResponseMessage Answer(string connectionString)
        {
            // var e = req.Registry.RegistryRecord[0].Record.RecordContent.BRAKZRZPRequest.ИдСвед;
            var e = Registry.RegistryRecord[0].Record.RecordContent.Attribute("ИдСвед")?.Value;
            return new BRAKZRZPResponse
            {
                ИдСвед = e
            };
        }
    }


    #endregion
    #region ОТВЕТ Регистрация брака
    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-zags-brakzrzp/root/112-26/4.0.0")]
    [XmlRoot(Namespace = "urn://x-artefacts-zags-brakzrzp/root/112-26/4.0.0", IsNullable = false)]
    public class BRAKZRZPResponse : IResponseMessage
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces Xmlns = new XmlSerializerNamespaces(new[]{ new XmlQualifiedName( "","urn://x-artefacts-zags-brakzrzp/root/112-26/4.0.0") });

        [XmlAttribute]
        public string ИдСвед { get; set; }

        [XmlAttribute] 
        public string КодОбр { get; set; } = "1";
        public XElement Serialize()
        {
            var xmlSerializer = new XmlSerializer(typeof(BRAKZRZPResponse));
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
