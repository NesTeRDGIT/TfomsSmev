using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using SMEV;
using SMEV.WCFContract;
using SMEV.VS.MedicalCare.newV1_0_0.FeedbackOnMedicalService;

namespace SmevAdapterService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,IncludeExceptionDetailInFaults = true,ConcurrencyMode = ConcurrencyMode.Multiple)]
    class WcfServer : IWcfInterface
    {
        private ILogger logger;
        private IProcess process;
        private IConfigurationManager ConfigurationManager { get; set; }
        private IPingManager pingManager;
      
        private void AddLog(string log, LogType type)
        {
            logger?.AddLog(log, type);
        }
       
        public WcfServer(ILogger logger, IProcess process, IConfigurationManager ConfigurationManager, IPingManager pingManager)
        {
            this.logger = logger;
            this.process = process;
            this.ConfigurationManager = ConfigurationManager;
            this.pingManager = pingManager;
        }

     
        public Configuration GetConfig()
        {
            return ConfigurationManager.config;
        }

 
        public void SetConfig(Configuration config)
        {
            ConfigurationManager.config = config;
            ConfigurationManager.config.Check();
            ConfigurationManager.Save();
            AddLog("Перезапуск Конфигурации(изменение)", LogType.Information);
            process.StopProcess();
            process.StartProcess(ConfigurationManager.config);
            AddLog("Конфигурация запущена", LogType.Information);
        }

        List<EntriesMy> IWcfInterface.GetEventLogEntry(int Count, bool HideWarning)
        {
            try
            {
                return logger?.GetLog(Count, HideWarning);
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
                foreach (var t in process.CurrentWork)
                {
                    var value = t.Value;
                    res.Add(new VSWorkProcess { Activ = value.isActiv, ItSystem = value.ItSystem, VS = value.VS, Text = value.Text });
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
                var cmd = new NpgsqlCommand("SELECT * FROM public.getlog_service(@Count, @VS, @DATE_B, @DATE_E)", new NpgsqlConnection(ConfigurationManager.config.ConnectionString));
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
            using (var con = new NpgsqlConnection(ConfigurationManager.config.ConnectionString))
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
            using (var con = new NpgsqlConnection(ConfigurationManager.config.ConnectionString))
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
                var cmd = new NpgsqlCommand("SELECT * FROM public.report_mp(@DATE_B, @DATE_E)", new NpgsqlConnection(ConfigurationManager.config.ConnectionString));
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
                res = pingManager.Ping();
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
                using (var CONN = new NpgsqlConnection(ConfigurationManager.config.ConnectionString))
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
            var VS = ConfigurationManager.config.ListVS.FirstOrDefault(x => x.VS == vS);
            if (VS != null)
            {
                VS.isEnabled = v;
                process.StopProcess();
                process.StartProcess(ConfigurationManager.config);
                ConfigurationManager.Save();
            }
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
            AddLog("Перезапуск PING:", LogType.Information);
            pingManager.Stop();
            pingManager.config = PC;
            pingManager.SaveConfig();
            pingManager.Start();
        }

        public PingConfig PingParamGet()
        {
            return pingManager.config;

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

   public class ProcessObrTask
   {
       public ProcessObrTask(Task Task, CancellationTokenSource Cancel, ProcessObrTaskParam Param)
       {
           this.Cancel = Cancel;
           this.Task = Task;
           this.Param = Param;
       }

       public CancellationTokenSource Cancel { get; set; }
       public Task Task { get; set; }
       public ProcessObrTaskParam Param { get; set; }
       public bool isActiv => this.Task != null && this.Task.Status == TaskStatus.Running;
       public string ItSystem => Param?.Config?.ItSystem;
       public string Text => Param?.Text;
       public SMEV.WCFContract.VS VS => Param.Config.VS;
   }

   public class ProcessObrTaskParam
   {
       public ProcessObrTaskParam( Config_VS con, int _TimeOut)
       {
           Config = con;
           TimeOut = _TimeOut;
       }
       public Config_VS Config { get; set; }
       public string Text { get; set; }
       public int TimeOut { get; set; }



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
