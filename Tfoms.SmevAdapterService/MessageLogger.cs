using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using SMEV;
using SMEV.WCFContract;

namespace SmevAdapterService
{
    public class SLUCH_REF
    {
        public SLUCH_REF(bool _IsMTR, int _SLUCH_Z_ID, int _SLUCH_ID, int? _USL_ID)
        {
            IsMTR = _IsMTR;
            SLUCH_Z_ID = _SLUCH_Z_ID;
            SLUCH_ID = _SLUCH_ID;
            USL_ID = _USL_ID;

        }
        public bool IsMTR { get; set; }
        public int SLUCH_Z_ID { get; set; }
        public int SLUCH_ID { get; set; }
        public int? USL_ID { get; set; }
    }

    public class Guids
    {
        public string GUID_IN { get; set; }
        public string GUID_OUT { get; set; }
    }
    public interface IMessageLogger
    {
        int AddInputMessage(MessageLoggerVS VS, string id_message_in, MessageLoggerStatus status_in, string orderid, string applicationid, string comment_in = "");
        void SetOutMessage(int ID, string id_message_out, MessageLoggerStatus status_out);
        void SetINMessage(int ID, string id_message_in, MessageLoggerStatus status_in);
        void UpdateStatusIN(int ID, MessageLoggerStatus status_in);
        void UpdateCommentIN(int ID, string comment_in);
        void InsertStatusOut(int log_service_id, MessageLoggerStatus status_out, string comment_out);
        string GetNewGuidOut();
        Guids GetGuids(int log_service_id);
        int? FindIDByMessageOut(string id_message_out);
        void SetMedpomDataIn(int log_service_id, string familyname, string firstname, string patronymic, DateTime birthdate, DateTime datefrom, DateTime dateto, string unitedpolicynumber, string orderId);
        void SetFeedbackINFO(SMEV.VS.MedicalCare.newV1_0_0.FeedbackOnMedicalService.InputData InputData, int log_service_id);
        void SetMedpomDataOut(int log_service_id, List<SLUCH_REF> IDs);
        List<SLUCH_REF> GetMedpomDataOut(int log_service_id);


    }

    public class MessageLogger: IMessageLogger
    {
        string ItSystem;
        string ConnectionString;
        private ILogger logger;
        public MessageLogger(string ConnectionString, string ItSystem, ILogger logger)
        {
            this.ItSystem = ItSystem;
            this.ConnectionString = ConnectionString;
            this.logger = logger;
        }

