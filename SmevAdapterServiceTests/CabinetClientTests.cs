using Microsoft.VisualStudio.TestTools.UnitTesting;
using CabinetContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CabinetContract.Tests
{
    [TestClass()]
    public class CabinetClientTests
    {
        [TestMethod()]
        public void OpenTest()
        {
            using (var cab = new CabinetClient())
            {
                cab.Open("http://localhost:8081/CabinetService.svc");
            }
        }
      

        [TestMethod()]
        public void OpenMainServiceTest()
        {
            using (var cab = new CabinetClient())
            {
              cab.Open("http://nexus:8081/RoutingService/WCFRouting.svc/CabinetService.svc");
               
                //cab.Client.GetData(personMy);
            }
        }

        private Person person = new Person()
        {
            FAM = "ЕМЕЛИН", IM = "ИЛЬЯ", OT = "НИКОЛАЕВИЧ", DR = new DateTime(1964, 2, 28), ENP = "3210987654321098", W = 1
        };

        private Person personMy = new Person()
        {
            FAM = "НЕСТЕРЕНОК",
            IM = "ДЕНИС",
            OT = "ВАЛЕРЬЕВИЧ",
            DR = new DateTime(1991, 1, 9),
            ENP = "7558800840000156",
            W = 1
        };

        private Person personFail = new Person()
        {
            FAM = "ЕМЕЛИН",
            IM = "ИЛЬЯ",
            OT = "Н",
            DR = new DateTime(1964, 2, 28),
            ENP = "3210987654321098",
            W = 1
        };
        [TestMethod()]
        public void GetDataTest()
        {
            using (var cab = new CabinetClient())
            {
                cab.Open("http://localhost:8081/CabinetService.svc");
                var t = cab.Client.GetData(personFail);
                cab.Close();
            }
        }

        [TestMethod()]
        public void InformingInsertRecordTest()
        {
            using (var cab = new CabinetClient())
            {
                cab.Open("http://localhost:8081/CabinetService.svc");
                cab.Client.InformingInsertRecord(person.ENP);
                cab.Close();
            }
        }
    }
}