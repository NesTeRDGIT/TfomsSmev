using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml.Serialization;

namespace SMEV.WCFContract
{
    [ServiceContract(CallbackContract = typeof(IWcfInterfaceCallback), SessionMode = SessionMode.Required)]
    public interface IWcfInterface
    {
        /// <summary>
        /// Получить список дириктории
        /// </summary>
        /// <param name="path">Путь</param>
        /// <returns>Список директорий</returns>
        [OperationContract]
        string[] GetFolderLocal(string path);
        /// <summary>
        /// Получиться список локальных дисков
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        string[] GetLocalDisk();
        [OperationContract]
        Configuration GetConfig();
        [OperationContract]
        void SetConfig(Configuration config);
        [OperationContract]
        List<EntriesMy> GetEventLogEntry(int Count,bool HideWarning);
        [OperationContract]
        List<VSWorkProcess> GetDoWork();
        [OperationContract]
        List<LogRow> GetLog(int Count, MessageLoggerVS[] VS, DateTime? DATE_B, DateTime? DATE_E);
        [OperationContract]
        MedpomData GetMedpomData(int ID);
        [OperationContract]
        FeedBackData GetFeedBackData(int ID);
        [OperationContract]
        List<ReportRow> GetReport(DateTime DATE_B, DateTime DATE_E);
        [OperationContract]
        bool Ping();
        [OperationContract]
        PingResult PingAdress();
        [OperationContract]
        void PingParamSet(PingConfig PC);
        [OperationContract]
        PingConfig PingParamGet();
        [OperationContract]
        void Register();
        [OperationContract]
        void DeleteLog(int[] IDs);
        [OperationContract]
        void ChangeActivProcess(VS vS, bool v);
        [OperationContract]
        List<STATUS_OUT> GetStatusOut(int ID);
        [OperationContract]
        void Resent(int ID);

    }
    [ServiceContract]
    public interface IWcfInterfaceCallback
    {
        [OperationContract(IsOneWay = true)]
        void PingResult(PingResult PR);

        [OperationContract(IsOneWay = true)]
        void Ping();
    }
    
}
