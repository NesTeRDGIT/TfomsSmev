using SmevAdapterService.VS;
using System;
using System.Xml.Linq;

namespace SmevAdapterService.AdapterLayer.XmlClasses
{
   
    public  class AdapterMessageCreator
    {   /// <summary>
        /// Создание SendRequest(ClientMessage)
        /// </summary>
        /// <param name="response"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static XDocument GenerateAddapterSendRequest(IResponseMessage response, string ITSystem, string reply,string sending)
        {
         
          
            var text = response.Serialize();
            var send = new SendRequest()
            {
                itSystem = ITSystem,
                ResponseMessage = new ResponseMessageType()
                {
                    ResponseMetadata = new ResponseMetadataType()
                    {
                        clientId = sending,
                        replyToClientId = reply
                    },
                    ResponseContent = new ResponseContentType()
                    {
                    }
                }
            };
            if (response == null)
            {
                send.ResponseMessage.ResponseContent.rejects = new Reject[]
                {
                               new Reject()
                               {
                                   code=RejectCode.NO_DATA,
                                   description="Сведения не найдены"
                               }
                };
            }
            else
            {
                send.ResponseMessage.ResponseContent.content = new Content() { MessagePrimaryContent = text };
            };

            return send.SerializeToX(send.Xmlns); 
        }
    }
}
