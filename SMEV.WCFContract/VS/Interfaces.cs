using System.Xml.Linq;

namespace SmevAdapterService.VS
{
    public interface IResponseMessage
    {
        XElement Serialize();    
    }
    public interface IRequestMessage
    {
        /// <summary>
        /// Ответное сообщение
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        IResponseMessage Answer(string connectionString);
        /// <summary>
        /// Сообщение для архива
        /// </summary>
        /// <returns></returns>
    }
}
