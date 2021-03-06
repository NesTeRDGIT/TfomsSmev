using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SmevAdapterService.VS.Zags
{

    [Serializable()]
    [XmlRoot(ElementName = "Request", Namespace = "urn://x-artefacts-zags-brakzrzp/root/112-26/4.0.0", IsNullable = false)]
    public partial class Request_BRAKZRZP : IRequestMessage
    {
        [XmlNamespaceDeclarations()]
        public static  XmlSerializerNamespaces XmlnsClass = new XmlSerializerNamespaces(new XmlQualifiedName[]
   { new XmlQualifiedName( "ns2","urn://x-artefacts-zags-brakzrzp/root/112-26/4.0.0"),
        
   });
        [XmlNamespaceDeclarations()]
        public XmlSerializerNamespaces Xmlns = new XmlSerializerNamespaces(new XmlQualifiedName[]
    { new XmlQualifiedName( "ns2","urn://x-artefacts-zags-brakzrzp/root/112-26/4.0.0"),
      new XmlQualifiedName( "ns4","urn://x-artefacts-smev-gov-ru/services/message-exchange/types/directive/1.3"),
      new XmlQualifiedName( "ns3","urn://x-artefacts-zags-brakzrzp/types/4.0.0"),
       new XmlQualifiedName( "ds","http://www.w3.org/2000/09/xmldsig#"),
       new XmlQualifiedName( "S","http://schemas.xmlsoap.org/soap/envelope/"),
       new XmlQualifiedName( "SOAP-ENV","http://schemas.xmlsoap.org/soap/envelope/"),
       new XmlQualifiedName( "ns0","urn://x-artefacts-smev-gov-ru/services/message-exchange/types/1.3"),
       new XmlQualifiedName( "ns1","urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3")

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
            // var e = req.Registry.RegistryRecord[0].Record.RecordContent.BRAKZRZPRequest.ИдСвед;
            var e = Registry.RegistryRecord[0].Record.RecordContent.Attribute("ИдСвед").Value;
            return new BRAKZRZPResponse()
            {
                ИдСвед = e,
                КодОбр = BRAKZRZPResponseКодОбр.Item1
            };
        }

/*
        public XElement Serialize()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Request_BRAKZRZP));
            MemoryStream memoryStream = new MemoryStream();
            XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            xmlTextWriter.Formatting = Formatting.Indented;            
            xmlSerializer.Serialize(xmlTextWriter, this, this.Xmlns);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return XElement.Load(memoryStream);
        }*/
   
    }

    #region ОТВЕТ Регистрация брака
    [Serializable()]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-zags-brakzrzp/root/112-26/4.0.0")]
    [XmlRoot(Namespace = "urn://x-artefacts-zags-brakzrzp/root/112-26/4.0.0", IsNullable = false)]
    public partial class BRAKZRZPResponse : IResponseMessage
    {
        [XmlNamespaceDeclarations()]
        public XmlSerializerNamespaces Xmlns = new XmlSerializerNamespaces(new XmlQualifiedName[]
       { new XmlQualifiedName( "","urn://x-artefacts-zags-brakzrzp/root/112-26/4.0.0") });

        private string идСведField;

        private BRAKZRZPResponseКодОбр кодОбрField;


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
        public BRAKZRZPResponseКодОбр КодОбр
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


        public XElement Serialize()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(BRAKZRZPResponse));
            MemoryStream memoryStream = new MemoryStream();
            XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            xmlTextWriter.Formatting = Formatting.Indented;
            xmlSerializer.Serialize(xmlTextWriter, this, this.Xmlns);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return XElement.Load(memoryStream);
        }

    }

    [Serializable()]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-zags-brakzrzp/root/112-26/4.0.0")]
    public enum BRAKZRZPResponseКодОбр
    {
        [System.Xml.Serialization.XmlEnumAttribute("1")]
        Item1,
    }
    #endregion
}
