using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmevAdapterService
{
    public interface IMPAnswer
    {
        List<V_MEDPOM_SMEV3Row> GetData(string FAM, string IM, string OT, DateTime? DR, string ENP, DateTime? DateFrom, DateTime? DateTo);
        List<V_MEDPOM_SMEV3Row> GetData(int[] SLUCH_ID_MAIN, int[] SLUCH_ID_MTR);
    }


    public class MPAnswer : IMPAnswer
    {
        private string connectionString { get; set; }

        public MPAnswer(string connectionString)
        {
            this.connectionString = connectionString;
        }
        public List<V_MEDPOM_SMEV3Row> GetData(string FAM, string IM, string OT, DateTime? DR, string ENP, DateTime? DateFrom, DateTime? DateTo)
        {
            var s = @"select *  from V_MEDPOM_SMEV where FAM = :FAM and IM= :IM and OT= :OT and DR= :DR and DATE_IN>= :DateFrom and nvl(DATE_OUT,'31.12.2200')<= :DateTo and ENP = :ENP";


            using (var conn = new OracleConnection(connectionString))
            {
                using (var oda = new OracleDataAdapter(s, conn))
                {
                    oda.SelectCommand.Parameters.AddRange(new List<OracleParameter>
                    {
                        new OracleParameter("FAM", FAM.NotIsNULL().ToUpper()),
                        new OracleParameter("IM", IM.NotIsNULL().ToUpper()),
                        new OracleParameter("OT", OT.NotIsNULL().ToUpper()),
                        new OracleParameter("DR", DR.NowIsNULL().Trunc()),
                        new OracleParameter("DateFrom", DateFrom.NowIsNULL().Trunc()),
                        new OracleParameter("DateTo", DateTo.NowIsNULL().Trunc()),
                        new OracleParameter("ENP", ENP.NotIsNULL().ToUpper())
                    }.ToArray());
                    var tbl = new DataTable();
                    oda.Fill(tbl);
                    return V_MEDPOM_SMEV3Row.Get(tbl.Select());
                }
            }
        }


        public List<V_MEDPOM_SMEV3Row> GetData(int[] SLUCH_ID_MAIN, int[] SLUCH_ID_MTR)
        {
            var whereop = new List<string>();
            if (SLUCH_ID_MAIN.Length != 0)
                whereop.Add($"(sluch_id in ({string.Join(",", SLUCH_ID_MAIN)}) and isMTR=0)");
            if (SLUCH_ID_MTR.Length != 0)
                whereop.Add($"(sluch_id in ({string.Join(",", SLUCH_ID_MTR)}) and isMTR=1)");

            if (whereop.Count==0) return new List<V_MEDPOM_SMEV3Row>();

            var s = $"Select * from V_MEDPOM_SMEV WHERE {string.Join(" or ", whereop)}";
            using (var conn = new OracleConnection(connectionString))
            {
                using (var oda = new OracleDataAdapter(s, conn))
                {
                    var tbl = new DataTable();
                    oda.Fill(tbl);
                    return V_MEDPOM_SMEV3Row.Get(tbl.Select());
                }
            }
        }


    }

    public class V_MEDPOM_SMEV3Row
    {
        public static List<V_MEDPOM_SMEV3Row> Get(IEnumerable<DataRow> rows)
        {
            return rows.Select(Get).ToList();
        }
        public static V_MEDPOM_SMEV3Row Get(DataRow row)
        {
            try
            {
                var item = new V_MEDPOM_SMEV3Row();
                item.SLUCH_Z_ID = Convert.ToInt32(row["sluch_z_id"]);
                item.SLUCH_ID = Convert.ToInt32(row["SLUCH_ID"]);
                if(row["USL_ID"] !=DBNull.Value)
                    item.USL_ID = Convert.ToInt32(row["usl_id"]);
                item.isMTR = Convert.ToBoolean(row["isMTR"]);
                item.FAM = Convert.ToString(row["FAM"]);
                item.IM = Convert.ToString(row["IM"]);
                item.OT = Convert.ToString(row["OT"]);
                item.DR = Convert.ToDateTime(row["DR"]);
                item.ENP = Convert.ToString(row["ENP"]);

                item.CODE_MO = Convert.ToString(row["CODE_MO"]);
                item.NAM_MOK = Convert.ToString(row["NAM_MOK"]);

                item.DATE_1 = Convert.ToDateTime(row["DATE_1"]);
                item.DATE_2 = Convert.ToDateTime(row["DATE_2"]);
                item.CODE_USL = Convert.ToString(row["CODE_USL"]);
                item.NAME_USL = Convert.ToString(row["NAME_USL"]);
                item.DATE_IN = Convert.ToDateTime(row["DATE_IN"]);
                item.DATE_OUT = Convert.ToDateTime(row["DATE_OUT"]);

                if(row["USL_OK"]!=DBNull.Value)
                    item.USL_OK = Convert.ToInt32(row["USL_OK"]);
                item.USL_OK_NAME = Convert.ToString(row["USL_OK_NAME"]);
                if (row["VIDPOM"] != DBNull.Value)
                    item.VIDPOM = Convert.ToInt32(row["VIDPOM"]);
                item.VIDPOM_NAME = Convert.ToString(row["VIDPOM_NAME"]);
                item.TF_NAME = Convert.ToString(row["TF_NAME"]);
                item.SUMP_USL = Convert.ToDecimal(row["SUMP_USL"]);               
                item.IDSP = Convert.ToInt32(row["IDSP"]);
                item.IDSP_NAME = Convert.ToString(row["IDSP_NAME"]);
                
                return item;
            }
            catch(Exception ex)
            {
                throw new Exception($"Ошибка получения V_MEDPOM_SMEV3Row: {ex.Message}", ex);
            }
        }
        public int SLUCH_Z_ID { get; set; }
        public int SLUCH_ID { get; set; }        
        public int? USL_ID { get; set; }
        public bool isMTR { get; set; }
        public string FAM { get; set; }
        public string IM { get; set; }
        public string OT { get; set; }
        public DateTime DR { get; set; }
        public string ENP { get; set; }
        public string CODE_MO { get; set; }
        public string NAM_MOK { get; set; }
        public DateTime DATE_1 { get; set; }
        public DateTime DATE_2 { get; set; }
        public string CODE_USL { get; set; }
        public string NAME_USL { get; set; }
        public DateTime DATE_IN { get; set; }
        public DateTime DATE_OUT { get; set; }
        public decimal SUMP_USL { get; set; }
        public int IDSP { get; set; }
        public int? USL_OK { get; set; }
        public string USL_OK_NAME { get; set; }
        public int? VIDPOM { get; set; }
        public string VIDPOM_NAME { get; set; }
        public string TF_NAME { get; set; }
        public string IDSP_NAME { get;  set; }
    }

    static class Ex
    {
        public static DateTime NowIsNULL(this DateTime? value)
        {
            return value ?? DateTime.Now;
        }

        public static DateTime Trunc(this DateTime value)
        {
            return value.Date;
        }

        public static string NotIsNULL(this string value)
        {
            return string.IsNullOrEmpty(value)? "НЕТ" : value;
        }
    }

}
