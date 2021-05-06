using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMEV;
using SMEV.WCFContract;

namespace SmevAdapterService
{
  
    public class LoggerEventLog : ILogger
    {
        private readonly string nameLog;

        public LoggerEventLog(string nameLog)
        {
            this.nameLog = nameLog;
        }
        public void AddLog(string log, LogType type)
        {
            try
            {
                var el = GetLog();
                el.WriteEntry(log, LogTypeToLogEntryType(type));
            }

            catch
            {
                // ignored
            }
        }

        private EventLog GetLog()
        {
            if (!EventLog.SourceExists(nameLog))
            {
                EventLog.CreateEventSource(nameLog, nameLog);
            }
            return new EventLog { Source = nameLog };
        }

        public void Clear()
        {
            var EventLog = GetLog();
            EventLog.Clear();
        }

        private EventLogEntryType LogTypeToLogEntryType(LogType lt)
        {
            switch (lt)
            {
                case LogType.Error: return EventLogEntryType.Error;
                case LogType.Information: return EventLogEntryType.Information;
                case LogType.Warning: return EventLogEntryType.Warning;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lt), lt, null);
            }
        }

        public List<EntriesMy> GetLog(int Count, bool HideWarning)
        {
            var res = new List<EntriesMy>();
            var EventLog1 = GetLog();

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
                res.Add(item);
                if (res.Count >= Count)
                    break; 
            }
            return res;
        }
    }
}
