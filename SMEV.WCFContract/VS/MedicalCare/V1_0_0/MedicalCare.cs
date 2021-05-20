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

namespace SMEV.VS.MedicalCare.V1_0_0
{

    /* REQUEST
      <ns1:InputData xmlns:ns1="http://ffoms.ru/GetInsuredRenderedMedicalServices/1.0.0" xmlns:q1="urn://x-artefacts-smev-gov-ru/supplementary/commons/1.2">
                <ns1:InsuredRenderingListRequest>
                    <ns1:RegionCode>76000</ns1:RegionCode>
                    <q1:FamilyName>ЕМЕЛИН</q1:FamilyName>
                    <q1:FirstName>ИЛЬЯ</q1:FirstName>
                    <q1:Patronymic>НИКОЛАЕВИЧ</q1:Patronymic>
                    <ns1:BirthDate>1964-02-28</ns1:BirthDate>
                    <ns1:UnitedPolicyNumber>3210987654321098</ns1:UnitedPolicyNumber>
                    <ns1:DateFrom>2017-01-01</ns1:DateFrom>
                    <ns1:DateTo>2017-12-31</ns1:DateTo>
                </ns1:InsuredRenderingListRequest>
                </ns1:InputData>
    */

    /// <summary>
    /// Входные данные
    /// </summary>
    [Serializable]
    [XmlRoot(ElementName = "InputData", Namespace = "http://ffoms.ru/GetInsuredRenderedMedicalServices/1.0.0")]
    public class InputData
    {

        [XmlNamespaceDeclarations]
        public static XmlSerializerNamespaces XmlnsClass = new XmlSerializerNamespaces(new[] { new XmlQualifiedName("ns1", "http://ffoms.ru/GetInsuredRenderedMedicalServices/1.0.0") });

        /// <summary>
        /// Запросы на получение сведений об оказанных медицинских услугах и их стоимости
        /// </summary>
        [XmlElement(Namespace = "http://ffoms.ru/GetInsuredRenderedMedicalServices/1.0.0")]
        public InsuredRenderingListRequest InsuredRenderingListRequest { get; set; }        
    }
    /// <summary>
    /// Запрос на получение сведений об оказанных медицинских услугах и их стоимости
    /// </summary>
    [Serializable]
    [XmlRoot(ElementName = "InsuredRenderingListRequest", Namespace = "http://ffoms.ru/GetInsuredRenderedMedicalServices/1.0.0")]
    public class InsuredRenderingListRequest
    {
        /// <summary>
        /// Пятизначный код ОКАТО субъекта Российской Федерации
        /// </summary>
        [XmlElement(Namespace = "http://ffoms.ru/GetInsuredRenderedMedicalServices/1.0.0")]
        public string RegionCode { get; set; }
        /// <summary>
        /// Фамимлия
        /// </summary>
        [XmlElement(Namespace = "urn://x-artefacts-smev-gov-ru/supplementary/commons/1.2")]
        public string FamilyName { get; set; }
        /// <summary>
        /// Имя
        /// </summary>
        [XmlElement(Namespace = "urn://x-artefacts-smev-gov-ru/supplementary/commons/1.2")]
        public string FirstName { get; set; }
        /// <summary>
        /// Отчество
        /// </summary>
        [XmlElement(Namespace = "urn://x-artefacts-smev-gov-ru/supplementary/commons/1.2")]
        public string Patronymic { get; set; }
        /// <summary>
        /// Дата рождения
        /// </summary>
        [XmlElement(Namespace = "http://ffoms.ru/GetInsuredRenderedMedicalServices/1.0.0")]
        public DateTime BirthDate { get; set; }
        /// <summary>
        /// ЕНП
        /// </summary>
        [XmlElement(Namespace = "http://ffoms.ru/GetInsuredRenderedMedicalServices/1.0.0")]
        public string UnitedPolicyNumber { get; set; }
        /// <summary>
        /// Дата начала
        /// </summary>
        [XmlElement(Namespace = "http://ffoms.ru/GetInsuredRenderedMedicalServices/1.0.0")]
        public DateTime DateFrom { get; set; }
        /// <summary>
        /// Дата окончания
        /// </summary>
        [XmlElement(Namespace = "http://ffoms.ru/GetInsuredRenderedMedicalServices/1.0.0")]
        public DateTime DateTo { get; set; }
    }


