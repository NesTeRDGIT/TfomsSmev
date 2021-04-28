using System;
using System.IO;
using System.ServiceProcess;

namespace SmevAdapterService
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        static void Main()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new AdapterService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
