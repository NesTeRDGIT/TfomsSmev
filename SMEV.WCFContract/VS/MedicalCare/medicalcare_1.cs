

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SmevAdapterService.VS.MedicalCare
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
            OutputData result = new OutputData();     
            while (reader.Read())
            {
                var medicalCare = new InsuredRenderingInfo()
                {
                    DateRenderingFrom = reader.GetDateTime(reader.GetOrdinal("DateRenderingFrom")),
                    DateRenderingTo = reader.GetDateTime(reader.GetOrdinal("DateRenderingTo")),
                    CareRegimen = reader.GetString(reader.GetOrdinal("CareRegimen")),
                    CareType = reader.GetString(reader.GetOrdinal("CareType")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    MedServicesSum = reader.GetDecimal(reader.GetOrdinal("MedServicesSum")),
                    ClinicName = reader.GetString(reader.GetOrdinal("ClinicName")),
                    RegionName = reader.GetString(reader.GetOrdinal("RegionName"))
                };
                result.InsuredRenderingList.Add(medicalCare);
            }
            if (result.InsuredRenderingList.Count == 0) return null;
            return result;
        }
    }

    /// <summary>
    /// Входные данные
    /// </summary>
    [Serializable]
    [XmlRoot(ElementName = "InputData", Namespace = "http://ffoms.ru/GetInsuredRenderedMedicalServices/1.0.0")]
    public class InputData : IRequestMessage
    {
    
        /// <summary>
        /// Запросы на получение сведений об оказанных медицинских услугах и их стоимости
        /// </summary>
        [XmlElement(Namespace = "http://ffoms.ru/GetInsuredRenderedMedicalServices/1.0.0")]
        public InsuredRenderingListRequest InsuredRenderingListRequest { get; set; }

        public IResponseMessage Answer(string connectionString)
        {
            var outData = new OutputData();

            string s = @"
                Select
                DateRenderingFrom,DateRenderingTo,CareRegimen,CareType,name_tarif as Name,sump as MedServicesSum,ClinicName,RegionName
                from V_MEDPOM_SMEV3 
                WHERE 
                FamilyName= :FamilyName and FirstName= :FirstName and Patronymic= :Patronymic and BirthDate= :BirthDate and
                DATERENDERINGFROM>= :DateFrom and nvl(DATERENDERINGTO,'31.12.2200')<= :DateTo and UnitedPolicyNumber = :UnitedPolicyNumber";

            var T_FamilyName = FormatParameter<string>(this.InsuredRenderingListRequest.FamilyName);
            var T_FirstName = FormatParameter<string>(this.InsuredRenderingListRequest.FirstName);
            var T_Patronymic = FormatParameter<string>(this.InsuredRenderingListRequest.Patronymic);
            var T_BirthDate = (DateTime)FormatParameter<DateTime>(this.InsuredRenderingListRequest.BirthDate);
            var T_DateFrom = (DateTime)FormatParameter<DateTime>(this.InsuredRenderingListRequest.DateFrom);
            var T_DateTo = (DateTime)FormatParameter<DateTime>(this.InsuredRenderingListRequest.DateTo);
            var T_UnitedPolicyNumber = FormatParameter<string>(this.InsuredRenderingListRequest.UnitedPolicyNumber);

            using (var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(connectionString))
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandTimeout = 0;
                    command.CommandType = CommandType.Text;
                    command.CommandText = s;

                    command.Parameters.AddRange(new List<Oracle.ManagedDataAccess.Client.OracleParameter>()
                        {
                            new Oracle.ManagedDataAccess.Client.OracleParameter("FamilyName",T_FamilyName),
                            new Oracle.ManagedDataAccess.Client.OracleParameter("FirstName",T_FirstName),
                            new Oracle.ManagedDataAccess.Client.OracleParameter("Patronymic",T_Patronymic),
                            new Oracle.ManagedDataAccess.Client.OracleParameter("BirthDate",T_BirthDate),
                            new Oracle.ManagedDataAccess.Client.OracleParameter("DateFrom",T_DateFrom),
                            new Oracle.ManagedDataAccess.Client.OracleParameter("DateTo",T_DateTo),
                            new Oracle.ManagedDataAccess.Client.OracleParameter("UnitedPolicyNumber",T_UnitedPolicyNumber)
                        }.ToArray());

                    try
                    {
                        connection.Open();
                        var reader = command.ExecuteReader();
                        outData = MedicalCareHelper.FromDataReader(reader);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        if (connection.State == ConnectionState.Open)
                            connection.Close();
                    }
                }

            }
            return outData;
        }

        public XElement Serialize()
        {
            throw new NotImplementedException();
        }

        private object FormatParameter<TType>(object obj)
        {
            Type Ttype = typeof(TType);

            if (Ttype == typeof(DateTime) && string.IsNullOrEmpty(obj.ToString())) return "31.12.2200";
            if (Ttype == typeof(DateTime) && string.IsNullOrEmpty(obj.ToString())) return "НЕТ";
            if (Ttype == typeof(string))
            {
                if (!string.IsNullOrEmpty(obj.ToString()))
                    return obj.ToString().ToUpper();
                else return "НЕТ";
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
    {/// <summary>
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
        public DateTime? DateFrom { get; set; }
        /// <summary>
        /// Дата окончания
        /// </summary>
        [XmlElement(Namespace = "http://ffoms.ru/GetInsuredRenderedMedicalServices/1.0.0")]
        public DateTime? DateTo { get; set; }
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
        [XmlNamespaceDeclarations()]
        public XmlSerializerNamespaces Xmlns = new XmlSerializerNamespaces(new XmlQualifiedName[] 
        { new XmlQualifiedName( "","http://ffoms.ru/GetInsuredRenderedMedicalServices/1.0.0") });
        public OutputData()
        {
      
        }
        public List<InsuredRenderingInfo> InsuredRenderingList { get; set; } = new List<InsuredRenderingInfo>();
        public XElement Serialize()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(OutputData));
            MemoryStream memoryStream = new MemoryStream();
            XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            xmlTextWriter.Formatting = Formatting.Indented;
            xmlSerializer.Serialize(xmlTextWriter, this, this.Xmlns);
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
