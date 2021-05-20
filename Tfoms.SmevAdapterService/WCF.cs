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
using SmevAdapterService.VS;

namespace SmevAdapterService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,IncludeExceptionDetailInFaults = true,ConcurrencyMode = ConcurrencyMode.Multiple)]
    class WcfServer : IWcfInterface
    {
        private ILogger logger;
        private IProcesor process;
        private IConfigurationManager ConfigurationManager { get; set; }
        private IPingManager pingManager;
        private IDBManager dbManager { get; set; }


        private void AddLog(string log, LogType type)
        {
            logger?.AddLog(log, type);
        }
       
        public WcfServer(ILogger logger, IProcesor process, IConfigurationManager ConfigurationManager, IPingManager pingManager, IDBManager dbManager)
        {
            this.logger = logger;
            this.process = process;
            this.ConfigurationManager = ConfigurationManager;
            this.pingManager = pingManager;
            this.dbManager = dbManager;
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
            dbManager.ChangeConnectionString(ConfigurationManager.config.ConnectionString);
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
                    res.Add(new VSWorkProcess { Activ = value.IsRunning, ItSystem = value.ItSystem, VS = value.VS, Text = value.Text });
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
                return dbManager.GetLogMessage(null,Count, VS, DATE_B, DATE_E);
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }
        public void  Resent(int ID)
        {
            try
            {
               var logs=  dbManager.GetLogMessage(ID, 1, null, null, null);
                var item = logs.FirstOrDefault();
                if (item == null)
                    throw new Exception("Не удалось найти сообщение");
                if(item.VS != MessageLoggerVS.InputData)
                    throw new Exception("Повтор отправки доступен только для InputData");
                var p = process.CurrentWork.FirstOrDefault(x => x.Key == SMEV.WCFContract.VS.MP);
                p.Value.Resent(ID);
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
            return dbManager.GetMedpomData(ID);
        }
        public FeedBackData GetFeedBackData(int ID)
        {
            return dbManager.GetFeedBackData(ID);
        }
        public List<ReportRow> GetReport(DateTime DATE_B, DateTime DATE_E)
        {
            try
            {
                return dbManager.GetReport(DATE_B, DATE_E);
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
                 dbManager.DeleteLog(IDs);
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }

        public void ChangeActivProcess(SMEV.WCFContract.VS vS, bool v)
        {
            var VS = ConfigurationManager.config.ListVS.FirstOrDefault(x => x.VS == vS);
            var proc = process.CurrentWork.ToArray().Where(x => x.Key == vS).Select(x=>x.Value).FirstOrDefault();
            if (VS != null && proc!=null)
            {
                VS.isEnabled = v;
                if(v)
                    proc.StartProcess();
                else
                    proc.StopProcess();
                ConfigurationManager.Save();
            }
        }

        public List<STATUS_OUT> GetStatusOut(int ID)
        {
            try
            {
                return dbManager.GetStatusOut(ID);
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
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
                catch (Exception)
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
