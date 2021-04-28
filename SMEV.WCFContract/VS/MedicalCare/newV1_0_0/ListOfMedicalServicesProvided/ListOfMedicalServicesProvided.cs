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
                
                medicalCare.MedServicesID = $"{(medicalCare.IsMTR? "1":"0")}_{medicalCare.SLUCH_ID}_{medicalCare.USL_ID}";
                result.InsuredRenderingList.Add(medicalCare);
            }
            return result.InsuredRenderingList.Count == 0 ? null : result;
        }
    }

    /// <summary>
    /// Входные данные
    /// </summary>
    [Serializable]
    [XmlRoot(ElementName = "InputData", Namespace = "http://ffoms.ru/ListOfMedicalServicesProvided/1.0.0")]
    public class InputData : IRequestMessage
    {
        [XmlNamespaceDeclarations]
        public static XmlSerializerNamespaces XmlnsClass = new XmlSerializerNamespaces(new[] { new XmlQualifiedName("ns1", "http://ffoms.ru/ListOfMedicalServicesProvided/1.0.0") });

        [XmlNamespaceDeclarations]
        public  XmlSerializerNamespaces Xmlns = new XmlSerializerNamespaces(new[] { new XmlQualifiedName("ns1", "http://ffoms.ru/ListOfMedicalServicesProvided/1.0.0") });
        public static InputData CreateTest()
        {
            return new InputData()
            {
                RegionCode = "79000",
                FamilyName = "ЕМЕЛИН",
                FirstName = "ИЛЬЯ",
                Patronymic = "НИКОЛАЕВИЧ",
                BirthDate = new DateTime(1964, 02, 28),
                UnitedPolicyNumber = "3210987654321098",
                DateFrom = new DateTime(2017, 01, 01),
                DateTo = new DateTime(2017, 12, 31),
                orderId = "12345678"
            };
        }

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
        public IResponseMessage Answer(string connectionString)
        {
            var s = @"Select sluch_z_id, sluch_id, usl_id, isMTR,
                DateRenderingFrom,DateRenderingTo,CareRegimen,CareType,Name,sump as MedServicesSum,ClinicName,RegionName
                from V_MEDPOM_SMEV3 
                WHERE 
                FamilyName= :FamilyName and FirstName= :FirstName and Patronymic= :Patronymic and BirthDate= :BirthDate and
                DATERENDERINGFROM>= :DateFrom and nvl(DATERENDERINGTO,'31.12.2200')<= :DateTo and UnitedPolicyNumber = :UnitedPolicyNumber";

            var T_FamilyName = FormatParameter<string>(FamilyName);
            var T_FirstName = FormatParameter<string>(FirstName);
            var T_Patronymic = FormatParameter<string>(Patronymic);
            var T_BirthDate = (DateTime)FormatParameter<DateTime>(BirthDate);
            var T_DateFrom = (DateTime)FormatParameter<DateTime>(DateFrom);
            var T_DateTo = (DateTime)FormatParameter<DateTime>(DateTo);
            var T_UnitedPolicyNumber = FormatParameter<string>(UnitedPolicyNumber);

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
                        return  MedicalCareHelper.FromDataReader(reader);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Ошибка чтения ответа: {ex.Message}", ex);
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
            var xmlSerializer = new XmlSerializer(typeof(InputData));
            var memoryStream = new MemoryStream();
            var xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8) { Formatting = Formatting.Indented };
            xmlSerializer.Serialize(xmlTextWriter, this, Xmlns);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return XElement.Load(memoryStream);
        }

        private object FormatParameter<TType>(object obj)
        {
            var Ttype = typeof(TType);

            if (Ttype == typeof(DateTime) && string.IsNullOrEmpty(obj.ToString())) return "31.12.2200";
            if (Ttype == typeof(DateTime) && string.IsNullOrEmpty(obj.ToString())) return "НЕТ";
            if (Ttype == typeof(string))
            {

                if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
                    return obj.ToString().ToUpper();
                return "НЕТ";
            }
            return obj;
        }
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
            var xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8) {Formatting = Formatting.Indented};
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
        [XmlIgnore]
        public int SLUCH_Z_ID { get; set; }
        [XmlIgnore]
        public int SLUCH_ID { get; set; }
        [XmlIgnore]
        public int? USL_ID { get; set; }
        [XmlIgnore]
        public bool IsMTR { get; set; }
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
