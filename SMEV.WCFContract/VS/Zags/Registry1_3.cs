using System;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SMEV.VS.Zags
{
    #region Общий класс для сведений ЗАГСА

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/directive/1.3")]
    [XmlRoot(ElementName = "Registry", Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/directive/1.3", IsNullable = false)]
    public class Registry
    {
        [XmlElement("RegistryRecord")]
        public RegistryRecord[] RegistryRecord { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/directive/1.3")]
    [XmlRoot(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/directive/1.3", IsNullable = false)]
    public class RegistryRecord
    {
        public int RecordId { get; set; }

        public Record Record { get; set; }

        public XmlElement RecordSignature { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/directive/1.3")]
    [XmlRoot(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/directive/1.3", IsNullable = false)]
    public class Record
    {
        public XElement RecordContent { get; set; }



        [XmlArray(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3")]
        [XmlArrayItem("AttachmentHeader", IsNullable = false)]
        public AttachmentHeaderType[] AttachmentHeaderList { get; set; }


        [XmlArray(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3")]
        [XmlArrayItem("RefAttachmentHeader", IsNullable = false)]
        public RefAttachmentHeaderType[] RefAttachmentHeaderList { get; set; }


        [XmlElement("PersonalSignature")]
        public XmlElement[] PersonalSignature { get; set; }


        [XmlAttribute(DataType = "ID")]
        public string Id { get; set; }
    }


    #endregion

    #region Registry classes

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3")]
    public class RefAttachmentHeaderType
    {
        public string uuid { get; set; }

        public string FileName { get; set; }

        public string NamespaceUri { get; set; }

        public string Hash { get; set; }

        public string MimeType { get; set; }

        [XmlElement(DataType = "base64Binary")]
        public byte[] SignaturePKCS7 { get; set; }


        [XmlArrayItem("File", IsNullable = false)]
        public FileType[] Archive { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3")]
    public class FileType
    {
        public string Name { get; set; }

        public string NamespaceUri { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3")]
    public class AttachmentHeaderType
    {
        public string contentId { get; set; }

        public string NamespaceUri { get; set; }

        public string MimeType { get; set; }

        [XmlElement(DataType = "base64Binary")]
        public byte[] SignaturePKCS7 { get; set; }

        [XmlArrayItem("File", IsNullable = false)]
        public FileType[] Archive { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3")]
    [XmlRoot(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3", IsNullable = false)]
    public class AttachmentHeaderList
    {
        [XmlElement("AttachmentHeader")]
        public AttachmentHeaderType[] AttachmentHeader { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3")]
    [XmlRoot(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3", IsNullable = false)]
    public class AttachmentContentList
    {
        [XmlElement("AttachmentContent")]
        public AttachmentContentType[] AttachmentContent { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3")]
    public class AttachmentContentType
    {
        [XmlElement(DataType = "ID")]
        public string Id { get; set; }

        [XmlElement(DataType = "base64Binary")]
        public byte[] Content { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3")]
    [XmlRoot(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3", IsNullable = false)]
    public class FSAttachmentsList
    {
        [XmlElement("FSAttachment")]
        public FSAuthInfo[] FSAttachment { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3")]
    public class FSAuthInfo
    {
        public string uuid { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string FileName { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3")]
    [XmlRoot(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3", IsNullable = false)]
    public class RefAttachmentHeaderList
    {
        [XmlElement("RefAttachmentHeader")]
        public RefAttachmentHeaderType[] RefAttachmentHeader { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3")]
    [XmlRoot(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3", IsNullable = false)]
    public class MessageReference
    {
        [XmlAttribute(DataType = "ID")]
        public string Id { get; set; }

        [XmlText]
        public string Value { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3")]
    [XmlRoot(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3", IsNullable = false)]
    public class AckTargetMessage
    {
        [XmlAttribute(DataType = "ID")]
        public string Id { get; set; }

        [XmlAttribute]
        public bool accepted { get; set; }

        [XmlIgnore]
        public bool acceptedSpecified { get; set; }

        [XmlText]
        public string Value { get; set; }
    }



    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3")]
    [XmlRoot(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3", IsNullable = false)]
    public class MessageTypeSelector
    {
        [XmlElement(DataType = "anyURI")]
        public string NamespaceURI { get; set; }


        [XmlElement(DataType = "NCName")]
        public string RootElementLocalName { get; set; }


        public DateTime Timestamp { get; set; }


        public string NodeID { get; set; }


        [XmlAttribute(DataType = "ID")]
        public string Id { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3")]
    [XmlRoot(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3", IsNullable = false)]
    public class Timestamp
    {
        [XmlAttribute(DataType = "ID")]
        public string Id { get; set; }


        [XmlText]
        public DateTime Value { get; set; }
    }

    #endregion
}

