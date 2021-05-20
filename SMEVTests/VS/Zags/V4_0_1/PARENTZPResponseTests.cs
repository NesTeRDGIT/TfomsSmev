using Microsoft.VisualStudio.TestTools.UnitTesting;
using SMEV.VS.Zags.V4_0_1;
using SmevAdapterService.VS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using SMEV.AdapterLayer.XmlClasses;
using SMEV.WCFContract;

namespace SMEV.VS.Zags.V4_0_1.Tests
{
    [TestClass()]
    public class PARENTZPResponseTests
    {
        [TestMethod()]
        public void PARENTZPResponseTest()
        {
            var Content = XDocument.Load(@"C:\InputMail\TEST\ZAGS\Новая папка\zags-parentroot112-274.0.1\IN\{95a982b0-2496-11ea-a700-2da1baa43ac2}.xml");
            var adapterInMessage = SeDeserializer<QueryResult>.DeserializeFromXDocument(Content);
            //Если запрос
            if (adapterInMessage.Message is RequestMessageType)
            {
                var rmt = adapterInMessage.Message as RequestMessageType;
                var ns = rmt.RequestContent.content.MessagePrimaryContent.Name.Namespace;

             
            }
        }


    }
}