using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmevAdapterService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Moq;
using SMEV.WCFContract;
using SmevAdapterService.AdapterLayer.Integration;
using ILogger = SMEV.WCFContract.ILogger;
using System.Xml.Linq;
using SMEV.VS.MedicalCare.newV1_0_0.FeedbackOnMedicalService;

namespace SmevAdapterService.Tests
{
    [TestClass()]
    public class ZAGSProcessTests
    {

        private List<MessageIntegration> CreateMessageList()
        {
            var list = new List<MessageIntegration>();
            list.Add(new MessageIntegration()
            {
                Content = XDocument.Load(@"C:\InputMail\TEST\ZAGS\Новая папка\zags-rogdzproot112-234.0.1\IN\{95c7b910-2496-11ea-a700-2da1baa43ac2}.xml"),
                Key = "95c7b910-2496-11ea-a700-2da1baa43ac2"
            });
            list.Add(new MessageIntegration()
            {
                Content = XDocument.Load(@"C:\InputMail\TEST\ZAGS\Новая папка\zags-rogdzproot112-234.0.1\STATUS\{581bc559-2a95-11ea-9ae4-00155d1d90bf}.xml"),
                Key = "581bc559-2a95-11ea-9ae4-00155d1d90bf"
            });
            list.Add(new MessageIntegration()
            {
                Content = XDocument.Load(@"C:\InputMail\TEST\ZAGS\Новая папка\zags-rogdzproot112-234.0.1\STATUS\{41248eb0-2a95-11ea-bea0-9f1b1ca5b9bf}.xml"),
                Key = "41248eb0-2a95-11ea-bea0-9f1b1ca5b9bf"
            });

            return list;
        }
      
        [TestMethod()]
        public void StartProcessTest()
        {
           var logger = new Mock<ILogger>();
           var repository = new Mock<IRepository>();
           var MessageLogger = new Mock<IMessageLogger>();
           var param = new ProcessObrTaskParam(new Config_VS(), 5);

           var zgProcess = new ZAGSProcess(logger.Object, repository.Object, MessageLogger.Object, param);
           var message = CreateMessageList();
           repository.Setup(x => x.GetMessage()).Returns(()=>
            {
                var item = message.FirstOrDefault();
                if(item!=null)
                {
                    message.Remove(item);
                    return new List<MessageIntegration>() { item };
                }
                zgProcess.StopProcess();
                return new List<MessageIntegration>();
            });


            MessageLogger.Setup(x => x.FindIDByMessageOut(It.IsAny<string>())).Returns(0);

            zgProcess.StartProcess();
            while (zgProcess.IsRunning)
            {
                
            }
            logger.Verify(x=>x.AddLog(It.IsAny<string>(), It.IsIn(LogType.Error)),Times.Never,"В логе ошибка!");
        }

        

    }
}