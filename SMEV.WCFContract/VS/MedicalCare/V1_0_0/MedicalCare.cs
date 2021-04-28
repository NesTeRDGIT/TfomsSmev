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

    public static class MedicalCareHelper
    {
        public static OutputData FromDataReader(IDataReader reader)
        {
            var result = new OutputData();     
            while (reader.Read())
            {
                int? usl_id = null;
                if (reader["USL_ID"] != DBNull.Value)
                    usl_id = Convert.ToInt32(reader["USL_ID"]);
                var medicalCare = new InsuredRenderingInfo
                {
                    SLUCH_Z_ID = Convert.ToInt32(reader["SLUCH_Z_ID"]),
                    SLUCH_ID = Convert.ToInt32(reader["SLUCH_ID"]),
                    USL_ID = usl_id,
                    IsMTR = Convert.ToBoolean(reader["IsMTR"]),
                    DateRenderingFrom = Convert.ToDateTime(reader["DateRenderingFrom"]),
                    DateRenderingTo = Convert.ToDateTime(reader["DateRenderingTo"]),
                    CareRegimen = Convert.ToString(reader["CareRegimen"]),
                    CareType = Convert.ToString(reader["CareType"]),
                    Name = Convert.ToString(reader["Name"]),
                    MedServicesSum = Convert.ToDecimal(reader["MedServicesSum"]),
                    ClinicName = Convert.ToString(reader["ClinicName"]),
                    RegionName = Convert.ToString(reader["RegionName"])
                };
                result.InsuredRenderingList.Add(medicalCare);
            }
            return result.InsuredRenderingList.Count == 0 ? null : result;
        }
    }

    /// <summary>
    /// Входные данные
    /// </summary>
    [Serializable]
    [XmlRoot(ElementName = "InputData", Namespace = "http://ffoms.ru/GetInsuredRenderedMedicalServices/1.0.0")]
    public class InputData : IRequestMessage
    {

        [XmlNamespaceDeclarations]
        public static XmlSerializerNamespaces XmlnsClass = new XmlSerializerNamespaces(new[] { new XmlQualifiedName("ns1", "http://ffoms.ru/GetInsuredRenderedMedicalServices/1.0.0") });

        /// <summary>
        /// Запросы на получение сведений об оказанных медицинских услугах и их стоимости
        /// </summary>
        [XmlElement(Namespace = "http://ffoms.ru/GetInsuredRenderedMedicalServices/1.0.0")]
        public InsuredRenderingListRequest InsuredRenderingListRequest { get; set; }

        public IResponseMessage Answer(string connectionString)
        {
          
            var s = @"Select sluch_z_id, sluch_id, usl_id, isMTR,
                DateRenderingFrom,DateRenderingTo,CareRegimen,CareType,Name,sump as MedServicesSum,ClinicName,RegionName
                from V_MEDPOM_SMEV3 
                WHERE 
                FamilyName= :FamilyName and FirstName= :FirstName and Patronymic= :Patronymic and BirthDate= :BirthDate and
                DATERENDERINGFROM>= :DateFrom and nvl(DATERENDERINGTO,'31.12.2200')<= :DateTo and UnitedPolicyNumber = :UnitedPolicyNumber";

            var T_FamilyName = FormatParameter<string>(InsuredRenderingListRequest.FamilyName);
            var T_FirstName = FormatParameter<string>(InsuredRenderingListRequest.FirstName);
            var T_Patronymic = FormatParameter<string>(InsuredRenderingListRequest.Patronymic);
            var T_BirthDate = (DateTime)FormatParameter<DateTime>(InsuredRenderingListRequest.BirthDate);
            var T_DateFrom = (DateTime)FormatParameter<DateTime>(InsuredRenderingListRequest.DateFrom);
            var T_DateTo = (DateTime)FormatParameter<DateTime>(InsuredRenderingListRequest.DateTo);
            var T_UnitedPolicyNumber = FormatParameter<string>(InsuredRenderingListRequest.UnitedPolicyNumber);

            using (var connection = new OracleConnection(connectionString))
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandTimeout = 0;
                    command.CommandType = CommandType.Text;
                    command.CommandText = s;

                    command.Parameters.AddRange(new List<OracleParameter>
                    {
                            new OracleParameter("FamilyName",T_FamilyName),
                            new OracleParameter("FirstName",T_FirstName),
                            new OracleParameter("Patronymic",T_Patronymic),
                            new OracleParameter("BirthDate",T_BirthDate),
                            new OracleParameter("DateFrom",T_DateFrom),
                            new OracleParameter("DateTo",T_DateTo),
                            new OracleParameter("UnitedPolicyNumber",T_UnitedPolicyNumber)
                        }.ToArray());

                    try
                    {
                        connection.Open();
                        var reader = command.ExecuteReader();
                        return MedicalCareHelper.FromDataReader(reader);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Ошибка чтения ответа: {ex.Message}",ex);
                    }
                    finally
                    {
                        if (connection.State == ConnectionState.Open)
                            connection.Close();
                    }
                }

            }
          
        }

        public XElement Serialize()
        {
            throw new NotImplementedException();
        }

        private object FormatParameter<TType>(object obj)
        {
            var Ttype = typeof(TType);

            if (Ttype == typeof(DateTime) && string.IsNullOrEmpty(obj.ToString())) return "31.12.2200";
            if (Ttype == typeof(DateTime) && string.IsNullOrEmpty(obj.ToString())) return "НЕТ";
            if (Ttype == typeof(string))
            {
                
                if (obj!=null && !string.IsNullOrEmpty(obj.ToString()))
                    return obj.ToString().ToUpper();
                return "НЕТ";
            }
            return obj;
        }
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
        [XmlIgnore]
        public int SLUCH_Z_ID {get;set;}
        [XmlIgnore]
        public int SLUCH_ID { get; set; }
        [XmlIgnore]
        public int? USL_ID { get; set; }
        [XmlIgnore]
        public bool IsMTR { get; set; }
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
