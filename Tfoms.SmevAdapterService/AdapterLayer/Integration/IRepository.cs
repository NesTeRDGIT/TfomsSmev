using System.Collections.Generic;

namespace SmevAdapterService.AdapterLayer.Integration
{
    /// <summary>
    /// Конфиг репозитория
    /// </summary>
    public interface IConfigRepository
    {

    }
    /// <summary>
    /// Репозиторий
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// Получить сообщение из очереди
        /// </summary>
        /// <returns></returns>
        List<MessageIntegration> GetMessage();
        /// <summary>
        /// Отправить сообщение
        /// </summary>
        /// <param name="doc"></param>
        void SendMessage(MessageIntegration doc);
        /// <summary>
        /// Пометить как прочитаное сообщение
        /// </summary>
        /// <param name="doc"></param>
        void ReadMessage(MessageIntegration doc);
        /// <summary>
        /// Пометить как ошибка
        /// </summary>
        /// <param name="doc"></param>
        void ErrorMessage(MessageIntegration doc, string perfix_ext = "");
        /// <summary>
        /// Пометить как обработаное
        /// </summary>
        /// <param name="mes"></param>
        void EndProcessMessage(MessageIntegration mes);
    }
  
    public interface IProcess
    {
        void Process();
    }

}
