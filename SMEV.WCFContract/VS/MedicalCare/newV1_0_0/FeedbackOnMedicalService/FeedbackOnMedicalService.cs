using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using SmevAdapterService.VS;

namespace SMEV.VS.MedicalCare.newV1_0_0.FeedbackOnMedicalService
{

    /* REQUEST
     Сведения из ТФОМС об оказанных медицинских услугах и их стоимости
    */

    public static class MedicalCareHelper
    {
        public static OutputData FromDataReader(IDataReader reader)
        {/*
            var result = new OutputData();
            while (reader.Read())
            {
                decimal? usl_id = null;
                if (reader["USL_ID"] != DBNull.Value)
                    usl_id = Convert.ToDecimal(reader["USL_ID"]);
                var medicalCare = new InsuredRenderingInfo
                {
                    SLUCH_Z_ID = Convert.ToDecimal(reader["SLUCH_Z_ID"]),
                    SLUCH_ID = Convert.ToDecimal(reader["SLUCH_ID"]),
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
            return result.InsuredRenderingList.Count == 0 ? null : result;*/
            return null;
        }
    }

    /// <summary>
    /// Входные данные
    /// </summary>
    [Serializable]
    [XmlRoot(ElementName = "InputData", Namespace = "http://ffoms.ru/FeedbackOnMedicalService/1.0.0")]
    public class InputData : IRequestMessage
    {

        public static InputData CreateTest()
        {
            return new InputData()
            {
                orderId = "123456789",
                ApplicationID = "12345678",
                InsuredAppealList = new List<InsuredAppealInfoType>
                {
                    new InsuredAppealInfoType()
                    {
                        AppealNumber =  "12345678.123456789",
                        AppealTopic = "Медицинская услуга не была мне оказана",
                        AppealDetail = "Я не обращался в ГБУЗ РА \"Майкопская городская поликлиника №2\" 15.06.2017, услугу осмотра врача-терапевта не получал",
                        DateRenderingFrom = new DateTime(2018,6,15),
                        DateRenderingTo = new DateTime(2018,6,15),
                        CareRegimen = "Амбулаторно (в условиях, не предусматривающих круглосуточного медицинского наблюдения и лечения), в том числе на дому при вызове медицинского работника",
                        Name = "Осмотр врача-терапевта",
                        ClinicName = "ГБУЗ РА \"МАЙКОПСКАЯ ГОРОДСКАЯ ПОЛИКЛИНИКА № 2\"",
                        RegionName = "Республика Адыгея",
                        RegionCode = "79000",
                        CareType = "первичная медико-санитарная помощь",
                        MedServicesSum = 600,
                        MedServicesID = "5d78bbc8-be9c-11e9-9cb5-2a2ae2dbcce4",
                        AppealTopicCode = 1
                    }
                }
            };

        }
        [XmlNamespaceDeclarations]
        public static XmlSerializerNamespaces XmlnsClass = new XmlSerializerNamespaces(new[] { new XmlQualifiedName("ns1", "http://ffoms.ru/FeedbackOnMedicalService/1.0.0") });

        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces Xmlns = new XmlSerializerNamespaces(new[] { new XmlQualifiedName("ns1", "http://ffoms.ru/FeedbackOnMedicalService/1.0.0") });
        /// <summary>
        /// Среда СМЭФ
        /// </summary>
        [XmlAttribute]
        public envType env { get; set; } = envType.DEV;
        /// <summary>
        /// Идентификатор заявления
        /// </summary>
        [XmlElement]
        public string orderId { get; set; }
        /// <summary>
        /// Номер заявки, в рамках которого сформировано обращение застрахованного
        /// </summary>
        [XmlElement]
        public string ApplicationID { get; set; }
        /// <summary>
        /// Список обращений застрахованных
        /// </summary>
        [XmlArray(ElementName = "InsuredAppealList")]
        [XmlArrayItem("InsuredAppealInfo")]
        public List<InsuredAppealInfoType> InsuredAppealList { get; set; } = new List<InsuredAppealInfoType>();

     
      
