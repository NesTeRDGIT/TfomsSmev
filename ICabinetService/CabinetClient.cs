using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CabinetContract
{
    /// <summary>
    /// Клиент к CabinetService
    /// </summary>
    public class CabinetClient:IDisposable
    {
        /// <summary>
        /// Контракт сервиса
        /// </summary>
        public ICabinetService Client { get; private set; }
        private ChannelFactory<ICabinetService> channel;
      
        /// <summary>
        /// Открыть подключение
        /// </summary>
        /// <param name="address">Адрес</param>
        public void Open(string address = "http://nexus:8081/CabinetService/CabinetService.svc")
        {
            var Binding = new WSHttpBinding(SecurityMode.None);
            Binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            Binding.Security.Message.EstablishSecurityContext = false;
            channel = new ChannelFactory<ICabinetService>(Binding, address);
            Client = channel.CreateChannel();
            Client.Ping();
        }
        /// <summary>
        /// Закрыть подключение
        /// </summary>
        public void Close()
        {
            channel?.Abort();
        }
        /// <summary>
        /// Уничтожение объекта
        /// </summary>
        public void Dispose()
        {
            Close();
            ((IDisposable) channel)?.Dispose();
        }
    }
}
