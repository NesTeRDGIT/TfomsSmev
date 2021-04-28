using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace AddapterSMEVClient.ServerBehavior
{
    class MessageServerBehavior : IEndpointBehavior, IDisposable
    {
        private MyServiceMessageInspector _inspector;

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            _inspector = new MyServiceMessageInspector();
            clientRuntime.MessageInspectors.Add(_inspector);
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        public void Dispose()
        {
            _inspector.Dispose();
        }
    }

    public class MyServiceMessageInspector : IClientMessageInspector
    {
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
           var st =  request.ToString();
            return null;
            // тут отлавливаем сообщение до сериализации перед отправкой запроса к сервису.
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            var st = reply.ToString(); 
            // тут отлавливаем сообщение до сериализации после получения ответа от сервиса. 
        }

        public void Dispose()
        {

        }
    }
}
