using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using SmevAdapterService.VS;

namespace SMEV.VS.Zags.V4_0_0
{
    /// <summary>
    /// https://smev3.gosuslugi.ru/portal/inquirytype_one.jsp?id=140607&zone=fed&page=1&dTest=false
    /// </summary>
    #region Сведения из ЕГР ЗАГС о государственной регистрации заключения брака
    [Serializable]
    [XmlRoot(ElementName = "Request", Namespace = "urn://x-artefacts-zags-brakzzp/root/112-47/4.0.0", IsNullable = false)]
    public class Request_BRAKZZP : IRequestMessage
    {
      
        [XmlElement(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/directive/1.3")]
        public Registry Registry { get; set; }


        public IResponseMessage Answer(string connectionString)
        {
            var e = Registry.RegistryRecord[0].Record.RecordContent.Attribute("ИдСвед")?.Value;
            return new BRAKZZPResponse
            {
                ИдСвед = e
            };
        }
    }


    #endregion
    #region ОТВЕТ Регистрация брака
    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-zags-brakzzp/root/112-47/4.0.0")]
    [XmlRoot(Namespace = "urn://x-artefacts-zags-brakzzp/root/112-47/4.0.0", IsNullable = false)]
    public class BRAKZZPResponse : IResponseMessage
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces Xmlns = new XmlSerializerNamespaces(new[]{ new XmlQualifiedName( "","urn://x-artefacts-zags-brakzrzp/root/112-26/4.0.0") });

        [XmlAttribute]
        public string ИдСвед { get; set; }


        [XmlAttribute] 
        public string КодОбр { get; set; } = "01";


        public XElement Serialize()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(BRAKZZPResponse));
            MemoryStream memoryStream = new MemoryStream();
            XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, new UTF8Encoding(false));
            xmlTextWriter.Formatting = Formatting.Indented;
            xmlSerializer.Serialize(xmlTextWriter, this, Xmlns);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return XElement.Load(memoryStream);
        }

    }

    #endregion
}