        private void AddLog(string log, LogType type)
        {
            logger?.AddLog(log, type);
        }
        /// <summary>
        /// Дабавить входящее сообщение
        /// </summary>
        /// <param name="VS"></param>
        /// <param name="ID_MESSAGE"></param>
        /// <param name="status"></param>
        /// <param name="Comment"></param>
        /// <returns></returns>
        public int AddInputMessage(MessageLoggerVS VS, string id_message_in, MessageLoggerStatus status_in, string orderid, string applicationid, string comment_in = "")
        {
            try
            {
                using (var con = new NpgsqlConnection(ConnectionString))
                {
                    using (var cmd = new NpgsqlCommand(@"insert INTO  log_service
                                                    (id_message_in, itsystem, vs, status_in, comment_in, date_in, orderid, applicationid)
                                                    values
                                                    (@id_message_in, @itsystem, @vs, @status_in, @comment_in, @date_in, @orderid, @applicationid) RETURNING id", con))
                    {


                        cmd.Parameters.Add(new NpgsqlParameter("id_message_in", id_message_in));
                        cmd.Parameters.Add(new NpgsqlParameter("itsystem", ItSystem));
                        cmd.Parameters.Add(new NpgsqlParameter("vs", (int)VS));
                        cmd.Parameters.Add(new NpgsqlParameter("status_in", (int)status_in));
                        cmd.Parameters.Add(new NpgsqlParameter("comment_in", comment_in));
                        cmd.Parameters.Add(new NpgsqlParameter("date_in", DateTime.Now));
                        cmd.Parameters.Add(new NpgsqlParameter("orderid", orderid));
                        cmd.Parameters.Add(new NpgsqlParameter("applicationid", applicationid));
                        con.Open();
                        var res = cmd.ExecuteScalar();
                        con.Close();
                        var id = Convert.ToInt32(res);
                        return id;
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog($"Ошибка вставки истории AddInputMessage[ItSystem[{ItSystem}],VS[{VS}],ID_MESSAGE[{id_message_in}]] : {ex.Message}", LogType.Error);
                return -5;
            }
        }
        /// <summary>
        /// Ответ на сообщение
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="id_message_out"></param>
        public void SetOutMessage(int ID, string id_message_out, MessageLoggerStatus status_out)
        {
            UpdateOutMessage(ID, id_message_out);
            InsertStatusOut(ID, MessageLoggerStatus.OUTPUT, "");
        }
        private void UpdateOutMessage(int ID,string id_message_out)
        {
            try
            {
                using (var con = new NpgsqlConnection(ConnectionString))
                {
                    using (var cmd = new NpgsqlCommand(@"update log_service t set id_message_out =  @id_message_out, date_out= @date_out  where t.ID = @ID", con))
                    {
                        cmd.Parameters.Add(new NpgsqlParameter("id_message_out", id_message_out));
                        cmd.Parameters.Add(new NpgsqlParameter("date_out", DateTime.Now));
                        cmd.Parameters.Add(new NpgsqlParameter("ID", ID));
                        con.Open();
                        var x = cmd.ExecuteNonQuery();
                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog($"Ошибка обновления истории SetOutMessage[ID[{ID}],id_message_out[{id_message_out}]] : {ex.Message}", LogType.Error);
            }
        }
        public void SetINMessage(int ID, string id_message_in, MessageLoggerStatus status_in)
        {

            try
            {
                var con = new NpgsqlConnection(ConnectionString);
                var cmd = new NpgsqlCommand(@"update log_service t set id_message_in =  @id_message_in, status_in = @status_in,date_in= @date_in
                                                   where t.ID = @ID", con);
                cmd.Parameters.Add(new NpgsqlParameter("id_message_in", id_message_in));
                cmd.Parameters.Add(new NpgsqlParameter("status_in", (int)status_in));
                cmd.Parameters.Add(new NpgsqlParameter("date_in", DateTime.Now));

                cmd.Parameters.Add(new NpgsqlParameter("ID", ID));
                con.Open();
                var x = cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                AddLog($"Ошибка обновления истории SetOutMessage[ID[{ID}],id_message_out[{status_in}]] : {ex.Message}", LogType.Error);
            }
        }
        public void UpdateStatusIN(int ID, MessageLoggerStatus status_in)
        {
            try
            {
                var con = new NpgsqlConnection(ConnectionString);
                var cmd = new NpgsqlCommand(@"update log_service t set status_in =  @status_in  where t.ID = @ID", con);
                cmd.Parameters.Add(new NpgsqlParameter("status_in", (int)status_in));
                cmd.Parameters.Add(new NpgsqlParameter("ID", ID));
                con.Open();
                var x = cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                AddLog($"Ошибка обновления истории UpdateStatusMessage[ID[{ID}]] : {ex.Message}", LogType.Error);
            }
        }
        public void UpdateCommentIN(int ID, string comment_in)
        {
            try
            {
                var con = new NpgsqlConnection(ConnectionString);

                var cmd = new NpgsqlCommand(@"update log_service t set comment_in =  @comment_in where t.ID = @ID", con);
                cmd.Parameters.Add(new NpgsqlParameter("comment_in", comment_in));
                cmd.Parameters.Add(new NpgsqlParameter("ID", ID));
                con.Open();
                var x = cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                AddLog($"Ошибка обновления истории UpdateStatusMessage[ID[{ID}]] : {ex.Message}", LogType.Error);
            }
        } 
        public void InsertStatusOut(int log_service_id, MessageLoggerStatus status_out,string comment_out)
        {
            NpgsqlTransaction tran = null;
            try
            {
                var con = new NpgsqlConnection(ConnectionString);

                var cmd = new NpgsqlCommand(@"INSERT INTO  status_out (log_service_id, status, comment) VALUES (@log_service_id, @status_out, @comment_out)", con);
                cmd.Parameters.Add(new NpgsqlParameter("log_service_id", log_service_id));
                cmd.Parameters.Add(new NpgsqlParameter("status_out", (int)status_out));
                cmd.Parameters.Add(new NpgsqlParameter("comment_out", comment_out));
                con.Open();
                tran = con.BeginTransaction();
                var x = cmd.ExecuteNonQuery();
                tran.Commit();
                con.Close();
            }
            catch (Exception ex)
            {
                tran?.Rollback();
                AddLog($"Ошибка вставки статуса  InsertStatusOut[ID[{log_service_id}]] : {ex.Message}", LogType.Error);
            }
        }
        public string GetNewGuidOut()
        {
            try
            {
                using (var con = new NpgsqlConnection(ConnectionString))
                {
                    using (var cmd = new NpgsqlCommand(@"select count(*) from log_service t where t.ID_MESSAGE_OUT = @ID_MESSAGE_OUT and t.ItSystem = @ItSystem", con))
                    {
                        cmd.Parameters.Add(new NpgsqlParameter("ID_MESSAGE_OUT", ""));
                        cmd.Parameters.Add(new NpgsqlParameter("ItSystem", ItSystem));
                        var t = "";
                        con.Open();
                        while (true)
                        {
                            t = Guid.NewGuid().ToString();
                            cmd.Parameters["ID_MESSAGE_OUT"].Value = t;
                            var c = cmd.ExecuteScalar();
                            if (Convert.ToInt32(c) == 0)
                                break;
                        }
                        con.Close();
                        return t;
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Ошибка в GetGuidOut: {0}", ex.Message, ex));
            }
        }
        public int? FindIDByMessageOut(string id_message_out)
        {
            try
            {
                using (var con = new NpgsqlConnection(ConnectionString))
                {
                    using (var cmd = new NpgsqlCommand(@"select max(ID) from log_service t 
                                          where t.id_message_out = @ID_MESSAGE_OUT and t.ItSystem = @ItSystem", con))
                    {
                        cmd.Parameters.Add(new NpgsqlParameter("ID_MESSAGE_OUT", id_message_out));
                        cmd.Parameters.Add(new NpgsqlParameter("ItSystem", ItSystem));
                        con.Open();
                        var c = cmd.ExecuteScalar();
                        con.Close();
                        if (c != DBNull.Value)
                            return Convert.ToInt32(c);
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Ошибка в FindIDByMessageOut: {0}", ex.Message, ex));
            }

        }
        public void SetMedpomDataIn(int log_service_id, string familyname, string firstname, string patronymic, DateTime birthdate, DateTime datefrom, DateTime dateto, string unitedpolicynumber, string orderId)
        {
            try
            {
                using (var con = new NpgsqlConnection(ConnectionString))
                {
                    using (var cmd = new NpgsqlCommand(@"INSERT INTO  public.medpom_data_in
(log_service_id,  familyname,  firstname,  patronymic,  birthdate,  datefrom,  dateto,  unitedpolicynumber,orderId)
VALUES (@log_service_id,@familyname, @firstname, @patronymic, @birthdate, @datefrom, @dateto, @unitedpolicynumber,@orderId)", con))
                    {
                        cmd.Parameters.Add(new NpgsqlParameter("log_service_id", log_service_id));
                        cmd.Parameters.Add(new NpgsqlParameter("familyname", string.IsNullOrEmpty(familyname) ? (object)DBNull.Value : familyname));
                        cmd.Parameters.Add(new NpgsqlParameter("firstname", string.IsNullOrEmpty(firstname) ? (object)DBNull.Value : firstname));
                        cmd.Parameters.Add(new NpgsqlParameter("patronymic", string.IsNullOrEmpty(patronymic) ? (object)DBNull.Value : patronymic));
                        cmd.Parameters.Add(new NpgsqlParameter("birthdate", birthdate));
                        cmd.Parameters.Add(new NpgsqlParameter("datefrom", datefrom));
                        cmd.Parameters.Add(new NpgsqlParameter("dateto", dateto));
                        cmd.Parameters.Add(new NpgsqlParameter("unitedpolicynumber", string.IsNullOrEmpty(unitedpolicynumber) ? (object)DBNull.Value : unitedpolicynumber));
                        cmd.Parameters.Add(new NpgsqlParameter("orderId", string.IsNullOrEmpty(orderId) ? (object)DBNull.Value : orderId));

                        con.Open();
                        var c = cmd.ExecuteNonQuery();
                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Ошибка в SetMedpomDataIn: {0}", ex.Message, ex));
            }
        }
        public void SetFeedbackINFO(SMEV.VS.MedicalCare.newV1_0_0.FeedbackOnMedicalService.InputData InputData, int log_service_id)
        {
            try
            {
                using (var con = new NpgsqlConnection(ConnectionString))
                {
                    using (var cmd = new NpgsqlCommand(@"INSERT INTO public.FeedbackINFO(AppealNumber,LOG_SERVICE_ID,AppealTopic,AppealDetail,MedServicesID, AppealTopicCode,CareRegimen,DateRenderingFrom,
                        DateRenderingTo,RegionCode, CareType,MedServicesSum)
                    VALUES(@AppealNumber,@LOG_SERVICE_ID,@AppealTopic,@AppealDetail,@MedServicesID,@AppealTopicCode,@CareRegimen,@DateRenderingFrom,
                           @DateRenderingTo, @RegionCode,@CareType,@MedServicesSum)", con))
                    {
                        cmd.Parameters.Add(new NpgsqlParameter("AppealNumber", NpgsqlDbType.Varchar));
                        cmd.Parameters.Add(new NpgsqlParameter("LOG_SERVICE_ID", log_service_id));
                        cmd.Parameters.Add(new NpgsqlParameter("AppealTopic", NpgsqlDbType.Varchar));
                        cmd.Parameters.Add(new NpgsqlParameter("AppealDetail", NpgsqlDbType.Varchar));
                        cmd.Parameters.Add(new NpgsqlParameter("MedServicesID", NpgsqlDbType.Varchar));
                        cmd.Parameters.Add(new NpgsqlParameter("AppealTopicCode", NpgsqlDbType.Integer));
                        cmd.Parameters.Add(new NpgsqlParameter("CareRegimen", NpgsqlDbType.Varchar));
                        cmd.Parameters.Add(new NpgsqlParameter("DateRenderingFrom", NpgsqlDbType.Date));
                        cmd.Parameters.Add(new NpgsqlParameter("DateRenderingTo", NpgsqlDbType.Date));
                        cmd.Parameters.Add(new NpgsqlParameter("RegionCode", NpgsqlDbType.Varchar));
                        cmd.Parameters.Add(new NpgsqlParameter("CareType", NpgsqlDbType.Varchar));
                        cmd.Parameters.Add(new NpgsqlParameter("MedServicesSum", NpgsqlDbType.Numeric));


                        con.Open();
                        foreach (var item in InputData.InsuredAppealList)
                        {
                            cmd.Parameters["AppealNumber"].Value = item.AppealNumber;
                            cmd.Parameters["AppealTopic"].Value = item.AppealTopic;
                            cmd.Parameters["AppealDetail"].Value = item.AppealDetail;
                            cmd.Parameters["MedServicesID"].Value = item.MedServicesID;
                            cmd.Parameters["AppealTopicCode"].Value = item.AppealTopicCode;
                            cmd.Parameters["CareRegimen"].Value = item.CareRegimen;
                            cmd.Parameters["DateRenderingFrom"].Value = item.DateRenderingFrom;
                            cmd.Parameters["DateRenderingTo"].Value = item.DateRenderingTo;
                            cmd.Parameters["RegionCode"].Value = item.RegionCode;
                            cmd.Parameters["CareType"].Value = item.CareType;
                            cmd.Parameters["MedServicesSum"].Value = item.MedServicesSum;
                            cmd.ExecuteNonQuery();
                        }
                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка в SetFeedbackINFO: {ex.Message}");
            }
        }       
        public void SetMedpomDataOut(int log_service_id, List<SLUCH_REF> IDs)
        {
            try
            {
                using (var con = new NpgsqlConnection(ConnectionString))
                {
                    using (var cmd = new NpgsqlCommand(@"INSERT INTO public.medpom_data_out
(log_service_id,  sluch_z_id,  sluch_id,  usl_id, ismtr)
VALUES 
(@log_service_id,@sluch_z_id,@sluch_id,@usl_id, @ismtr)", con))
                    {
                        cmd.Parameters.Add(new NpgsqlParameter("log_service_id", log_service_id));
                        cmd.Parameters.Add(new NpgsqlParameter("sluch_z_id", 1));
                        cmd.Parameters.Add(new NpgsqlParameter("sluch_id", 1));
                        cmd.Parameters.Add(new NpgsqlParameter("usl_id", DBNull.Value));
                        cmd.Parameters.Add(new NpgsqlParameter("ismtr", DBNull.Value));

                        con.Open();
                        foreach (var id in IDs)
                        {
                            cmd.Parameters["ismtr"].Value = id.IsMTR;
                            cmd.Parameters["sluch_z_id"].Value = id.SLUCH_Z_ID;
                            cmd.Parameters["sluch_id"].Value = id.SLUCH_ID;
                            cmd.Parameters["usl_id"].Value = id.USL_ID ?? (object)DBNull.Value;
                            var c = cmd.ExecuteNonQuery();
                        }



                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Ошибка в SetMedpomDataOut: {0}", ex.Message, ex));
            }
        }
        public Guids GetGuids(int log_service_id)
        {
            try
            {
                using (var con = new NpgsqlConnection(ConnectionString))
                {
                    using (var oda = new NpgsqlDataAdapter(@"select id_message_out, id_message_in from log_service t where t.ID = @log_service_id", con))
                    {
                        oda.SelectCommand.Parameters.Add(new NpgsqlParameter("log_service_id", log_service_id));
                        var tbl = new DataTable();
                        oda.Fill(tbl);
                        if (tbl.Rows.Count != 0)
                            return new Guids { GUID_IN = tbl.Rows[0]["id_message_in"].ToString(), GUID_OUT = tbl.Rows[0]["id_message_out"].ToString(), };
                        return null;
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Ошибка в GetGuids: {0}", ex.Message, ex));
            }
        }

        public List<SLUCH_REF> GetMedpomDataOut(int log_service_id)
        {
            try
            {
                using (var con = new NpgsqlConnection(ConnectionString))
                {
                    using (var oda = new NpgsqlDataAdapter(@"select sluch_z_id,  sluch_id,  usl_id, ismtr from medpom_data_out t where t.log_service_id = @log_service_id", con))
                    {
                        oda.SelectCommand.Parameters.Add(new NpgsqlParameter("log_service_id", log_service_id));
                        var tbl = new DataTable();
                        oda.Fill(tbl);
                        var res = new List<SLUCH_REF>();
                        foreach(DataRow row in tbl.Rows)
                        {
                            var sr = new SLUCH_REF(Convert.ToBoolean(row["ismtr"]), Convert.ToInt32(row["sluch_z_id"]), Convert.ToInt32(row["sluch_id"]), row["usl_id"] != DBNull.Value ? Convert.ToInt32(row["USL_ID"]) :(int?)null);
                            res.Add(sr);
                        }
                        return res;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Ошибка в GetGuids: {0}", ex.Message, ex));
            }
        }
    }
}
