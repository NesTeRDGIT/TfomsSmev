using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Oracle.ManagedDataAccess.Client;
using SmevAdapterService.VS;

namespace SMEV.VS.MedicalCare.newV1_0_0.ListOfMedicalServicesProvided
{

    /* REQUEST
     Сведения из ТФОМС об оказанных медицинских услугах и их стоимости
    */

    /// <summary>
    /// Входные данные
    /// </summary>
    [Serializable]
    [XmlRoot(ElementName = "InputData", Namespace = "http://ffoms.ru/ListOfMedicalServicesProvided/1.0.0")]
    public class InputData
    {
        [XmlNamespaceDeclarations]
        public static XmlSerializerNamespaces XmlnsClass = new XmlSerializerNamespaces(new[] { new XmlQualifiedName("ns1", "http://ffoms.ru/ListOfMedicalServicesProvided/1.0.0") });

        [XmlNamespaceDeclarations]
        public  XmlSerializerNamespaces Xmlns = new XmlSerializerNamespaces(new[] { new XmlQualifiedName("ns1", "http://ffoms.ru/ListOfMedicalServicesProvided/1.0.0") });
        /// <summary>
        /// Среда СМЭВ
        /// </summary>
        [XmlAttribute] public envType env { get; set; } = envType.DEV;
        /// <summary>
        /// Пятизначный код ОКАТО субъекта Российской Федерации
        /// </summary>
        [XmlElement]
        public string RegionCode { get; set; }
        /// <summary>
        /// Фамилия
        /// </summary>
        [XmlElement]
        public string FamilyName { get; set; }
        /// <summary>
        /// Имя
        /// </summary>
        [XmlElement]
        public string FirstName { get; set; }
        /// <summary>
        /// Отчество
        /// </summary>
        [XmlElement]
        public string Patronymic { get; set; }
        /// <summary>
        /// Дата рождения
        /// </summary>
        [XmlElement(DataType = "date")]
        public DateTime BirthDate { get; set; }
        /// <summary>
        /// ЕНП
        /// </summary>
        [XmlElement]
        public string UnitedPolicyNumber { get; set; }
        /// <summary>
        /// Дата начала
        /// </summary>
        [XmlElement(DataType = "date")]
        public DateTime DateFrom { get; set; }
        /// <summary>
        /// Дата окончания
        /// </summary>
        [XmlElement(DataType = "date")]
        public DateTime DateTo { get; set; }
        /// <summary>
        /// Идентификатор заявления
        /// </summary>
        [XmlElement]
        public string orderId { get; set; }
    }
   
    /// <summary>
    /// выходные данные
    /// </summary>
    [Serializable]
    [XmlRoot(ElementName = "OutputData", Namespace = "http://ffoms.ru/ListOfMedicalServicesProvided/1.0.0")]
    public class OutputData : IResponseMessage
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces Xmlns = new XmlSerializerNamespaces(new[] { new XmlQualifiedName( "ns1","http://ffoms.ru/ListOfMedicalServicesProvided/1.0.0") });
        [XmlAttribute]
        public envType env { get; set; } = envType.DEV;
        [XmlElement]
        public string orderId { get; set; }
        public List<InsuredRenderingInfo> InsuredRenderingList { get; set; } = new List<InsuredRenderingInfo>();
        public XElement Serialize()
        {
            var xmlSerializer = new XmlSerializer(typeof(OutputData));
            var memoryStream = new MemoryStream();
            var xmlTextWriter = new XmlTextWriter(memoryStream, new UTF8Encoding(false)) {Formatting = Formatting.Indented};
            xmlSerializer.Serialize(xmlTextWriter, this, Xmlns);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return XElement.Load(memoryStream);
        }

    }

    public enum envType
    {
        [XmlEnum("DEV")]
        DEV,
        [XmlEnum("TCOD")]
        TCOD,
        [XmlEnum("PROD")]
        PROD
    }


    [Serializable]
    [XmlRoot(ElementName = "InsuredRenderingInfo", Namespace = "http://ffoms.ru/ListOfMedicalServicesProvided/1.0.0")]
    public class InsuredRenderingInfo
    {
        /// <summary>
        /// Дата начала оказания медицинской услуги
        /// </summary>
        [XmlElement(DataType = "date")]
        public DateTime DateRenderingFrom { get; set; }
        /// <summary>
        /// Дата окончания оказания медицинской услуги
        /// </summary>
        [XmlElement(DataType = "date")]
        public DateTime DateRenderingTo { get; set; }
        /// <summary>
        /// Условие оказания медицинской помощи
        /// </summary>
        [XmlElement]
        public string CareRegimen { get; set; }
        /// <summary>
        /// Виды медицинской помощи
        /// </summary>
        [XmlElement]
        public string CareType { get; set; }
        /// <summary>
        /// Наименование медицинской услуги
        /// </summary>
        [XmlElement]
        public string Name { get; set; }
        /// <summary>
        /// Стоимость (руб.)
        /// </summary>
        [XmlElement]
        public decimal MedServicesSum { get; set; }
        /// <summary>
        /// Наименование медицинской организации - юридического лица
        /// </summary>
        [XmlElement]
        public string ClinicName { get; set; }
        /// <summary>
        /// Код субъекта Российской Федерации
        /// </summary>
        [XmlElement]
        public string RegionCode { get; set; }
        /// <summary>
        /// Субъекта Российской Федерации, в котором оказана медицинская услуга
        /// </summary>
        [XmlElement]
        public string RegionName { get; set; }
        /// <summary>
        /// Внутренний идентификатор медицинской услуги, оказанной застрахованному лицу
        /// </summary>
        [XmlElement]
        public string MedServicesID { get; set; }
    }

}
