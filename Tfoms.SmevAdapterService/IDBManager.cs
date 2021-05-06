using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmevAdapterService.VS;

namespace SmevAdapterService
{
    public interface IDBManager
    {
        IResponseMessage Process();
    }

    public class PGManager : IDBManager
    {
        public IResponseMessage Process()
        {
            throw new NotImplementedException();
        }
    }
}
