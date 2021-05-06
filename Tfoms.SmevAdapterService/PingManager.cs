using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SMEV.WCFContract;
using System.Management;

namespace SmevAdapterService
{
    public class PingManager : IPingManager
    {
        public PingConfig config { get; set; } = new PingConfig();
        private string path_config;
        private Action<PingResult> onResult;

        private ILogger logger;
        private void AddLog(string log, LogType type)
        {
            logger?.AddLog(log, type);
        }
        public PingManager(string path_config, Action<PingResult> onResult, ILogger logger)
        {
            this.logger = logger;
            this.onResult = onResult;
            this.path_config = path_config;
        }
     

        private Task PingTask = null;
        private CancellationTokenSource CTS;


        public void LoadConfig()
        {
            config = PingConfig.LoadFromFile(path_config);
        }

        public void SaveConfig()
        {
            PingConfig.SaveToFile(path_config, config);
        }

        public void Start()
        {
            Stop();
            if (config.IsEnabled)
            {
                AddLog("Запуск PING", LogType.Information);
                CTS = new CancellationTokenSource();
                PingTask = new Task(()=> PingWork(CTS.Token));
                PingTask.Start();
                AddLog("PING запущен", LogType.Information);
            }
            else
            {
                AddLog("PING отключен", LogType.Information);
            }
        }

        public void Stop()
        {
            CTS?.Cancel();
        }
        private void PingWork(CancellationToken cancel)
        {
            try
            {
                while (!cancel.IsCancellationRequested)
                {
                    var res = Ping();
                    if (!res.Result)
                        onResult?.Invoke(res);
                    Delay(config.TimeOut * 60 * 1000);
                }
            }
            catch (Exception e)
            {
                AddLog($"Ошибка в PingThread:{e.Message}", LogType.Error);
            }
        }

        private void Delay(int MS)
        {
            var t = Task.Delay(MS);
            t.Wait();
        }

        public PingResult Ping()
        {
            var res = new PingResult();
            try
            {
                res.Adress = config.Adress;
                var ping = new System.Net.NetworkInformation.Ping();
                var result = false;
                for (var i = 0; i < 4; i++)
                {
                    var pingReply = ping.Send(config.Adress);
                    if (pingReply != null && pingReply.Status == System.Net.NetworkInformation.IPStatus.Success)
                    {
                        result = true;
                    }
                }
                res.Result = result;

                if (config.Process != null)
                {
                    var procList = GetProcessPath().ToArray();
                    foreach (var proc in config.Process)
                    {
                        if (procList.Count(x => string.Equals(x, proc, StringComparison.CurrentCultureIgnoreCase)) == 0)
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
        private List<string> GetProcessPath()
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
            catch (Exception)
            {
                return Result;
            }

        }

    }
}
