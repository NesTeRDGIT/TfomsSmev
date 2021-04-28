
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;


namespace SmevAdapterService.AdapterLayer.Integration
{


    public class DataBaseIntegration : IRepository
    {
        string connectionString;
        public DataBaseIntegration(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void EndProcessMessage(MessageIntegration mes)
        {
            throw new NotImplementedException();
        }

        public void ErrorMessage(MessageIntegration doc)
        {
            throw new NotImplementedException();
        }

        public List<MessageIntegration> GetMessage()
        {
            List<MessageIntegration> list = new List<MessageIntegration>();


            using (var cx = new AdapterDbModel(connectionString))
            {
                //var t = cx.send_receive.ToList();
                var l = cx.receive_table.Where(x => x.status == "NEW").ToList();
                foreach (var item in l)
                {
                    list.Add(new MessageIntegration() { ID = item.uid, Key = item.id, Content = XDocument.Parse(item.content) });
                }
            }
            return list;
        }

        public void ReadMessage(MessageIntegration doc)
        {
            using (var cx = new AdapterDbModel(connectionString))
            {
                var l = cx.receive_table.Where(x => x.uid == doc.ID).ToList();
                foreach (var item in l)
                {
                    item.status = "";
                }
                cx.SaveChanges();
            }
        }

        public void SendMessage(MessageIntegration doc)
        {
            using (var cx = new AdapterDbModel(connectionString))
            {
                //Отправка
                send_receive send = new send_receive();
                send.id = doc.Key;
                send.status = "NEW";
              //  send.content = doc.Content.ToString();
              //  cx.send_table.Add(send);
                //
                var l = cx.receive_table.Where(x => x.uid == doc.ID).ToList();
                foreach (var item in l)
                {
                    item.status = "";
                }

                cx.SaveChanges();

            }
        }
    }


   
    /*
    public class MessageProcessorDataBase : AdapterMessageProcessor
    {
        public SendRequest responseToDb = null;

        public MessageProcessorDataBase(XDocument message, VsType entry,bool isTest=false) :base(message, entry, isTest) { }
        public MessageProcessorDataBase(XDocument message, VsType entry, List<XmlQualifiedName> ns, bool isTest=false) : base(message, entry, ns, isTest) { }
    }

    public class TransportProcessorDataBase : AdapterTransportProcessor
    {
        public override AdapterIntegration Integration { get { return AdapterIntegration.Database; } }
        private readonly string connectionString;
        public TransportProcessorDataBase(VsType type) : base(type)
        {
            if (!NeedToProcess) return;

            var vs = VsType.VsEntry.FirstOrDefault(x=>x.VsIntegration==AdapterIntegration.Database && x.Enabled==true);
            if (vs == null) throw new ArgumentNullException("Интеграция не соответствует параметру Database или параметр Enabled=false");
            connectionString = "name="+vs.VsPath;
            if (!CheckConnection(connectionString))
            {
                throw new FailConnectionException("Обработчик не может подключиться к БД!");
            }
        }
        protected override Dictionary<string,string> GetMessagesFromQueue()
        {
            Dictionary<string, string> list = null;
            try
            {
                using (var cx = new AdapterDbModel(connectionString))
                {
                    //var t = cx.send_receive.ToList();
                    var l = cx.send_receive.Where(x => x.status == "NEW").ToList();
                    if (l.Any())
                    {
                        list = (from a in l
                                where a.status == "NEW"
                                join b in cx.receive_table.Where(x=>x.messagetype== "Request") on a.id equals b.id 
                                select new { id = b.id, message = b.content })
                                    .ToDictionary(p => p.id, p => p.message);
                        return list;
                    }
                    return null;
                }
            }
            catch(EntityException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public override void PutMessagesToQueue(List<Tuple<string, string, string>> messages)
        {
            try
            {
                using (var cx = new AdapterDbModel(connectionString))
                {
                    foreach (var message in messages)
                    {
                        send_table send = new send_table();

                        send.id = message.Item1;
                        send.status = "NEW";
                        send.content = message.Item2;
                        cx.send_table.Add(send);
                        cx.SaveChanges();
                    }
                }
            }
            catch (EntityException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool CheckConnection(string connection)
        {
            var er = AdapterDbModel.GetADOConnectionString(connection);
            Npgsql.NpgsqlConnectionStringBuilder sb = new Npgsql.NpgsqlConnectionStringBuilder(er.ConnectionString);
            sb.Timeout = 2;
            er = new Npgsql.NpgsqlConnection(sb.ConnectionString);
            try
            {
                using (var cx = new AdapterDbModel(er,true))
                {

                    cx.Database.Connection.Open();
                    cx.Database.Connection.Close();
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
    */
}
