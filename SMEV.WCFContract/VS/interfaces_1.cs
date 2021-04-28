using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
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
