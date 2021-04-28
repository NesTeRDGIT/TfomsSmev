﻿using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using SMEV.VS.Zags;
using SmevAdapterService.VS;

namespace SMEV.VS.Zags4_0_1
{
    /// <summary>
    /// https://smev3.gosuslugi.ru/portal/inquirytype_one.jsp?id=140627&zone=fed&page=1&dTest=false
    /// </summary>
    #region ЗАПРОС Регистрация рождения


    [Serializable]
    [XmlRoot(ElementName = "Request", Namespace = "urn://x-artefacts-zags-rogdzp/root/112-23/4.0.1", IsNullable = false)]
    public class Request_ROGDZP : IRequestMessage
    {
        [XmlNamespaceDeclarations]
        public static XmlSerializerNamespaces XmlnsClass = new XmlSerializerNamespaces(new[]
{ new XmlQualifiedName( "ns2","urn://x-artefacts-zags-rogdzp/root/112-23/4.0.1")

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
            return new ROGDZPResponse
            {
                ИдСвед = e,
                КодОбр = ROGDZPResponseКодОбр.Item1
            };
        }

    }
    #endregion

    #region ОТВЕТ Регистрация рождения

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-zags-rogdzp/root/112-23/4.0.1")]
    [XmlRoot(Namespace = "urn://x-artefacts-zags-rogdzp/root/112-23/4.0.1", IsNullable = false)]
    public class ROGDZPResponse : IResponseMessage
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces Xmlns = new XmlSerializerNamespaces(new[]
     { new XmlQualifiedName( "","urn://x-artefacts-zags-rogdzp/root/112-23/4.0.1") });

        private string идСведField;

        private ROGDZPResponseКодОбр кодОбрField;


        [XmlAttribute]
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


        [XmlAttribute]
        public ROGDZPResponseКодОбр КодОбр
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
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ROGDZPResponse));
            MemoryStream memoryStream = new MemoryStream();
            XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            xmlTextWriter.Formatting = Formatting.Indented;
            xmlSerializer.Serialize(xmlTextWriter, this, Xmlns);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return XElement.Load(memoryStream);
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-zags-rogdzp/root/112-23/4.0.1")]
    public enum ROGDZPResponseКодОбр
    {

        [XmlEnum("1")]
        Item1
    }

    #endregion
}
