using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmevAdapterService.CabinetService
{
    public interface IInforming
    {
        void AddInforming(string ENP);
        bool Validate(string ENP);
    }
    public class Informing: IInforming
    {
        private string ConnectionOio { get; set; }


        public Informing(string connectionOio)
        {
            ConnectionOio = connectionOio;
        }


        public void AddInforming(string ENP)
        {
            using (var conn = new SqlConnection(ConnectionOio))
            {
                conn.Open();
                using (var cmd = new SqlCommand("dbo.AddInforming", conn) { CommandType = CommandType.StoredProcedure})
                {
                    cmd.Parameters.Add(new SqlParameter("@ENP",ENP));
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        public bool Validate(string ENP)
        {
            using (var conn = new SqlConnection(ConnectionOio))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT dbo.IsNeedInforming(@ENP)", conn) { CommandType = CommandType.Text })
                {
                    cmd.Parameters.Add(new SqlParameter("@ENP", ENP));
                    var item = cmd.ExecuteScalar();
                    conn.Close();
                    return Convert.ToBoolean(item);
                }
             
            }
        }
    }
    public static class DateExtensions
    {
        public static int Age(this DateTime Dr)
        {
            return Age(Dr, DateTime.Now);
        }
        public static int Age(this DateTime Dr, DateTime ForDate)
        {
            int age;
            age = ForDate.Year - Dr.Year;
            if (age > 0)
            {
                age -= Convert.ToInt32(ForDate.Date < Dr.Date.AddYears(age));
            }
            else
            {
                age = 0;
            }
            return age;
        }
    }
}