    /* RESPONSE
    <ns1:OutputData xmlns:ns1="http://ffoms.ru/GetInsuredRenderedMedicalServices/1.0.0">
        - <ns1:InsuredRenderingList>
        - <ns1:InsuredRenderingInfo>
          <ns1:DateRenderingFrom>2017-06-15</ns1:DateRenderingFrom> 
          <ns1:DateRenderingTo>2017-06-15</ns1:DateRenderingTo> 
          <ns1:CareRegimen>первичная медико-санитарная помощь</ns1:CareRegimen> 
          <ns1:CareType>Амбулаторно (в условиях, не предусматривающих круглосуточного медицинского наблюдения и лечения), в том числе на дому при вызове медицинского работника</ns1:CareType> 
          <ns1:Name>Осмотр врача-терапевта</ns1:Name> 
          <ns1:MedServicesSum>600.00</ns1:MedServicesSum> 
          <ns1:ClinicName>ГБУЗ РА "МАЙКОПСКАЯ ГОРОДСКАЯ ПОЛИКЛИНИКА № 2"</ns1:ClinicName> 
          <ns1:RegionName>Республика Адыгея</ns1:RegionName> 
          </ns1:InsuredRenderingInfo>
        - <ns1:InsuredRenderingInfo>
          <ns1:DateRenderingFrom>2017-06-16</ns1:DateRenderingFrom> 
          <ns1:DateRenderingTo>2017-06-16</ns1:DateRenderingTo> 
          <ns1:CareRegimen>первичная медико-санитарная помощь</ns1:CareRegimen> 
          <ns1:CareType>Амбулаторно (в условиях, не предусматривающих круглосуточного медицинского наблюдения и лечения), в том числе на дому при вызове медицинского работника</ns1:CareType> 
          <ns1:Name>Осмотр врача-травматолога</ns1:Name> 
          <ns1:MedServicesSum>950.00</ns1:MedServicesSum> 
          <ns1:ClinicName>ГБУЗ РА "МАЙКОПСКАЯ ГОРОДСКАЯ ПОЛИКЛИНИКА № 2"</ns1:ClinicName> 
          <ns1:RegionName>Республика Адыгея</ns1:RegionName> 
          </ns1:InsuredRenderingInfo>
          </ns1:InsuredRenderingList>
          </ns1:OutputData>
  </MessagePrimaryContent>
    */
    /// <summary>
    /// выходные данные
    /// </summary>
    [Serializable]  
    [XmlRoot(ElementName = "OutputData", Namespace = "http://ffoms.ru/GetInsuredRenderedMedicalServices/1.0.0")]
    public class OutputData : IResponseMessage
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces Xmlns = new XmlSerializerNamespaces(new[] 
        { new XmlQualifiedName( "","http://ffoms.ru/GetInsuredRenderedMedicalServices/1.0.0") });

        public List<InsuredRenderingInfo> InsuredRenderingList { get; set; } = new List<InsuredRenderingInfo>();
        public XElement Serialize()
        {
            var xmlSerializer = new XmlSerializer(typeof(OutputData));
            var memoryStream = new MemoryStream();
            var xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            xmlTextWriter.Formatting = Formatting.Indented;
            xmlSerializer.Serialize(xmlTextWriter, this, Xmlns);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return XElement.Load(memoryStream);
        }

    }


    [Serializable]
    [XmlRoot(ElementName = "InsuredRenderingInfo", Namespace = "http://ffoms.ru/GetInsuredRenderedMedicalServices/1.0.0")]
    public class InsuredRenderingInfo
    {
        /// <summary>
        /// Дата начала оказания медицинской услуги, Namespace = "http://ffoms.ru/GetInsuredRenderedMedicalServices/1.0.0",
        /// </summary>
        [XmlElement( DataType = "date")]
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
        /// Cубъекта Российской Федерации, в котором оказана медицинская услуга
        /// </summary>
        [XmlElement]
        public string RegionName { get; set; }

    }
 }
