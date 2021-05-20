﻿using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using SmevAdapterService.VS;

namespace SMEV.VS.Zags.V4_0_0
{
    //https://smev3.gosuslugi.ru/portal/inquirytype_one.jsp?id=101176&zone=fed&page=1&dTest=false
    #region Запрос Установление отцовства

    [Serializable]
    [XmlRoot(ElementName = "Request", Namespace = "urn://x-artefacts-zags-parent/root/112-27/4.0.0", IsNullable = false)]
    public class Request_PARENTZP : IRequestMessage
    {
       
        [XmlElement(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/directive/1.3")]
        public Registry Registry { get; set; }

        public IResponseMessage Answer(string connectionString)
        {
            var e = Registry.RegistryRecord[0].Record.RecordContent.Attribute("ИдСвед").Value;
            return new PARENTZPResponse
            {
                ИдСвед = e
            };
        }
    }
    #endregion
    #region ОТВЕТ Установление отцовства

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-zags-parent/root/112-27/4.0.0")]
    [XmlRoot(Namespace = "urn://x-artefacts-zags-parent/root/112-27/4.0.0", IsNullable = false)]
    public class PARENTZPResponse : IResponseMessage
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces Xmlns = new XmlSerializerNamespaces(new[]{ new XmlQualifiedName( "","urn://x-artefacts-zags-parent/root/112-27/4.0.0") });
    
        [XmlAttribute]
        public string ИдСвед { get; set; }


        [XmlAttribute] 
        public string КодОбр { get; set; } = "1";

        XElement IResponseMessage.Serialize()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(PARENTZPResponse));
            MemoryStream memoryStream = new MemoryStream();
            XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            xmlTextWriter.Formatting = Formatting.Indented;
            xmlSerializer.Serialize(xmlTextWriter, this, Xmlns);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return XElement.Load(memoryStream);
        }
    }

    #endregion

}