using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmevAdapterService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Moq;
using SMEV.WCFContract;
using SmevAdapterService.AdapterLayer.Integration;

namespace SmevAdapterService.Tests
{
    [TestClass()]
    public class MPProcessTests
    {
        private List<MessageIntegration> CreateMessageList()
        {
            var list = new List<MessageIntegration>();
            list.Add(new MessageIntegration()
            {
                Content = XDocument.Load(@"C:\InputMail\TEST\{5ee9fd39-ab8e-11ea-8968-005056933ff3}.xml"),
                Key = "5ee9fd39-ab8e-11ea-8968-005056933ff3"
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
            var mpAnswer = new Mock<IMPAnswer>();

            var zgProcess = new MPProcess(logger.Object, repository.Object, MessageLogger.Object, param, mpAnswer.Object);
            var message = CreateMessageList();
            repository.Setup(x => x.GetMessage()).Returns(() =>
            {
                var item = message.FirstOrDefault();
                if (item != null)
                {
                    message.Remove(item);
                    return new List<MessageIntegration>() { item };
                }
                zgProcess.StopProcess();
                return new List<MessageIntegration>();
            });

            mpAnswer.Setup(x => x.GetData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).
                Returns(new List<V_MEDPOM_SMEV3Row> {new V_MEDPOM_SMEV3Row()});


            MessageLogger.Setup(x => x.FindIDByMessageOut(It.IsAny<string>())).Returns(0);

            zgProcess.StartProcess();
            while (zgProcess.IsRunning)
            {

            }
            logger.Verify(x => x.AddLog(It.IsAny<string>(), It.IsIn(LogType.Error)), Times.Never, "В логе ошибка!");
        }
    }
}