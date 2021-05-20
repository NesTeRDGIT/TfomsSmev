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
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            MemoryStream memoryStream = new MemoryStream();
            XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            xmlTextWriter.Formatting = Formatting.Indented;
            xmlSerializer.Serialize(xmlTextWriter, xmlObject);

            string output = Encoding.UTF8.GetString(memoryStream.ToArray());
            string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
            if (output.StartsWith(_byteOrderMarkUtf8))
            {
                output = output.Remove(0, _byteOrderMarkUtf8.Length);
            }

            return output;
        }



        public static T DeserializeFromXmlElement(XmlElement xml) 
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            StringReader stringReader = new StringReader(xml.OuterXml);
            var xmlObject = (T)xmlSerializer.Deserialize(stringReader);
            return xmlObject;
        }
        public static T DeserializeFromXDocument(XDocument xml) 
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            StringReader stringReader = new StringReader(xml.ToString());//.ToXmlDocument().OuterXml);
            var xmlObject = (T)xmlSerializer.Deserialize(stringReader);
            return xmlObject;
        }
        public static T DeserializeFromXDocument(XElement xml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            StringReader stringReader = new StringReader(xml.ToString());//.ToXmlDocument().OuterXml);
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
        public static string SerializeTo<T>(this T xmlObject)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            MemoryStream memoryStream = new MemoryStream();
            XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            xmlTextWriter.Formatting = Formatting.Indented;
            xmlSerializer.Serialize(xmlTextWriter, xmlObject);

            string output = Encoding.UTF8.GetString(memoryStream.ToArray());
            string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
            if (output.StartsWith(_byteOrderMarkUtf8))
            {
                output = output.Remove(0, _byteOrderMarkUtf8.Length);
            }

            return output;
        }
        public static XDocument SerializeToX<T>(this T xmlObject,XmlSerializerNamespaces ns = null)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            MemoryStream memoryStream = new MemoryStream(); 
            XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            xmlTextWriter.Formatting = Formatting.Indented;
            xmlSerializer.Serialize(xmlTextWriter, xmlObject, ns);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return XDocument.Load(memoryStream);
        }

        public static XElement SerializeToX<T>(this T xmlObject,  Type[] extraTypes)
        {
            var xmlSerializer = new XmlSerializer(typeof(T), extraTypes);
            var memoryStream = new MemoryStream();
            var xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            xmlTextWriter.Formatting = Formatting.Indented;
            var xsn = new XmlSerializerNamespaces();
            xsn.Add(string.Empty, string.Empty);
            xmlSerializer.Serialize(xmlTextWriter, xmlObject, xsn);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return XElement.Load(memoryStream);
        }

        public static XmlDocument SerializeToXML<T>(this T xmlObject)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            MemoryStream memoryStream = new MemoryStream();
            XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            xmlTextWriter.Formatting = Formatting.Indented;
            xmlSerializer.Serialize(xmlTextWriter, xmlObject);

            string output = Encoding.UTF8.GetString(memoryStream.ToArray());
            string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
            if (output.StartsWith(_byteOrderMarkUtf8))
            {
                output = output.Remove(0, _byteOrderMarkUtf8.Length);
            }
            var xml = new XmlDocument();
            xml.LoadXml(output);
            return xml;
        }
      
    }


}