        public IResponseMessage Answer(string connectionString)
        {
            return new OutputData()
            {
                env = env,
                orderId = orderId,
                AppealResultList = InsuredAppealList.Select(x=> new AppealResultInfo(x) { AppealResult = "Ваше обращение было рассмотрено, проведена Медико-экономическая экспертиза, в рамках которой было установлено, что услуга действительно Вам не была оказана. В связи с этим было принято решение о применение мер, предусмотренных статьей 41 Федерального закона от 29 ноября 2010 года N 326-ФЗ \"Об обязательном медицинском страховании в Российской Федерации\"" }).ToList()
            };
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
    [Serializable]
    [XmlRoot("InsuredAppealInfo",Namespace = "http://ffoms.ru/FeedbackOnMedicalService/1.0.0")]

    public class InsuredAppealInfoType: AppealInfoType
    {
        /// <summary>
        /// Пятизначный код ОКАТО субъекта Российской Федерации
        /// </summary>
        [XmlElement]
        public string RegionCode { get; set; }

        /// <summary>
        /// Виды медицинской помощи
        /// </summary>
        [XmlElement]
        public string CareType { get; set; }
        /// <summary>
        /// Стоимость (руб.)
        /// </summary>
        [XmlElement]
        public decimal MedServicesSum { get; set; }
        /// <summary>
        /// Внутренний идентификатор медицинской услуги, оказанной застрахованному лицу
        /// </summary>
        [XmlElement]
        public string MedServicesID { get; set; }
        /// <summary>
        /// Код темы обращения
        /// </summary>
        [XmlElement]
        public int AppealTopicCode { get; set; }

    }
    [XmlRoot(ElementName = "")]
    public class AppealInfoType
    {
        /// <summary>
        /// Номер обращения ЕПГУ
        /// </summary>
        [XmlElement]
        public string AppealNumber { get; set; }
        /// <summary>
        /// Тема обращения
        /// </summary>
        [XmlElement]
        public string AppealTopic { get; set; }
        /// <summary>
        /// Обращение подробно
        /// </summary>
        [XmlElement]
        public  string AppealDetail { get; set; }
        /// <summary>
        /// Дата начала услуги
        /// </summary>
        [XmlElement(DataType = "date")]
        public DateTime DateRenderingFrom { get; set; }
        /// <summary>
        /// Дата окончание услуги
        /// </summary>
        [XmlElement(DataType = "date")]
        public DateTime DateRenderingTo { get; set; }
        /// <summary>
        /// Условие оказания
        /// </summary>
        [XmlElement]
        public  string CareRegimen { get; set; }
        /// <summary>
        /// Наименование мед услуги
        /// </summary>
        [XmlElement]
        public string Name { get; set; }
        /// <summary>
        /// Наименование МО
        /// </summary>
        [XmlElement]
        public string ClinicName { get; set; }
        /// <summary>
        /// Субъект РФ в котором оказана услуга
        /// </summary>
        [XmlElement]
        public  string RegionName { get; set; }
    }

    /// <summary>
    /// выходные данные
    /// </summary>
    [Serializable]
    [XmlRoot(ElementName = "OutputData", Namespace = "http://ffoms.ru/FeedbackOnMedicalService/1.0.0")]
    public class OutputData : IResponseMessage
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces Xmlns = new XmlSerializerNamespaces(new[] { new XmlQualifiedName("ns1", "http://ffoms.ru/FeedbackOnMedicalService/1.0.0") });

        /// <summary>
        /// Среда СМЭФ
        /// </summary>
        [XmlAttribute]
        public envType env { get; set; } = envType.DEV;

        /// <summary>
        /// Идентификатор заявления
        /// </summary>
        [XmlElement]
        public string orderId { get; set; }
        public List<AppealResultInfo> AppealResultList { get; set; }


        public XElement Serialize()
        {
            var xmlSerializer = new XmlSerializer(typeof(OutputData));
            var memoryStream = new MemoryStream();
            var xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8) { Formatting = Formatting.Indented };
            xmlSerializer.Serialize(xmlTextWriter, this, Xmlns);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return XElement.Load(memoryStream);
        }

    }

    public enum envType
    {
        [XmlEnum(Name = "DEV")]
        DEV,
        [XmlEnum(Name = "TCOD")]
        TCOD,
        [XmlEnum(Name = "PROD")]
        PROD
    }


    [Serializable]
    [XmlRoot(ElementName = "AppealResultInfo", Namespace = "http://ffoms.ru/FeedbackOnMedicalService/1.0.0")]
    public class AppealResultInfo:AppealInfoType
    {
        public AppealResultInfo()
        {

        }
        public AppealResultInfo(AppealInfoType item)
        {
            this.AppealDetail = item.AppealDetail;
            this.AppealNumber = item.AppealNumber;
            this.AppealTopic = item.AppealTopic;
            this.CareRegimen = item.CareRegimen;
            this.ClinicName = item.ClinicName;
            this.DateRenderingFrom = item.DateRenderingFrom;
            this.DateRenderingTo = item.DateRenderingTo;
            this.Name = item.Name;
            this.RegionName = item.RegionName;
        }
        public  string AppealResult { get; set; }
    }

}
