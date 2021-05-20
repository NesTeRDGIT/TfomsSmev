using CabinetContract;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmevAdapterService.CabinetService
{
    public interface IRegister
    {
        List<PersonInfo> GetPersonInfo(Person person);
    }

    public class Register: IRegister
    {
        private string connectionString;
        public Register(string connectionString)
        {
            this.connectionString = connectionString;
        }
        public List<PersonInfo> GetPersonInfo(Person person)
        {
            using (var con = new SqlConnection(connectionString))
            {
                using (var sda = new SqlDataAdapter("GetPersonInfo", con))
                {
                    sda.SelectCommand.CommandType = CommandType.StoredProcedure;
                    sda.SelectCommand.Parameters.AddRange(GetParam(person).ToArray());
                    var tbl = new DataTable();
                    sda.Fill(tbl);
                    return Get(tbl);
                }
            }
        }


        private List<SqlParameter> GetParam(Person person)
        {
            var res = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(person.FAM))
                res.Add(new SqlParameter("@fam", person.FAM));
            if (!string.IsNullOrEmpty(person.IM))
                res.Add(new SqlParameter("@im", person.IM));
            if (!string.IsNullOrEmpty(person.OT))
                res.Add(new SqlParameter("@ot", person.OT));
            res.Add(new SqlParameter("@dr", person.DR));
            if (!string.IsNullOrEmpty(person.ENP))
                res.Add(new SqlParameter("@enp", person.ENP));
            if (!string.IsNullOrEmpty(person.SNILS))
                res.Add(new SqlParameter("@ss", person.SNILS));
            if (!string.IsNullOrEmpty(person.DOCS))
                res.Add(new SqlParameter("@docs", person.DOCS));
            if (!string.IsNullOrEmpty(person.DOCN))
                res.Add(new SqlParameter("@docn", person.DOCN));
            return res;
        }

        private List<PersonInfo> Get(DataTable tbl)
        {
            return tbl.Select().Select(PersonInfo.Get).ToList();
        }
    }
}
