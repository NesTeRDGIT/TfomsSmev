using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SmevAdapterService.VS
{
    public static class SeDeserializer<T>
    {
        public static string SerializeTo(T xmlObject)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            var memoryStream = new MemoryStream();
            var xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            xmlTextWriter.Formatting = Formatting.Indented;
            xmlSerializer.Serialize(xmlTextWriter, xmlObject);

            var output = Encoding.UTF8.GetString(memoryStream.ToArray());
            var _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
            if (output.StartsWith(_byteOrderMarkUtf8))
            {
                output = output.Remove(0, _byteOrderMarkUtf8.Length);
            }

            return output;
        }



        public static T DeserializeFromXmlElement(XmlElement xml) 
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            var stringReader = new StringReader(xml.OuterXml);
            var xmlObject = (T)xmlSerializer.Deserialize(stringReader);
            return xmlObject;
        }
        public static T DeserializeFromXDocument(XDocument xml) 
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            var stringReader = new StringReader(xml.ToString());//.ToXmlDocument().OuterXml);
            var xmlObject = (T)xmlSerializer.Deserialize(stringReader);
            return xmlObject;
        }
        public static T DeserializeFromXDocument(XElement xml)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            var stringReader = new StringReader(xml.ToString());//.ToXmlDocument().OuterXml);
            var xmlObject = (T)xmlSerializer.Deserialize(stringReader);
            return xmlObject;
        }

        public static string Namespace
        {
            get
            {
                var atrr = (XmlRootAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(XmlRootAttribute));
                return atrr?.Namespace;
            }
        }
        
    }

    public static class StaticSeDeserializer
    {
        public static XDocument SerializeToX<T>(this T xmlObject,XmlSerializerNamespaces ns = null)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            using (var memoryStream = new MemoryStream())
            {
                var xmlTextWriter = new XmlTextWriter(memoryStream, new UTF8Encoding(false))
                {
                    Formatting = Formatting.Indented
                };
                xmlSerializer.Serialize(xmlTextWriter, xmlObject, ns);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return XDocument.Load(memoryStream);
            }
        }
    }


}
