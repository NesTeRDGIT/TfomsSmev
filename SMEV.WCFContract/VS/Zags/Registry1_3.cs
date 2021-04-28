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
        private RegistryRecord[] registryRecordField;

        [XmlElement("RegistryRecord")]
        public RegistryRecord[] RegistryRecord
        {
            get
            {
                return registryRecordField;
            }
            set
            {
                registryRecordField = value;
            }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/directive/1.3")]
    [XmlRoot(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/directive/1.3", IsNullable = false)]
    public class RegistryRecord
    {

        private int recordIdField;

        private Record recordField;

        private XmlElement recordSignatureField;

        public int RecordId
        {
            get
            {
                return recordIdField;
            }
            set
            {
                recordIdField = value;
            }
        }

        public Record Record
        {
            get
            {
                return recordField;
            }
            set
            {
                recordField = value;
            }
        }

        public XmlElement RecordSignature
        {
            get
            {
                return recordSignatureField;
            }
            set
            {
                recordSignatureField = value;
            }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/directive/1.3")]
    [XmlRoot(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/directive/1.3", IsNullable = false)]
    public class Record
    {

        private XElement recordContentField;

        private AttachmentHeaderType[] attachmentHeaderListField;

        private RefAttachmentHeaderType[] refAttachmentHeaderListField;

        private XmlElement[] personalSignatureField;

        private string idField;


        public XElement RecordContent
        {
            get
            {
                return recordContentField;
            }
            set
            {
                recordContentField = value;
            }
        }



        [XmlArray(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3")]
        [XmlArrayItem("AttachmentHeader", IsNullable = false)]
        public AttachmentHeaderType[] AttachmentHeaderList
        {
            get
            {
                return attachmentHeaderListField;
            }
            set
            {
                attachmentHeaderListField = value;
            }
        }


        [XmlArray(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3")]
        [XmlArrayItem("RefAttachmentHeader", IsNullable = false)]
        public RefAttachmentHeaderType[] RefAttachmentHeaderList
        {
            get
            {
                return refAttachmentHeaderListField;
            }
            set
            {
                refAttachmentHeaderListField = value;
            }
        }


        [XmlElement("PersonalSignature")]
        public XmlElement[] PersonalSignature
        {
            get
            {
                return personalSignatureField;
            }
            set
            {
                personalSignatureField = value;
            }
        }


        [XmlAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return idField;
            }
            set
            {
                idField = value;
            }
        }
    }


    #endregion

    #region Registry classes

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3")]
    public class RefAttachmentHeaderType
    {

        private string uuidField;

        private string fileNameField;

        private string namespaceUriField;

        private string hashField;

        private string mimeTypeField;

        private byte[] signaturePKCS7Field;

        private FileType[] archiveField;

        public string uuid
        {
            get
            {
                return uuidField;
            }
            set
            {
                uuidField = value;
            }
        }

        public string FileName
        {
            get
            {
                return fileNameField;
            }
            set
            {
                fileNameField = value;
            }
        }

        public string NamespaceUri
        {
            get
            {
                return namespaceUriField;
            }
            set
            {
                namespaceUriField = value;
            }
        }

        public string Hash
        {
            get
            {
                return hashField;
            }
            set
            {
                hashField = value;
            }
        }

        public string MimeType
        {
            get
            {
                return mimeTypeField;
            }
            set
            {
                mimeTypeField = value;
            }
        }

        [XmlElement(DataType = "base64Binary")]
        public byte[] SignaturePKCS7
        {
            get
            {
                return signaturePKCS7Field;
            }
            set
            {
                signaturePKCS7Field = value;
            }
        }


        [XmlArrayItem("File", IsNullable = false)]
        public FileType[] Archive
        {
            get
            {
                return archiveField;
            }
            set
            {
                archiveField = value;
            }
        }
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3")]
    public class FileType
    {
        private string nameField;
        private string namespaceUriField;

        public string Name
        {
            get
            {
                return nameField;
            }
            set
            {
                nameField = value;
            }
        }

        public string NamespaceUri
        {
            get
            {
                return namespaceUriField;
            }
            set
            {
                namespaceUriField = value;
            }
        }
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3")]
    public class AttachmentHeaderType
    {

        private string contentIdField;

        private string namespaceUriField;

        private string mimeTypeField;

        private byte[] signaturePKCS7Field;

        private FileType[] archiveField;

        public string contentId
        {
            get
            {
                return contentIdField;
            }
            set
            {
                contentIdField = value;
            }
        }

        public string NamespaceUri
        {
            get
            {
                return namespaceUriField;
            }
            set
            {
                namespaceUriField = value;
            }
        }

        public string MimeType
        {
            get
            {
                return mimeTypeField;
            }
            set
            {
                mimeTypeField = value;
            }
        }

        [XmlElement(DataType = "base64Binary")]
        public byte[] SignaturePKCS7
        {
            get
            {
                return signaturePKCS7Field;
            }
            set
            {
                signaturePKCS7Field = value;
            }
        }

        [XmlArrayItem("File", IsNullable = false)]
        public FileType[] Archive
        {
            get
            {
                return archiveField;
            }
            set
            {
                archiveField = value;
            }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3")]
    [XmlRoot(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3", IsNullable = false)]
    public class AttachmentHeaderList
    {

        private AttachmentHeaderType[] attachmentHeaderField;

        [XmlElement("AttachmentHeader")]
        public AttachmentHeaderType[] AttachmentHeader
        {
            get
            {
                return attachmentHeaderField;
            }
            set
            {
                attachmentHeaderField = value;
            }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3")]
    [XmlRoot(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3", IsNullable = false)]
    public class AttachmentContentList
    {

        private AttachmentContentType[] attachmentContentField;

        [XmlElement("AttachmentContent")]
        public AttachmentContentType[] AttachmentContent
        {
            get
            {
                return attachmentContentField;
            }
            set
            {
                attachmentContentField = value;
            }
        }
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3")]
    public class AttachmentContentType
    {

        private string idField;

        private byte[] contentField;

        [XmlElement(DataType = "ID")]
        public string Id
        {
            get
            {
                return idField;
            }
            set
            {
                idField = value;
            }
        }

        [XmlElement(DataType = "base64Binary")]
        public byte[] Content
        {
            get
            {
                return contentField;
            }
            set
            {
                contentField = value;
            }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3")]
    [XmlRoot(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3", IsNullable = false)]
    public class FSAttachmentsList
    {

        private FSAuthInfo[] fSAttachmentField;

        [XmlElement("FSAttachment")]
        public FSAuthInfo[] FSAttachment
        {
            get
            {
                return fSAttachmentField;
            }
            set
            {
                fSAttachmentField = value;
            }
        }
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3")]
    public class FSAuthInfo
    {

        private string uuidField;

        private string userNameField;

        private string passwordField;

        private string fileNameField;

        public string uuid
        {
            get
            {
                return uuidField;
            }
            set
            {
                uuidField = value;
            }
        }

        public string UserName
        {
            get
            {
                return userNameField;
            }
            set
            {
                userNameField = value;
            }
        }

        public string Password
        {
            get
            {
                return passwordField;
            }
            set
            {
                passwordField = value;
            }
        }

        public string FileName
        {
            get
            {
                return fileNameField;
            }
            set
            {
                fileNameField = value;
            }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3")]
    [XmlRoot(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3", IsNullable = false)]
    public class RefAttachmentHeaderList
    {

        private RefAttachmentHeaderType[] refAttachmentHeaderField;

        [XmlElement("RefAttachmentHeader")]
        public RefAttachmentHeaderType[] RefAttachmentHeader
        {
            get
            {
                return refAttachmentHeaderField;
            }
            set
            {
                refAttachmentHeaderField = value;
            }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3")]
    [XmlRoot(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3", IsNullable = false)]
    public class MessageReference
    {

        private string idField;

        private string valueField;

        [XmlAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return idField;
            }
            set
            {
                idField = value;
            }
        }

        [XmlText]
        public string Value
        {
            get
            {
                return valueField;
            }
            set
            {
                valueField = value;
            }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3")]
    [XmlRoot(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3", IsNullable = false)]
    public class AckTargetMessage
    {

        private string idField;

        private bool acceptedField;

        private bool acceptedFieldSpecified;

        private string valueField;

        [XmlAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return idField;
            }
            set
            {
                idField = value;
            }
        }

        [XmlAttribute]
        public bool accepted
        {
            get
            {
                return acceptedField;
            }
            set
            {
                acceptedField = value;
            }
        }

        [XmlIgnore]
        public bool acceptedSpecified
        {
            get
            {
                return acceptedFieldSpecified;
            }
            set
            {
                acceptedFieldSpecified = value;
            }
        }

        [XmlText]
        public string Value
        {
            get
            {
                return valueField;
            }
            set
            {
                valueField = value;
            }
        }
    }



    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3")]
    [XmlRoot(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3", IsNullable = false)]
    public class MessageTypeSelector
    {

        private string namespaceURIField;

        private string rootElementLocalNameField;

        private DateTime timestampField;

        private string nodeIDField;

        private string idField;


        [XmlElement(DataType = "anyURI")]
        public string NamespaceURI
        {
            get
            {
                return namespaceURIField;
            }
            set
            {
                namespaceURIField = value;
            }
        }


        [XmlElement(DataType = "NCName")]
        public string RootElementLocalName
        {
            get
            {
                return rootElementLocalNameField;
            }
            set
            {
                rootElementLocalNameField = value;
            }
        }


        public DateTime Timestamp
        {
            get
            {
                return timestampField;
            }
            set
            {
                timestampField = value;
            }
        }


        public string NodeID
        {
            get
            {
                return nodeIDField;
            }
            set
            {
                nodeIDField = value;
            }
        }


        [XmlAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return idField;
            }
            set
            {
                idField = value;
            }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3")]
    [XmlRoot(Namespace = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/basic/1.3", IsNullable = false)]
    public class Timestamp
    {

        private string idField;

        private DateTime valueField;


        [XmlAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return idField;
            }
            set
            {
                idField = value;
            }
        }


        [XmlText]
        public DateTime Value
        {
            get
            {
                return valueField;
            }
            set
            {
                valueField = value;
            }
        }
    }

    #endregion
}

