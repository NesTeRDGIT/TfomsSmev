using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.ServiceModel;
using System.Threading;
using Npgsql;
using NpgsqlTypes;
using SMEV.WCFContract;
using SMEV.VS.MedicalCare.newV1_0_0.FeedbackOnMedicalService;

namespace SmevAdapterService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,IncludeExceptionDetailInFaults = true,ConcurrencyMode = ConcurrencyMode.Multiple)]
    class WcfServer : IWcfInterface
    {
        const string log_name = "SMEV_Service";
        public static void AddLog(string log, EventLogEntryType type)
        {
            try
            {
                var el = new EventLog();
                if (!EventLog.SourceExists(log_name))
                {
                    EventLog.CreateEventSource(log_name, log_name);
                }
                el.Source = log_name;
                el.WriteEntry(log, type);
            }

            catch { }
        }
        public Configuration Config { get; set; }
        public PingConfig Config_Ping { get; set; }
        public Dictionary<SMEV.WCFContract.VS, ProcessObr> CurrentWork { get; set; } = new Dictionary<SMEV.WCFContract.VS, ProcessObr>();
        public WcfServer(Configuration conf)
        {
            Config = conf;
        }
        public Configuration GetConfig()
        {
            return Config;
        }

        public delegate void ChangeConfig();
        public event ChangeConfig onChangeConfig;
        public event ChangeConfig onChangeConfig_PING;

        public void SetConfig(Configuration config)
        {
            Config = config;
            Config.Check();
            onChangeConfig?.Invoke();
        }

        List<EntriesMy> IWcfInterface.GetEventLogEntry(int Count, bool HideWarning)
        {
            try
            {
                var rez = new List<EntriesMy>();
                if (!EventLog.Exists(log_name)) return rez;
                var EventLog1 = new EventLog {Source = log_name};

                for (var i = EventLog1.Entries.Count - 1; i >= 0; i--)
                {
                    var entry = EventLog1.Entries[i];
                    if (HideWarning && entry.EntryType == EventLogEntryType.Warning)
                        continue;
                    var item = new EntriesMy { Message = entry.Message, TimeGenerated = entry.TimeGenerated };
                    switch (entry.EntryType)
                    {
                        case EventLogEntryType.Error: item.Type = TypeEntries.error; break;
                        case EventLogEntryType.Warning: item.Type = TypeEntries.warning; break;
                        default: item.Type = TypeEntries.message; break;
                    }
                    rez.Add(item);
                    if(rez.Count>=Count)
                        break;;
                }
                return rez;
            }
            catch(Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }

        public List<VSWorkProcess> GetDoWork()
        {
            try
            {
                var res = new List<VSWorkProcess>();
                foreach (var t in CurrentWork)
                {
                    res.Add(new VSWorkProcess { Activ = t.Value.Thread?.IsAlive ?? false, ItSystem = t.Value.Config.ItSystem, VS = t.Value.Config.VS, Text = t.Value.Text });
                }
                return res;
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }
        public List<LogRow> GetLog(int Count, MessageLoggerVS[] VS, DateTime? DATE_B, DateTime? DATE_E)
        {
            try
            {
               
                var tbl = new DataTable();
                var cmd = new NpgsqlCommand("SELECT * FROM public.getlog_service(@Count, @VS, @DATE_B, @DATE_E)", new NpgsqlConnection(Config.ConnectionString));
                cmd.Parameters.Add(new NpgsqlParameter("Count", Count));

                var vs_par = new NpgsqlParameter("VS", NpgsqlDbType.Array | NpgsqlDbType.Numeric);
                vs_par.Value = VS!=null? (object)VS.Select(x=>(int)x).ToArray(): DBNull.Value;              
                cmd.Parameters.Add(vs_par);

                cmd.Parameters.Add(new NpgsqlParameter("DATE_B", DATE_B.HasValue? (object)DATE_B.Value.Date: DBNull.Value));
                cmd.Parameters.Add(new NpgsqlParameter("DATE_E", DATE_E.HasValue ? (object)DATE_E.Value.Date : DBNull.Value));
                var oda = new NpgsqlDataAdapter(cmd);
                oda.Fill(tbl);
                return LogRow.Get(tbl.Select());
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }

        public string[] GetFolderLocal(string path)
        {
            try
            {
                return Directory.GetDirectories(path);
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }
        public string[] GetFilesLocal(string path, string pattern)
        {
            try
            {
                return Directory.GetFiles(path, pattern);
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }
        public string[] GetLocalDisk()
        {
            return Environment.GetLogicalDrives();
        }
        public MedpomData GetMedpomData(int ID)
        {
            using (var con = new NpgsqlConnection(Config.ConnectionString))
            {
                var oda = new NpgsqlDataAdapter(@"SELECT * FROM  public.medpom_data_out t  where log_service_id = @log_service_id", con);
                oda.SelectCommand.Parameters.Add(new NpgsqlParameter("log_service_id", ID));
                var outTable = new DataTable();
                oda.Fill(outTable);

                oda = new NpgsqlDataAdapter(@"SELECT * FROM  public.medpom_data_in where  log_service_id = @log_service_id", con);
                oda.SelectCommand.Parameters.Add(new NpgsqlParameter("log_service_id", ID));
                var inTable = new DataTable();
                oda.Fill(inTable);
     
                return new MedpomData(inTable.Rows.Count!=0? MedpomInData.Get(inTable.Rows[0]) : new MedpomInData(), MedpomOutData.Get(outTable.Select()));
            }
        }
        public FeedBackData GetFeedBackData(int ID)
        {
            using (var con = new NpgsqlConnection(Config.ConnectionString))
            {
                var oda = new NpgsqlDataAdapter(@"SELECT * FROM public.feedbackinfo where  log_service_id = @log_service_id", con);
                oda.SelectCommand.Parameters.Add(new NpgsqlParameter("log_service_id", ID));
                var inTable = new DataTable();
                oda.Fill(inTable);


                return new FeedBackData(FeedBackDataIN.Get(inTable.Select()));
            }
        }
        public List<ReportRow> GetReport(DateTime DATE_B, DateTime DATE_E)
        {
            try
            {
                var rez = new List<ReportRow>();
                var tbl = new DataTable();
                var cmd = new NpgsqlCommand("SELECT * FROM public.report_mp(@DATE_B, @DATE_E)", new NpgsqlConnection(Config.ConnectionString));
                cmd.Parameters.Add(new NpgsqlParameter("DATE_B", NpgsqlDbType.Date) { Value = DATE_B.Date });
                cmd.Parameters.Add(new NpgsqlParameter("DATE_E", NpgsqlDbType.Date) { Value = DATE_E.Date });
                var oda = new NpgsqlDataAdapter(cmd);
                oda.Fill(tbl);
                foreach (DataRow row in tbl.Rows)
                {
                    rez.Add(ReportRow.Get(row));
                }
                return rez;
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }
        public PingResult PingAdress()
        {
            var res = new PingResult();
            try
            {
                res.Adress = Config_Ping.Adress;

                var ping = new System.Net.NetworkInformation.Ping();
                var result = false;
                for (var i = 0; i < 4; i++)
                {
                    var pingReply = ping.Send(Config_Ping.Adress);
                    if (pingReply != null && pingReply.Status == System.Net.NetworkInformation.IPStatus.Success)
                    {
                        result = true;
                    }
                }
                res.Result = result;

                if (Config_Ping.Process != null)
                {
                    var procList = GetProcessPath().ToArray();
                    foreach (var proc in Config_Ping.Process)
                    {
                        if (procList.Count(x =>string.Equals(x, proc, StringComparison.CurrentCultureIgnoreCase))==0)
                        {
                            res.Result = false;
                            res.Text += $"Процесс '{proc}' не запущен;";
                        }
                    }
                }

            }
            catch (Exception e)
            {
                res.Result = false;
                res.Text = e.FullMessage();
            }
            return res;
        }


        public static List<string> GetProcessPath()
        {
            var Result = new List<string>();
            try
            {
                var Query = "SELECT ProcessId, ExecutablePath, CommandLine FROM Win32_Process";
                using (var mos = new ManagementObjectSearcher(Query))
                {
                    using (var moc = mos.Get())
                    {
                        var query = from p in Process.GetProcesses()
                            join mo in moc.Cast<ManagementObject>()
                                on p.Id equals (int)(uint)mo["ProcessId"]
                            select new
                            {
                                Process = p,
                                Path = (string)mo["ExecutablePath"],
                                CommandLine = (string)mo["CommandLine"],
                            };
                        foreach (var item in query)
                        {
                            Result.Add(item.Path);
                        }

                        return Result;
                    }
                }

            }
            catch(Exception ex)
            {
                return Result;
            }

        }
        List<IWcfInterfaceCallback> clientList = new List<IWcfInterfaceCallback>();
        public void Register()
        {
            CheckClient();
            var callback = OperationContext.Current.GetCallbackChannel<IWcfInterfaceCallback>();
            clientList.Add(callback);
        }

        public void DeleteLog(int[] IDs)
        {
            try
            {
                using (var CONN = new NpgsqlConnection(Config.ConnectionString))
                {
                    CONN.Open();
                    using (var TRAN = CONN.BeginTransaction())
                    {
                        try
                        {
                            foreach (var ID in IDs)
                            {
                                var cmd = new NpgsqlCommand("SELECT public.DELETE_LOG(@ID)", CONN);
                            
                                cmd.Parameters.Add(new NpgsqlParameter("ID", ID));
                                cmd.ExecuteNonQuery();
                            }
                            TRAN.Commit();
                            
                        }
                        catch (Exception)
                        {
                            TRAN.Rollback();
                            throw;
                        }
                    }
                    CONN.Close();
                }
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }

        public void ChangeActivProcess(SMEV.WCFContract.VS vS, bool v)
        {
            var VS = Config.ListVS.First(x => x.VS == vS);
            VS.isEnabled = v;
            onChangeConfig?.Invoke();
        }


        private void CheckClient()
        {
            var removeCl = new List<IWcfInterfaceCallback>();
            foreach (var cal in clientList)
            {
                try
                {
                    cal.Ping();
                }
                catch (Exception ex)
                {
                    removeCl.Add(cal);
                }
            }

            foreach (var cal in removeCl)
            {
                clientList.Remove(cal);
            }
        }


        public void PingParamSet(PingConfig PC)
        {
            Config_Ping = PC;
            onChangeConfig_PING?.Invoke();
        }

        public PingConfig PingParamGet()
        {
            return Config_Ping;

        }


        public void SEND_PING_RESULT(PingResult PR)
        {
            try
            {
                CheckClient();
                foreach (var cal in clientList)
                {
                    try
                    {
                        cal.PingResult(PR);
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }
                }
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }


        public bool Ping()
        {
            return true;
        }



    }

    class ProcessObr
    {
        public ProcessObr(Thread th, CancellationTokenSource can, Config_VS con, int _TimeOut)
        {
            Thread = th;
            Cancel = can;
            Config = con;
            TimeOut = _TimeOut;
        }
        public Thread Thread { get; set; }
        public CancellationTokenSource Cancel { get; set; }

        public Config_VS Config { get; set; }
        public string Text { get; set; }

        public int TimeOut { get; set; } = 1000;
    }


    public static class EXT
    {
        public static string FullMessage(this Exception ex)
        {
            if (ex == null) return "";
            var add = ex.InnerException.FullMessage();
            return ex.Message + (string.IsNullOrEmpty(add) ? "" : $";{add}");
        }
    }
}
