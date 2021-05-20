using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using SMEV.WCFContract;
using SmevAdapterService.VS;

namespace SmevAdapterService
{
    public interface IDBManager
    {
        void ChangeConnectionString(string ConnectionString);
        List<LogRow> GetLogMessage(int? ID, int Count, MessageLoggerVS[] VS, DateTime? DATE_B, DateTime? DATE_E);
        MedpomData GetMedpomData(int ID);
        FeedBackData GetFeedBackData(int ID);
        List<ReportRow> GetReport(DateTime DATE_B, DateTime DATE_E);

        void DeleteLog(int[] IDs);
        List<STATUS_OUT> GetStatusOut(int ID);
    }

    public class PGManager : IDBManager
    {
        private string ConnectionString;

        public PGManager(string ConnectionString)
        {
            ChangeConnectionString(ConnectionString);
        }


        public void ChangeConnectionString(string ConnectionString)
        {
            this.ConnectionString = ConnectionString;
        }

        public List<LogRow> GetLogMessage(int? ID, int Count, MessageLoggerVS[] VS, DateTime? DATE_B, DateTime? DATE_E)
        {
            using (var con = new NpgsqlConnection(ConnectionString))
            {
                using (var cmd = new NpgsqlCommand("SELECT * FROM public.getlog_service(@ID,@Count, @VS, @DATE_B, @DATE_E)", con))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("ID", (object)ID ?? DBNull.Value));
                    cmd.Parameters.Add(new NpgsqlParameter("Count", Count));
                    var vs_par = new NpgsqlParameter("VS", NpgsqlDbType.Numeric | NpgsqlDbType.Array);
                    vs_par.Value = VS != null ? (object)VS.Select(x => (int)x).ToArray() : DBNull.Value;
                    cmd.Parameters.Add(vs_par);
                    cmd.Parameters.Add(new NpgsqlParameter("DATE_B", DATE_B.HasValue ? (object)DATE_B.Value.Date : DBNull.Value));
                    cmd.Parameters.Add(new NpgsqlParameter("DATE_E", DATE_E.HasValue ? (object)DATE_E.Value.Date : DBNull.Value));
                    var oda = new NpgsqlDataAdapter(cmd);
                    var tbl = new DataTable();
                    oda.Fill(tbl);
                    return LogRow.Get(tbl.Select());
                }
            }
        }

        public MedpomData GetMedpomData(int ID)
        {
            using (var con = new NpgsqlConnection(ConnectionString))
            {
                using (var oda_out = new NpgsqlDataAdapter(@"SELECT * FROM  public.medpom_data_out t  where log_service_id = @log_service_id", con))
                {
                    oda_out.SelectCommand.Parameters.Add(new NpgsqlParameter("log_service_id", ID));
                    var outTable = new DataTable();
                    oda_out.Fill(outTable);
                    using (var oda_in = new NpgsqlDataAdapter(@"SELECT * FROM  public.medpom_data_in where  log_service_id = @log_service_id", con))
                    {
                        oda_in.SelectCommand.Parameters.Add(new NpgsqlParameter("log_service_id", ID));
                        var inTable = new DataTable();
                        oda_in.Fill(inTable);
                        return new MedpomData(inTable.Rows.Count != 0 ? MedpomInData.Get(inTable.Rows[0]) : new MedpomInData(), MedpomOutData.Get(outTable.Select()));
                    }
                }
            }
        }

        public FeedBackData GetFeedBackData(int ID)
        {
            using (var con = new NpgsqlConnection(ConnectionString))
            {
                using (var oda = new NpgsqlDataAdapter(@"SELECT * FROM public.feedbackinfo where  log_service_id = @log_service_id", con))
                {
                    oda.SelectCommand.Parameters.Add(new NpgsqlParameter("log_service_id", ID));
                    var inTable = new DataTable();
                    oda.Fill(inTable);
                    return new FeedBackData(FeedBackDataIN.Get(inTable.Select()));
                }
            }
        }

        public List<ReportRow> GetReport(DateTime DATE_B, DateTime DATE_E)
        {
            using (var con = new NpgsqlConnection(ConnectionString))
            {
                using (var oda = new NpgsqlDataAdapter("SELECT * FROM public.report_mp(@DATE_B, @DATE_E)", con))
                {
                    oda.SelectCommand.Parameters.Add(new NpgsqlParameter("DATE_B", NpgsqlDbType.Date) { Value = DATE_B.Date });
                    oda.SelectCommand.Parameters.Add(new NpgsqlParameter("DATE_E", NpgsqlDbType.Date) { Value = DATE_E.Date });
                    var tbl = new DataTable();
                    oda.Fill(tbl);
                    return (from DataRow row in tbl.Rows select ReportRow.Get(row)).ToList();
                }
            }
        }

        public void DeleteLog(int[] IDs)
        {
            using (var con = new NpgsqlConnection(ConnectionString))
            {
                con.Open();
                using (var TRAN = con.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new NpgsqlCommand("SELECT public.DELETE_LOG(@ID)", con))
                        {
                            cmd.Parameters.Add(new NpgsqlParameter("ID", -1));
                            foreach (var ID in IDs)
                            {
                                cmd.Parameters["ID"].Value = ID;
                                cmd.ExecuteNonQuery();
                            }
                        }
                        TRAN.Commit();
                    }
                    catch (Exception)
                    {
                        TRAN.Rollback();
                        throw;
                    }
                }
                con.Close();
            }
        }

        public List<STATUS_OUT> GetStatusOut(int ID)
        {
            using (var con = new NpgsqlConnection(ConnectionString))
            {
                using (var oda = new NpgsqlDataAdapter("SELECT * FROM public.status_out where log_service_id = @log_service_id order by date_insert desc", con))
                {
                    oda.SelectCommand.Parameters.Add(new NpgsqlParameter("log_service_id", ID));
                    var tbl = new DataTable();
                    oda.Fill(tbl);
                    return STATUS_OUT.Get(tbl.Select());
                }
            }
        }
    }
}
