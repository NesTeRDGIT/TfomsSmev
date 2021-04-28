using Npgsql;
using SMEV.WCFContract;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmevAdapterService
{
    [ServiceBehavior(
       InstanceContextMode = InstanceContextMode.Single,
       IncludeExceptionDetailInFaults = true,
       ConcurrencyMode = ConcurrencyMode.Multiple
   ),
   ]
    class WcfServer : IWcfInterface
    {
        const string log_name = "SMEV_Service";
        static public void AddLog(string log, EventLogEntryType type)
        {
            try
            {
                EventLog el = new EventLog();
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

        public void SetConfig(Configuration config)
        {
            Config = config;
            Config.Check();
            onChangeConfig();
        }

        List<EntriesMy> IWcfInterface.GetEventLogEntry()
        {
            try
            {
                List<EntriesMy> rez = new List<EntriesMy>();
                if (EventLog.Exists(log_name))
                {
                    EventLog EventLog1 = new System.Diagnostics.EventLog();
                    EventLog1.Source = log_name;

                    for (int i = 0; i < 50; i++)
                    {
                        if (i > EventLog1.Entries.Count - 1)
                            continue;
                        EventLogEntry entry = EventLog1.Entries[EventLog1.Entries.Count - 1 - i];
                        EntriesMy item = new EntriesMy();
                        item.Message = entry.Message;
                        item.TimeGenerated = entry.TimeGenerated;
                        switch (entry.EntryType)
                        {
                            case EventLogEntryType.Error: item.Type = TypeEntries.error; break;
                            case EventLogEntryType.Warning: item.Type = TypeEntries.warning; break;
                            default: item.Type = TypeEntries.message; break;
                        }
                        rez.Add(item);
                    }
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
                List<VSWorkProcess> res = new List<VSWorkProcess>();
                foreach (var t in CurrentWork)
                {
                    res.Add(new VSWorkProcess() { Activ = t.Value.Thread == null ? false : t.Value.Thread.IsAlive, ItSystem = t.Value.Config.ItSystem, VS = t.Value.Config.VS, Text = t.Value.Text });
                }
                return res;
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
            }
        }
        public List<LogRow> GetLog(int Count)
        {
            try
            {
                List<LogRow> rez = new List<LogRow>();
                DataTable tbl = new DataTable();
                NpgsqlDataAdapter oda = new NpgsqlDataAdapter("SELECT * FROM  log_service t order by t.id desc limit "+Count, new NpgsqlConnection(Config.ConnectionString));
                oda.Fill(tbl);
                foreach (DataRow row in tbl.Rows)
                {
                    rez.Add(LogRow.Get(row));
                }
                return rez;
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

}
