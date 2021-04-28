using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

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
