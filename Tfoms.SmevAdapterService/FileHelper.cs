using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmevAdapterService
{
    public class FileHelper
    {
        public static bool CheckFileAv(string path)
        {
            try
            {
                using (var st = File.OpenRead(path))
                {
                    st.Close();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static Task<bool> CheckFileAvAsync(string path)
        {
            return Task.Run(() => CheckFileAv(path));
        }


        public static async Task<bool> TryCheckFileAvAsync(string path, int maxCount, int timeOut)
        {
            var count = 0;
            while (count < maxCount)
            {
                if (await CheckFileAvAsync(path))
                    return true;
                count++;
                await Task.Delay(timeOut);
            }
            return false;
        }


        public static bool TryCheckFileAv(string path, int maxCount, int timeOut)
        {
            var count = 0;
            while (count < maxCount)
            {
                if ( CheckFileAv(path))
                    return true;
                count++;
                Thread.Sleep(timeOut);
            }
            return false;
        }

    }
}
