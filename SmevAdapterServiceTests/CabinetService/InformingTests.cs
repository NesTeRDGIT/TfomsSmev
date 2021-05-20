using CabinetContract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmevAdapterService.CabinetService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmevAdapterService.CabinetService.Tests
{
    [TestClass()]
    public class InformingTests
    {
        public string SQLconn = "Data Source=NDV-1;Initial Catalog=OIO;Integrated Security=True";

        [TestMethod()]
        public void AddInformingTest()
        {
            var inf = new Informing(SQLconn);
            inf.AddInforming("7500");
            
        }

        [TestMethod()]
        public void ValidateTest()
        {
            var inf = new Informing(SQLconn);
            var t = inf.Validate("7500");
        }
    }
}