using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMEV.WCFContract
{
    public interface ILogger
    {
        void AddLog(string log, LogType type);
        List<EntriesMy> GetLog(int Count, bool HideWarning); 
        void Clear();
    }
    public enum TypeEntries
    {
        message = 0,
        error = 1,
        warning = 2
    }
    /// <summary>
    /// Мой Entries упрощеный
    /// </summary>
    public class EntriesMy
    {
        public DateTime TimeGenerated { get; set; }
        public string Message { get; set; }
        public TypeEntries Type { get; set; }
    }

    public enum LogType
    {
        Error = 0,
        Information = 1,
        Warning = 2
    }
}
