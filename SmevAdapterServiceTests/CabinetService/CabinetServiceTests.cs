using Microsoft.VisualStudio.TestTools.UnitTesting;
using SMEV.WCFContract;
using SmevAdapterService.CabinetService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CabinetContract;
using Castle.Components.DictionaryAdapter;
using Moq;
using SmevAdapterService.AdapterLayer.Integration;

namespace SmevAdapterService.CabinetService.Tests
{
    [TestClass()]
    public class CabinetServiceTests
    {
        private Person person = new Person()
        {
            FAM = "ЕМЕЛИН",
            IM = "ИЛЬЯ",
            OT = "НИКОЛАЕВИЧ",
            DR = new DateTime(1964, 2, 28),
            ENP = "3210987654321098",
            W = 1
        };
        [TestMethod()]
        public void GetDataTest()
        {
            var logger = new Mock<ILogger>();
            var register = new Mock<IRegister>();
            var informing = new Mock<IInforming>();
            var MessageLogger = new Mock<IMessageLogger>();
         
            var mpAnswer = new Mock<IMPAnswer>();
            register.Setup(x => x.GetPersonInfo(It.IsAny<Person>()))
                .Returns(new EditableList<PersonInfo>(){new PersonInfo()});

            mpAnswer.Setup(x => x.GetData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).
                Returns(new List<V_MEDPOM_SMEV3Row> { new V_MEDPOM_SMEV3Row() });


         
            var cab = new CabinetService(MessageLogger.Object, mpAnswer.Object, informing.Object, register.Object, logger.Object);
            cab.GetData(person);


            logger.Verify(x => x.AddLog(It.IsAny<string>(), It.IsIn(LogType.Error)), Times.Never, "В логе ошибка!");

           
        }
    }
}