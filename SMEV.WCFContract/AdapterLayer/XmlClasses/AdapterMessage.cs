using System;
using System.ComponentModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SMEV.AdapterLayer.XmlClasses
{
    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types/faults")]
    public class SystemFault : Fault
    {
    }

    [XmlInclude(typeof(ValidationFault))]
    [XmlInclude(typeof(SystemFault))]
    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types/faults")]
    public class Fault : object, INotifyPropertyChanged
    {
        private string codeField;
        private string descriptionField;

        [XmlElement(Order = 0)]
        public string code
        {
            get
            {
                return codeField;
            }
            set
            {
                codeField = value;
                RaisePropertyChanged("code");
            }
        }

        [XmlElement(Order = 1)]
        public string description
        {
            get
            {
                return descriptionField;
            }
            set
            {
                descriptionField = value;
                RaisePropertyChanged("description");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    public class Status : object, INotifyPropertyChanged
    {

        private string codeField;

        private string descriptionField;

        private StatusParameter[] parameterField;

        [XmlElement(Order = 0)]
        public string code
        {
            get
            {
                return codeField;
            }
            set
            {
                codeField = value;
                RaisePropertyChanged("code");
            }
        }

        [XmlElement(Order = 1)]
        public string description
        {
            get
            {
                return descriptionField;
            }
            set
            {
                descriptionField = value;
                RaisePropertyChanged("description");
            }
        }

        [XmlElement("parameter", IsNullable = true, Order = 2)]
        public StatusParameter[] parameter
        {
            get
            {
                return parameterField;
            }
            set
            {
                parameterField = value;
                RaisePropertyChanged("parameter");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    public class StatusParameter : object, INotifyPropertyChanged
    {
        private string keyField;
        private string valueField;

        [XmlElement(Order = 0)]
        public string key
        {
            get
            {
                return keyField;
            }
            set
            {
                keyField = value;
                RaisePropertyChanged("key");
            }
        }

        [XmlElement(Order = 1)]
        public string value
        {
            get
            {
                return valueField;
            }
            set
            {
                valueField = value;
                RaisePropertyChanged("value");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    public class Reject : object, INotifyPropertyChanged
    {
        private RejectCode codeField;
        private string descriptionField;

        [XmlElement(Order = 0)]
        public RejectCode code
        {
            get
            {
                return codeField;
            }
            set
            {
                codeField = value;
                RaisePropertyChanged("code");
            }
        }

        [XmlElement(Order = 1)]
        public string description
        {
            get
            {
                return descriptionField;
            }
            set
            {
                descriptionField = value;
                RaisePropertyChanged("description");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    public enum RejectCode
    {
        ACCESS_DENIED,
        NO_DATA,
        UNKNOWN_REQUEST_DESCRIPTION,
        FAILURE
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    public class ResponseContentType : object, INotifyPropertyChanged
    {

        private Content contentField;

        private Reject[] rejectsField;

        private Status statusField;

        [XmlElement(Order = 0)]
        public Content content
        {
            get
            {
                return contentField;
            }
            set
            {
                contentField = value;
                RaisePropertyChanged("content");
            }
        }


        [XmlElement("rejects", IsNullable = true, Order = 1)]
        public Reject[] rejects
        {
            get
            {
                return rejectsField;
            }
            set
            {
                rejectsField = value;
                RaisePropertyChanged("rejects");
            }
        }


        [XmlElement(Order = 2)]
        public Status status
        {
            get
            {
                return statusField;
            }
            set
            {
                statusField = value;
                RaisePropertyChanged("status");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    public class Content : object, INotifyPropertyChanged
    {

        private XElement messagePrimaryContentField;

        private XmlElement personalSignatureField;

        private AttachmentHeaderType[] attachmentHeaderListField;


        [XmlElement(Order = 0)]
        public XElement MessagePrimaryContent
        {
            get
            {
                return messagePrimaryContentField;
            }
            set
            {
                messagePrimaryContentField = value;
                RaisePropertyChanged("MessagePrimaryContent");
            }
        }


        [XmlElement(Order = 1)]
        public XmlElement PersonalSignature
        {
            get
            {
                return personalSignatureField;
            }
            set
            {
                personalSignatureField = value;
                RaisePropertyChanged("PersonalSignature");
            }
        }


        [XmlArray(Order = 2)]
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
                RaisePropertyChanged("AttachmentHeaderList");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    public class AttachmentHeaderType : object, INotifyPropertyChanged
    {

        private string idField;

        private string filePathField;

        private byte[] signaturePKCS7Field;

        private TransferMethodType transferMethodField;

        private bool transferMethodFieldSpecified;


        [XmlElement(Order = 0)]
        public string Id
        {
            get
            {
                return idField;
            }
            set
            {
                idField = value;
                RaisePropertyChanged("Id");
            }
        }


        [XmlElement(Order = 1)]
        public string filePath
        {
            get
            {
                return filePathField;
            }
            set
            {
                filePathField = value;
                RaisePropertyChanged("filePath");
            }
        }


        [XmlElement(DataType = "base64Binary", Order = 2)]
        public byte[] SignaturePKCS7
        {
            get
            {
                return signaturePKCS7Field;
            }
            set
            {
                signaturePKCS7Field = value;
                RaisePropertyChanged("SignaturePKCS7");
            }
        }


        [XmlElement(Order = 3)]
        public TransferMethodType TransferMethod
        {
            get
            {
                return transferMethodField;
            }
            set
            {
                transferMethodField = value;
                RaisePropertyChanged("TransferMethod");
            }
        }


        [XmlIgnore]
        public bool TransferMethodSpecified
        {
            get
            {
                return transferMethodFieldSpecified;
            }
            set
            {
                transferMethodFieldSpecified = value;
                RaisePropertyChanged("TransferMethodSpecified");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    public enum TransferMethodType
    {
        MTOM,
        REFERENCE
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    public class RequestContentType : object, INotifyPropertyChanged
    {

        private Content contentField;

        [XmlElement(Order = 0)]
        public Content content
        {
            get
            {
                return contentField;
            }
            set
            {
                contentField = value;
                RaisePropertyChanged("content");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    public class RegistryRecordRoutingType : object, INotifyPropertyChanged
    {

        private int recordIdField;

        private bool useGeneralRoutingField;

        private string[] dynamicRoutingField;

        private string[] identifierRoutingField;

        [XmlElement(Order = 0)]
        public int RecordId
        {
            get
            {
                return recordIdField;
            }
            set
            {
                recordIdField = value;
                RaisePropertyChanged("RecordId");
            }
        }

        [XmlElement(Order = 1)]
        public bool UseGeneralRouting
        {
            get
            {
                return useGeneralRoutingField;
            }
            set
            {
                useGeneralRoutingField = value;
                RaisePropertyChanged("UseGeneralRouting");
            }
        }

        [XmlArray(Order = 2)]
        [XmlArrayItem("DynamicValue", IsNullable = false)]
        public string[] DynamicRouting
        {
            get
            {
                return dynamicRoutingField;
            }
            set
            {
                dynamicRoutingField = value;
                RaisePropertyChanged("DynamicRouting");
            }
        }

        [XmlArray(Order = 3)]
        [XmlArrayItem("IdentifierValue", IsNullable = false)]
        public string[] IdentifierRouting
        {
            get
            {
                return identifierRoutingField;
            }
            set
            {
                identifierRoutingField = value;
                RaisePropertyChanged("IdentifierRouting");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }



    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    public class RoutingInformationType : object, INotifyPropertyChanged
    {

        private string[] dynamicRoutingField;

        private string[] identifierRoutingField;

        private RegistryRecordRoutingType[] registryRoutingField;


        [XmlArray(Order = 0)]
        [XmlArrayItem("DynamicValue", IsNullable = false)]
        public string[] DynamicRouting
        {
            get
            {
                return dynamicRoutingField;
            }
            set
            {
                dynamicRoutingField = value;
                RaisePropertyChanged("DynamicRouting");
            }
        }


        [XmlArray(Order = 1)]
        [XmlArrayItem("IdentifierValue", IsNullable = false)]
        public string[] IdentifierRouting
        {
            get
            {
                return identifierRoutingField;
            }
            set
            {
                identifierRoutingField = value;
                RaisePropertyChanged("IdentifierRouting");
            }
        }


        [XmlArray(Order = 2)]
        [XmlArrayItem("RegistryRecordRouting", IsNullable = false)]
        public RegistryRecordRoutingType[] RegistryRouting
        {
            get
            {
                return registryRoutingField;
            }
            set
            {
                registryRoutingField = value;
                RaisePropertyChanged("RegistryRouting");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    public class BusinessProcessMetadata : object, INotifyPropertyChanged
    {

        private XmlElement[] anyField;


        [XmlAnyElement(Order = 0)]
        public XmlElement[] Any
        {
            get
            {
                return anyField;
            }
            set
            {
                anyField = value;
                RaisePropertyChanged("Any");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    public class CreateGroupIdentity : object, INotifyPropertyChanged
    {

        private string fRGUServiceCodeField;

        private string fRGUServiceDescriptionField;

        private string fRGUServiceRecipientDescriptionField;


        [XmlElement(Order = 0)]
        public string FRGUServiceCode
        {
            get
            {
                return fRGUServiceCodeField;
            }
            set
            {
                fRGUServiceCodeField = value;
                RaisePropertyChanged("FRGUServiceCode");
            }
        }


        [XmlElement(Order = 1)]
        public string FRGUServiceDescription
        {
            get
            {
                return fRGUServiceDescriptionField;
            }
            set
            {
                fRGUServiceDescriptionField = value;
                RaisePropertyChanged("FRGUServiceDescription");
            }
        }


        [XmlElement(Order = 2)]
        public string FRGUServiceRecipientDescription
        {
            get
            {
                return fRGUServiceRecipientDescriptionField;
            }
            set
            {
                fRGUServiceRecipientDescriptionField = value;
                RaisePropertyChanged("FRGUServiceRecipientDescription");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    public class LinkedGroupIdentity : object, INotifyPropertyChanged
    {

        private string refClientIdField;

        private string refGroupIdField;


        [XmlElement(Order = 0)]
        public string refClientId
        {
            get
            {
                return refClientIdField;
            }
            set
            {
                refClientIdField = value;
                RaisePropertyChanged("refClientId");
            }
        }


        [XmlElement(Order = 1)]
        public string refGroupId
        {
            get
            {
                return refGroupIdField;
            }
            set
            {
                refGroupIdField = value;
                RaisePropertyChanged("refGroupId");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }


    [XmlInclude(typeof(ResponseMetadataType))]
    [XmlInclude(typeof(StatusMetadataType))]
    [XmlInclude(typeof(RequestMetadataType))]
    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    public class Metadata : object, INotifyPropertyChanged
    {

        private string clientIdField;


        [XmlElement(Order = 0)]
        public string clientId
        {
            get
            {
                return clientIdField;
            }
            set
            {
                clientIdField = value;
                RaisePropertyChanged("clientId");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }


    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    public class ResponseMetadataType : Metadata
    {

        private string replyToClientIdField;


        [XmlElement(Order = 0)]
        public string replyToClientId
        {
            get
            {
                return replyToClientIdField;
            }
            set
            {
                replyToClientIdField = value;
                RaisePropertyChanged("replyToClientId");
            }
        }
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    public class StatusMetadataType : Metadata
    {

        private string originalClientIdField;


        [XmlElement(Order = 0)]
        public string originalClientId
        {
            get
            {
                return originalClientIdField;
            }
            set
            {
                originalClientIdField = value;
                RaisePropertyChanged("originalClientId");
            }
        }
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    public class RequestMetadataType : Metadata
    {

        private LinkedGroupIdentity linkedGroupIdentityField;

        private CreateGroupIdentity createGroupIdentityField;

        private string nodeIdField;

        private DateTime eolField;

        private bool eolFieldSpecified;

        private bool testMessageField;

        private bool testMessageFieldSpecified;

        private string transactionCodeField;

        private BusinessProcessMetadata businessProcessMetadataField;

        private RoutingInformationType routingInformationField;


        [XmlElement(Order = 0)]
        public LinkedGroupIdentity linkedGroupIdentity
        {
            get
            {
                return linkedGroupIdentityField;
            }
            set
            {
                linkedGroupIdentityField = value;
                RaisePropertyChanged("linkedGroupIdentity");
            }
        }


        [XmlElement(Order = 1)]
        public CreateGroupIdentity createGroupIdentity
        {
            get
            {
                return createGroupIdentityField;
            }
            set
            {
                createGroupIdentityField = value;
                RaisePropertyChanged("createGroupIdentity");
            }
        }


        [XmlElement(Order = 2)]
        public string nodeId
        {
            get
            {
                return nodeIdField;
            }
            set
            {
                nodeIdField = value;
                RaisePropertyChanged("nodeId");
            }
        }


        [XmlElement(Order = 3)]
        public DateTime eol
        {
            get
            {
                return eolField;
            }
            set
            {
                eolField = value;
                RaisePropertyChanged("eol");
            }
        }


        [XmlIgnore]
        public bool eolSpecified
        {
            get
            {
                return eolFieldSpecified;
            }
            set
            {
                eolFieldSpecified = value;
                RaisePropertyChanged("eolSpecified");
            }
        }


        [XmlElement(Order = 4)]
        public bool testMessage
        {
            get
            {
                return testMessageField;
            }
            set
            {
                testMessageField = value;
                RaisePropertyChanged("testMessage");
            }
        }


        [XmlIgnore]
        public bool testMessageSpecified
        {
            get
            {
                return testMessageFieldSpecified;
            }
            set
            {
                testMessageFieldSpecified = value;
                RaisePropertyChanged("testMessageSpecified");
            }
        }


        [XmlElement(Order = 5)]
        public string TransactionCode
        {
            get
            {
                return transactionCodeField;
            }
            set
            {
                transactionCodeField = value;
                RaisePropertyChanged("TransactionCode");
            }
        }


        [XmlElement(Order = 6)]
        public BusinessProcessMetadata BusinessProcessMetadata
        {
            get
            {
                return businessProcessMetadataField;
            }
            set
            {
                businessProcessMetadataField = value;
                RaisePropertyChanged("BusinessProcessMetadata");
            }
        }


        [XmlElement(Order = 7)]
        public RoutingInformationType RoutingInformation
        {
            get
            {
                return routingInformationField;
            }
            set
            {
                routingInformationField = value;
                RaisePropertyChanged("RoutingInformation");
            }
        }
    }


    [XmlInclude(typeof(ResponseMessageType))]
    [XmlInclude(typeof(StatusMessage))]
    [XmlInclude(typeof(ErrorMessage))]
    [XmlInclude(typeof(RequestMessageType))]
    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    public class Message : object, INotifyPropertyChanged
    {

        private string messageTypeField;


        [XmlElement(Order = 0)]
        public string messageType
        {
            get
            {
                return messageTypeField;
            }
            set
            {
                messageTypeField = value;
                RaisePropertyChanged("messageType");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    public class ResponseMessageType : Message
    {

        private ResponseMetadataType responseMetadataField;

        private ResponseContentType responseContentField;


        [XmlElement(Order = 0)]
        public ResponseMetadataType ResponseMetadata
        {
            get
            {
                return responseMetadataField;
            }
            set
            {
                responseMetadataField = value;
                RaisePropertyChanged("ResponseMetadata");
            }
        }


        [XmlElement(Order = 1)]
        public ResponseContentType ResponseContent
        {
            get
            {
                return responseContentField;
            }
            set
            {
                responseContentField = value;
                RaisePropertyChanged("ResponseContent");
            }
        }
    }


    [XmlInclude(typeof(ErrorMessage))]
    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    public class StatusMessage : Message
    {

        private StatusMetadataType statusMetadataField;

        private StatusMessageCategory statusField;

        private string detailsField;

        private DateTime timestampField;

        private bool timestampFieldSpecified;


        [XmlElement(Order = 0)]
        public StatusMetadataType statusMetadata
        {
            get
            {
                return statusMetadataField;
            }
            set
            {
                statusMetadataField = value;
                RaisePropertyChanged("statusMetadata");
            }
        }


        [XmlElement(Order = 1)]
        public StatusMessageCategory status
        {
            get
            {
                return statusField;
            }
            set
            {
                statusField = value;
                RaisePropertyChanged("status");
            }
        }


        [XmlElement(Order = 2)]
        public string details
        {
            get
            {
                return detailsField;
            }
            set
            {
                detailsField = value;
                RaisePropertyChanged("details");
            }
        }


        [XmlElement(Order = 3)]
        public DateTime timestamp
        {
            get
            {
                return timestampField;
            }
            set
            {
                timestampField = value;
                RaisePropertyChanged("timestamp");
            }
        }


        [XmlIgnore]
        public bool timestampSpecified
        {
            get
            {
                return timestampFieldSpecified;
            }
            set
            {
                timestampFieldSpecified = value;
                RaisePropertyChanged("timestampSpecified");
            }
        }
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    public enum StatusMessageCategory
    {
        OTHER,
        ERROR
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    public class ErrorMessage : StatusMessage
    {

        private ErrorType typeField;

        private Fault faultField;


        [XmlElement(Order = 0)]
        public ErrorType type
        {
            get
            {
                return typeField;
            }
            set
            {
                typeField = value;
                RaisePropertyChanged("type");
            }
        }


        [XmlElement(Order = 1)]
        public Fault fault
        {
            get
            {
                return faultField;
            }
            set
            {
                faultField = value;
                RaisePropertyChanged("fault");
            }
        }
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    public enum ErrorType
    {
        SERVER,
        CLIENT
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    public class RequestMessageType : Message
    {

        private RequestMetadataType requestMetadataField;

        private RequestContentType requestContentField;


        [XmlElement(Order = 0)]
        public RequestMetadataType RequestMetadata
        {
            get
            {
                return requestMetadataField;
            }
            set
            {
                requestMetadataField = value;
                RaisePropertyChanged("RequestMetadata");
            }
        }


        [XmlElement(Order = 1)]
        public RequestContentType RequestContent
        {
            get
            {
                return requestContentField;
            }
            set
            {
                requestContentField = value;
                RaisePropertyChanged("RequestContent");
            }
        }
    }


    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    public class SmevMetadata : object, INotifyPropertyChanged
    {

        private string messageIdField;

        private string referenceMessageIDField;

        private string transactionCodeField;

        private string originalMessageIDField;


        [XmlElement(Order = 0)]
        public string MessageId
        {
            get
            {
                return messageIdField;
            }
            set
            {
                messageIdField = value;
                RaisePropertyChanged("MessageId");
            }
        }
        [XmlElement(Order = 1)]
        public string ReferenceMessageID
        {
            get
            {
                return referenceMessageIDField;
            }
            set
            {
                referenceMessageIDField = value;
                RaisePropertyChanged("ReferenceMessageID");
            }
        }
        [XmlElement(Order = 2)]
        public string TransactionCode
        {
            get
            {
                return transactionCodeField;
            }
            set
            {
                transactionCodeField = value;
                RaisePropertyChanged("TransactionCode");
            }
        }
        [XmlElement(Order = 3)]
        public string OriginalMessageID
        {
            get
            {
                return originalMessageIDField;
            }
            set
            {
                originalMessageIDField = value;
                RaisePropertyChanged("OriginalMessageID");
            }
        }
        [XmlElement(Order = 4)]
        public string Sender { get; set; }
        [XmlElement(Order = 5)]
        public string Recipient { get; set; }
        

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    public class QueryTypeCriteria : object, INotifyPropertyChanged
    {

        private TypeCriteria messageTypeCriteriaField;


        [XmlElement(Order = 0)]
        public TypeCriteria messageTypeCriteria
        {
            get
            {
                return messageTypeCriteriaField;
            }
            set
            {
                messageTypeCriteriaField = value;
                RaisePropertyChanged("messageTypeCriteria");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    public enum TypeCriteria
    {
        RESPONSE,
        REQUEST
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types/faults")]
    public class ValidationFault : Fault
    {
    }

    [ServiceContract(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter", ConfigurationName = "AdapterService.SMEVServiceAdapterPortType")]
    public interface SMEVServiceAdapterPortType
    {

        [OperationContract(Action = "urn:Get", ReplyAction = "urn://x-artefacts-smev-gov-ru/services/service-adapter:SMEVServiceAdapterPortType" +
            ":GetResponse")]
        [FaultContract(typeof(SystemFault), Action = "urn://x-artefacts-smev-gov-ru/services/service-adapter:SMEVServiceAdapterPortType" +
            ":Get:Fault:SystemFault", Name = "SystemFault", Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types/faults")]
        [FaultContract(typeof(ValidationFault), Action = "urn://x-artefacts-smev-gov-ru/services/service-adapter:SMEVServiceAdapterPortType" +
            ":Get:Fault:ValidationFault", Name = "ValidationFault", Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types/faults")]
        [XmlSerializerFormat(SupportFaults = true)]
        [ServiceKnownType(typeof(Metadata))]
        GetResponse Get(GetRequest request);

        // CODEGEN: Идет формирование контракта на сообщение, так как операция может иметь много возвращаемых значений.
        [OperationContract(Action = "urn:Get", ReplyAction = "urn://x-artefacts-smev-gov-ru/services/service-adapter:SMEVServiceAdapterPortType" +
            ":GetResponse")]
        Task<GetResponse> GetAsync(GetRequest request);

        // CODEGEN: Контракт генерации сообщений с пространством имен упаковщика (urn://x-artefacts-smev-gov-ru/services/service-adapter/types) сообщения FindRequest не соответствует значению по умолчанию (urn://x-artefacts-smev-gov-ru/services/service-adapter).
        [OperationContract(Action = "urn:Find", ReplyAction = "urn://x-artefacts-smev-gov-ru/services/service-adapter:SMEVServiceAdapterPortType" +
            ":FindResponse")]
        [FaultContract(typeof(SystemFault), Action = "urn://x-artefacts-smev-gov-ru/services/service-adapter:SMEVServiceAdapterPortType" +
            ":Find:Fault:SystemFault", Name = "SystemFault", Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types/faults")]
        [FaultContract(typeof(ValidationFault), Action = "urn://x-artefacts-smev-gov-ru/services/service-adapter:SMEVServiceAdapterPortType" +
            ":Find:Fault:ValidationFault", Name = "ValidationFault", Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types/faults")]
        [XmlSerializerFormat(SupportFaults = true)]
        [ServiceKnownType(typeof(Metadata))]
        FindResponse Find(FindRequest request);

        [OperationContract(Action = "urn:Find", ReplyAction = "urn://x-artefacts-smev-gov-ru/services/service-adapter:SMEVServiceAdapterPortType" +
            ":FindResponse")]
        Task<FindResponse> FindAsync(FindRequest request);

        [OperationContract(Action = "urn:Send", ReplyAction = "urn://x-artefacts-smev-gov-ru/services/service-adapter:SMEVServiceAdapterPortType" +
            ":SendResponse")]
        [FaultContract(typeof(SystemFault), Action = "urn://x-artefacts-smev-gov-ru/services/service-adapter:SMEVServiceAdapterPortType" +
            ":Send:Fault:SystemFault", Name = "SystemFault", Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types/faults")]
        [FaultContract(typeof(ValidationFault), Action = "urn://x-artefacts-smev-gov-ru/services/service-adapter:SMEVServiceAdapterPortType" +
            ":Send:Fault:ValidationFault", Name = "ValidationFault", Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types/faults")]
        [XmlSerializerFormat(SupportFaults = true)]
        [ServiceKnownType(typeof(Metadata))]
        SendResponse Send(SendRequest request);

        // CODEGEN: Идет формирование контракта на сообщение, так как операция может иметь много возвращаемых значений.
        [OperationContract(Action = "urn:Send", ReplyAction = "urn://x-artefacts-smev-gov-ru/services/service-adapter:SMEVServiceAdapterPortType" +
            ":SendResponse")]
        Task<SendResponse> SendAsync(SendRequest request);
    }

    [MessageContract(WrapperName = "MessageQuery", WrapperNamespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types", IsWrapped = true)]
    public class GetRequest
    {

        [MessageBodyMember(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types", Order = 0)]
        public string itSystem;

        [MessageBodyMember(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types", Order = 1)]
        public string nodeId;

        [MessageBodyMember(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types", Order = 2)]
        public QueryTypeCriteria specificQuery;

        public GetRequest()
        {
        }

        public GetRequest(string itSystem, string nodeId, QueryTypeCriteria specificQuery)
        {
            this.itSystem = itSystem;
            this.nodeId = nodeId;
            this.specificQuery = specificQuery;
        }
    }

    [MessageContract(WrapperName = "QueryResult", WrapperNamespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types", IsWrapped = true)]
    public class GetResponse
    {

        [MessageBodyMember(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types", Order = 0)]
        public SmevMetadata smevMetadata;

        [MessageBodyMember(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types", Order = 1)]
        public Message Message;

        public GetResponse()
        {
        }

        public GetResponse(SmevMetadata smevMetadata, Message Message)
        {
            this.smevMetadata = smevMetadata;
            this.Message = Message;
        }
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    public class FindTypeCriteria : object, INotifyPropertyChanged
    {

        private MessageIntervalCriteria messagePeriodCriteriaField;

        private MessageClientIdCriteria messageClientIdCriteriaField;


        [XmlElement(Order = 0)]
        public MessageIntervalCriteria messagePeriodCriteria
        {
            get
            {
                return messagePeriodCriteriaField;
            }
            set
            {
                messagePeriodCriteriaField = value;
                RaisePropertyChanged("messagePeriodCriteria");
            }
        }


        [XmlElement(Order = 1)]
        public MessageClientIdCriteria messageClientIdCriteria
        {
            get
            {
                return messageClientIdCriteriaField;
            }
            set
            {
                messageClientIdCriteriaField = value;
                RaisePropertyChanged("messageClientIdCriteria");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    public class MessageIntervalCriteria : object, INotifyPropertyChanged
    {

        private DateTime fromField;

        private DateTime toField;

        private bool toFieldSpecified;


        [XmlElement(Order = 0)]
        public DateTime from
        {
            get
            {
                return fromField;
            }
            set
            {
                fromField = value;
                RaisePropertyChanged("from");
            }
        }


        [XmlElement(Order = 1)]
        public DateTime to
        {
            get
            {
                return toField;
            }
            set
            {
                toField = value;
                RaisePropertyChanged("to");
            }
        }


        [XmlIgnore]
        public bool toSpecified
        {
            get
            {
                return toFieldSpecified;
            }
            set
            {
                toFieldSpecified = value;
                RaisePropertyChanged("toSpecified");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    public class MessageClientIdCriteria : object, INotifyPropertyChanged
    {

        private string clientIdField;

        private ClientIdCriteria clientIdCriteriaField;


        [XmlElement(Order = 0)]
        public string clientId
        {
            get
            {
                return clientIdField;
            }
            set
            {
                clientIdField = value;
                RaisePropertyChanged("clientId");
            }
        }


        [XmlElement(Order = 1)]
        public ClientIdCriteria clientIdCriteria
        {
            get
            {
                return clientIdCriteriaField;
            }
            set
            {
                clientIdCriteriaField = value;
                RaisePropertyChanged("clientIdCriteria");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [Serializable]
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    public enum ClientIdCriteria
    {

        GET_REQUEST_BY_REQUEST_CLIENTID,
        GET_RESPONSE_BY_REQUEST_CLIENTID,
        GET_RESPONSE_BY_RESPONSE_CLIENTID
    }

    [Serializable]
    [XmlRoot(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
 
    [XmlType(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
  
    public class QueryResult : object, INotifyPropertyChanged
    {

        private SmevMetadata smevMetadataField;

        private Message messageField;
        

        [XmlElement(Order = 0)]
        public SmevMetadata smevMetadata
        {
            get
            {
                return smevMetadataField;
            }
            set
            {
                smevMetadataField = value;
                RaisePropertyChanged("smevMetadata");
            }
        }


        [XmlElement(Order = 1)]
        public Message Message
        {
            get
            {
                return messageField;
            }
            set
            {
                messageField = value;
                RaisePropertyChanged("Message");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if ((propertyChanged != null))
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [MessageContract(WrapperName = "FindMessageQuery", WrapperNamespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types", IsWrapped = true)]
    public class FindRequest
    {

        [MessageBodyMember(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types", Order = 0)]
        public string itSystem;

        [MessageBodyMember(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types", Order = 1)]
        public FindTypeCriteria specificQuery;

        public FindRequest()
        {
        }

        public FindRequest(string itSystem, FindTypeCriteria specificQuery)
        {
            this.itSystem = itSystem;
            this.specificQuery = specificQuery;
        }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [MessageContract(WrapperName = "QueryResultList", WrapperNamespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types", IsWrapped = true)]
    public class FindResponse
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces Xmlns = new XmlSerializerNamespaces(new[]
 { new XmlQualifiedName( "","urn://x-artefacts-smev-gov-ru/services/service-adapter/types")});

        [MessageBodyMember(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types", Order = 0)]
        [XmlElement("QueryResult")]
        public QueryResult[] QueryResult;

        public FindResponse()
        {
        }

        public FindResponse(QueryResult[] QueryResult)
        {
            this.QueryResult = QueryResult;
        }
    }


    [Serializable]
    [XmlRoot(ElementName = "ClientMessage", Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types")]
    [MessageContract(WrapperName = "ClientMessage", WrapperNamespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types", IsWrapped = true)]
    public class SendRequest
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces Xmlns = new XmlSerializerNamespaces(new[]
 { new XmlQualifiedName("","urn://x-artefacts-smev-gov-ru/services/service-adapter/types"),
   new XmlQualifiedName("ns2","urn://x-artefacts-smev-gov-ru/services/service-adapter/types/faults")
 });

        [MessageBodyMember(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types", Order = 0)]
        public string itSystem;

        [MessageBodyMember(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types", Order = 1)]
        public RequestMessageType RequestMessage;

        [MessageBodyMember(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types", Order = 2)]
        public ResponseMessageType ResponseMessage;

        public SendRequest()
        {
        }

        public SendRequest(string itSystem, RequestMessageType RequestMessage, ResponseMessageType ResponseMessage)
        {
            this.itSystem = itSystem;
            this.RequestMessage = RequestMessage;
            this.ResponseMessage = ResponseMessage;
        }
    }


    [MessageContract(WrapperName = "MessageResult", WrapperNamespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types", IsWrapped = true)]
    public class SendResponse
    {

        [MessageBodyMember(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types", Order = 0)]
        public string itSystem;

        [MessageBodyMember(Namespace = "urn://x-artefacts-smev-gov-ru/services/service-adapter/types", Order = 1)]
        public string MessageId;

        public SendResponse()
        {
        }

        public SendResponse(string itSystem, string MessageId)
        {
            this.itSystem = itSystem;
            this.MessageId = MessageId;
        }
    }


    public interface SMEVServiceAdapterPortTypeChannel : SMEVServiceAdapterPortType, IClientChannel
    {
    }

    public class SMEVServiceAdapterPortTypeClient : ClientBase<SMEVServiceAdapterPortType>, SMEVServiceAdapterPortType
    {

        public SMEVServiceAdapterPortTypeClient()
        {
        }

        public SMEVServiceAdapterPortTypeClient(string endpointConfigurationName) :
                base(endpointConfigurationName)
        {
        }

        public SMEVServiceAdapterPortTypeClient(string endpointConfigurationName, string remoteAddress) :
                base(endpointConfigurationName, remoteAddress)
        {
        }

        public SMEVServiceAdapterPortTypeClient(string endpointConfigurationName, EndpointAddress remoteAddress) :
                base(endpointConfigurationName, remoteAddress)
        {
        }

        public SMEVServiceAdapterPortTypeClient(Binding binding, EndpointAddress remoteAddress) :
                base(binding, remoteAddress)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        GetResponse SMEVServiceAdapterPortType.Get(GetRequest request)
        {
            return Channel.Get(request);
        }

        public SmevMetadata Get(string itSystem, string nodeId, QueryTypeCriteria specificQuery, out Message Message)
        {
            GetRequest inValue = new GetRequest();
            inValue.itSystem = itSystem;
            inValue.nodeId = nodeId;
            inValue.specificQuery = specificQuery;
            GetResponse retVal = ((SMEVServiceAdapterPortType)(this)).Get(inValue);
            Message = retVal.Message;
            return retVal.smevMetadata;
        }

        public Task<GetResponse> GetAsync(GetRequest request)
        {
            return Channel.GetAsync(request);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        FindResponse SMEVServiceAdapterPortType.Find(FindRequest request)
        {
            return Channel.Find(request);
        }

        public QueryResult[] Find(string itSystem, FindTypeCriteria specificQuery)
        {
            FindRequest inValue = new FindRequest();
            inValue.itSystem = itSystem;
            inValue.specificQuery = specificQuery;
            FindResponse retVal = ((SMEVServiceAdapterPortType)(this)).Find(inValue);
            return retVal.QueryResult;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        Task<FindResponse> SMEVServiceAdapterPortType.FindAsync(FindRequest request)
        {
            return Channel.FindAsync(request);
        }

        public Task<FindResponse> FindAsync(string itSystem, FindTypeCriteria specificQuery)
        {
            FindRequest inValue = new FindRequest();
            inValue.itSystem = itSystem;
            inValue.specificQuery = specificQuery;
            return ((SMEVServiceAdapterPortType)(this)).FindAsync(inValue);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        SendResponse SMEVServiceAdapterPortType.Send(SendRequest request)
        {
            return Channel.Send(request);
        }

        public string Send(ref string itSystem, RequestMessageType RequestMessage, ResponseMessageType ResponseMessage)
        {
            SendRequest inValue = new SendRequest();
            inValue.itSystem = itSystem;
            inValue.RequestMessage = RequestMessage;
            inValue.ResponseMessage = ResponseMessage;
            SendResponse retVal = ((SMEVServiceAdapterPortType)(this)).Send(inValue);
            itSystem = retVal.itSystem;
            return retVal.MessageId;
        }

        public Task<SendResponse> SendAsync(SendRequest request)
        {
            return Channel.SendAsync(request);
        }
    }
}
