using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SMEV.WCFContract
{
    public class LogRow
    {
        public static List<LogRow> Get(IEnumerable<DataRow> rows)
        {
            return rows.Select(Get).ToList();
        }

        public static LogRow Get(DataRow row)
        {
            try
            {
                var item = new LogRow();
                item.ID = Convert.ToInt32(row["ID"]);
                item.ITSYSTEM = Convert.ToString(row["ITSYSTEM"]);
                item.VS = (MessageLoggerVS)Convert.ToInt32(row["VS"]);
                item.ID_MESSAGE_IN = Convert.ToString(row["ID_MESSAGE_IN"]);
                if (row["STATUS_IN"] != DBNull.Value)
                    item.STATUS_IN = MessageLoggerStatusFromString(Convert.ToInt32(row["STATUS_IN"]));
                item.COMMENT_IN = Convert.ToString(row["COMMENT_IN"]);
                item.ID_MESSAGE_OUT = Convert.ToString(row["ID_MESSAGE_OUT"]);
                if (row["STATUS_OUT"] != DBNull.Value)
                    item.STATUS_OUT = MessageLoggerStatusFromString(Convert.ToInt32(row["STATUS_OUT"]));
                item.COMMENT_OUT = Convert.ToString(row["COMMENT_OUT"]);
                if (row["DATE_IN"] != DBNull.Value)
                    item.DATE_IN = Convert.ToDateTime(row["DATE_IN"]);
                if (row["DATE_OUT"] != DBNull.Value)
                    item.DATE_OUT = Convert.ToDateTime(row["DATE_OUT"]);

                item.OrderId = Convert.ToString(row["OrderId"]);
                item.ApplicationId = Convert.ToString(row["ApplicationId"]);
                return item;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка получения LogRow: {ex.Message}", ex);
            }
        }

        private static MessageLoggerStatus? MessageLoggerStatusFromString(int value)
        {
            return (MessageLoggerStatus)value;
        }

        /// <summary>
        /// Ид сообщения в БД
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// Мнемоника
        /// </summary>
        public string ITSYSTEM { get; set; } = "";
        /// <summary>
        /// Вид сведений
        /// </summary>
        public MessageLoggerVS VS { get; set; } = MessageLoggerVS.UNKNOW;
        /// <summary>
        /// Ид входящего сообщения СМЭВ
        /// </summary>
        public string ID_MESSAGE_IN { get; set; } = "";
        /// <summary>
        /// Статус входящего сообщения СМЭВ
        /// </summary>
        public MessageLoggerStatus? STATUS_IN { get; set; }
        /// <summary>
        /// Коментарий сообщения СМЭВ
        /// </summary>
        public string COMMENT_IN { get; set; } = "";
        /// <summary>
        /// Ид исходящего сообщения СМЭВ
        /// </summary>
        public string ID_MESSAGE_OUT { get; set; } = "";
        /// <summary>
        /// Статус исходящего сообщения СМЭВ
        /// </summary>
        public MessageLoggerStatus? STATUS_OUT { get; set; }
        /// <summary>
        /// Коментарий исходящего сообщения СМЭВ
        /// </summary>
        public string COMMENT_OUT { get; set; } = "";
        public DateTime? DATE_IN { get; set; }
        public DateTime? DATE_OUT { get; set; }

        public string OrderId { get; set; } = "";
        public string ApplicationId { get; set; } = "";


    }

    [DataContract]
    public class MedpomData
    {
        public MedpomData(MedpomInData IN, List<MedpomOutData> OUT)
        {
            this.IN = IN;
            this.OUT = OUT;
        }
        [DataMember]
        public MedpomInData IN { get; set; }
        [DataMember]
        public List<MedpomOutData> OUT { get; set; }
    }
    [DataContract]
    public class MedpomInData
    {
        public static MedpomInData Get(DataRow row)
        {
            MedpomInData item = new MedpomInData();
            try
            {
                item.log_service_id = Convert.ToInt32(row["log_service_id"]);
                item.familyname = Convert.ToString(row["familyname"]);
                item.firstname = Convert.ToString(row["firstname"]);
                item.patronymic = Convert.ToString(row["patronymic"]);
                item.birthdate = Convert.ToDateTime(row["birthdate"]);
                item.datefrom = Convert.ToDateTime(row["datefrom"]);
                item.dateto = Convert.ToDateTime(row["dateto"]);
                item.unitedpolicynumber = Convert.ToString(row["unitedpolicynumber"]);
                item.snils = Convert.ToString(row["snils"]);
                item.doc_s = Convert.ToString(row["doc_s"]);
                item.doc_n = Convert.ToString(row["doc_n"]);
                return item;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка получения MedpomInData: {ex.Message}", ex);
            }
        }
        [DataMember]
        public int log_service_id { get; set; }
        [DataMember]
        public string familyname { get; set; }
        [DataMember]
        public string firstname { get; set; }
        [DataMember]
        public string patronymic { get; set; }
        [DataMember]
        public DateTime birthdate { get; set; }
        [DataMember]
        public DateTime datefrom { get; set; }
        [DataMember]
        public DateTime dateto { get; set; }
        [DataMember]
        public string unitedpolicynumber { get; set; }
        [DataMember]
        public string snils { get; set; }
        [DataMember]
        public string doc_s { get; set; }
        [DataMember]
        public string doc_n { get; set; }
    }
    [DataContract]
    public class MedpomOutData
    {
        public static List<MedpomOutData> Get(IEnumerable<DataRow> rows)
        {
            return rows.Select(Get).ToList();
        }

        public static MedpomOutData Get(DataRow row)
        {
            MedpomOutData item = new MedpomOutData();
            try
            {
                item.IsMTR = Convert.ToBoolean(row["IsMTR"]);
                item.SLUCH_Z_ID = Convert.ToInt32(row["SLUCH_Z_ID"]);
                item.SLUCH_ID = Convert.ToInt32(row["SLUCH_ID"]);
                if (row["USL_ID"] != DBNull.Value)
                    item.USL_ID = Convert.ToInt32(row["USL_ID"]);
                return item;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Ошибка получения MedpomOutData: {0}", ex.Message), ex);
            }
        }
        [DataMember]
        public bool IsMTR { get; set; }
        [DataMember]
        public int SLUCH_Z_ID { get; set; }
        [DataMember]
        public int SLUCH_ID { get; set; }
        [DataMember]
        public int? USL_ID { get; set; }
    }
    [DataContract]
    public class ReportRow
    {
        public static ReportRow Get(DataRow row)
        {
            try
            {
                var item = new ReportRow();
                if (row["dt"] != DBNull.Value)
                    item.dt = Convert.ToDateTime(row["dt"]);
                item.Count = Convert.ToInt32(row["Count"]);
                item.People = Convert.ToInt32(row["People"]);
                item.USL = Convert.ToInt32(row["USL"]);
                item.Answer = Convert.ToInt32(row["Answer"]);
                item.Error = Convert.ToInt32(row["Error"]);
                item.noAnswer = Convert.ToInt32(row["noAnswer"]);

                return item;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка получения ReportRow: {ex.Message}", ex);
            }
        }
        [DataMember]
        public DateTime? dt { get; set; }
        [DataMember]
        public int Count { get; set; }
        [DataMember]
        public int People { get; set; }
        [DataMember]
        public int USL { get; set; }
        [DataMember]
        public int Answer { get; set; }
        [DataMember]
        public int Error { get; set; }
        [DataMember]
        public int noAnswer { get; set; }
    }
    [DataContract]
    public class FeedBackData
    {
        public FeedBackData(List<FeedBackDataIN> IN)
        {
            this.IN = IN;
        }
        [DataMember]
        public List<FeedBackDataIN> IN { get; set; }

    }
    [DataContract]
    public class FeedBackDataIN
    {
        public static List<FeedBackDataIN> Get(IEnumerable<DataRow> rows)
        {
            return rows.Select(Get).ToList();
        }

        public static FeedBackDataIN Get(DataRow row)
        {
            var item = new FeedBackDataIN();
            try
            {
                item.AppealNumber = Convert.ToString(row["AppealNumber"]);
                item.FeedbackInfo_Id = Convert.ToInt32(row["FeedbackInfo_Id"]);
                item.Log_Service_Id = Convert.ToInt32(row["Log_Service_Id"]);
                item.AppealTopic = Convert.ToString(row["AppealTopic"]);
                item.AppealDetail = Convert.ToString(row["AppealDetail"]);
                item.MedservicesId = Convert.ToString(row["MedservicesId"]);
                item.AppealTopicCode = Convert.ToInt32(row["AppealTopicCode"]);
                item.CareRegimen = Convert.ToString(row["CareRegimen"]);
                item.DateRenderingFrom = Convert.ToDateTime(row["DateRenderingFrom"]);
                item.DateRenderingTo = Convert.ToDateTime(row["DateRenderingTo"]);
                item.RegionCode = Convert.ToString(row["RegionCode"]);
                item.CareType = Convert.ToString(row["CareType"]);
                item.MedservicesSum = Convert.ToDecimal(row["MedservicesSum"]);
                return item;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка получения FeedBackDataIN: {ex.Message}", ex);
            }
        }
        [DataMember]
        public string AppealNumber { get; set; }
        [DataMember]
        public int FeedbackInfo_Id { get; set; }
        [DataMember]
        public int Log_Service_Id { get; set; }
        [DataMember]
        public string AppealTopic { get; set; }
        [DataMember]
        public string AppealDetail { get; set; }
        [DataMember]
        public string MedservicesId { get; set; }
        [DataMember]
        public int AppealTopicCode { get; set; }
        [DataMember]
        public string CareRegimen { get; set; }
        [DataMember]
        public DateTime DateRenderingFrom { get; set; }
        [DataMember]
        public DateTime DateRenderingTo { get; set; }
        [DataMember]
        public string RegionCode { get; set; }
        [DataMember]
        public string CareType { get; set; }
        [DataMember]
        public decimal MedservicesSum { get; set; }

    }
    [DataContract]
    public class STATUS_OUT
    {
        public static List<STATUS_OUT> Get(IEnumerable<DataRow> rows)
        {
            return rows.Select(Get).ToList();
        }

        public static STATUS_OUT Get(DataRow row)
        {
            var item = new STATUS_OUT();
            try
            {
                item.STATUS_OUT_ID = Convert.ToInt32(row["STATUS_OUT_ID"]);
                item.LOG_SERVICE_ID = Convert.ToInt32(row["LOG_SERVICE_ID"]);
                item.DATE_INSERT = Convert.ToDateTime(row["DATE_INSERT"]);
                item.STATUS = MessageLoggerStatusFromString(Convert.ToInt32(row["STATUS"]));
                item.COMMENT = Convert.ToString(row["COMMENT"]);
                return item;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка получения FeedBackDataIN: {ex.Message}", ex);
            }
        }

        private static MessageLoggerStatus MessageLoggerStatusFromString(int value)
        {
            return (MessageLoggerStatus)value;
        }
        [DataMember]
        public int STATUS_OUT_ID { get; set; }
        [DataMember]
        public int LOG_SERVICE_ID { get; set; }
        [DataMember]
        public DateTime DATE_INSERT { get; set; }
        [DataMember]
        public MessageLoggerStatus STATUS { get; set; }
        [DataMember]
        public string COMMENT { get; set; }
    }


}
