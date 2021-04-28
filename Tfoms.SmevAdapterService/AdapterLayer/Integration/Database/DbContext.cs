using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using Npgsql;

#region Таблицы
namespace Tfoms.AdapterLayer.Integration.DataBase
{
    [Table("adapter_smev3.adapter_log")]
    public class adapter_log
    {
        [Key]
        public int uid { get; set; }

        public string id { get; set; }

        public string request { get; set; }

        public string response { get; set; }

        [StringLength(15)]
        public string status { get; set; }

        public string fail_info { get; set; }
    }

    [Table("adapter_smev3.errors_log")]
    public class errors_log
    {
        [Key]
        public int uid { get; set; }

        public string ref_id { get; set; }

        [Required]
        [StringLength(20)]
        public string error_code { get; set; }

        [Required]
        [StringLength(200)]
        public string error_exception_type { get; set; }

        public string error_description { get; set; }

        [StringLength(30)]
        public string vs_code { get; set; }

        public string vs_name { get; set; }

        public string methodName { get; set; }
    }

    [Table("adapter_smev3.receive_table")]
    public class receive_table
    {
        [Key]
        public int uid { get; set; }

        public string id { get; set; }

        public string node_id { get; set; }

        [Required]
        public string content { get; set; }

        public string ref_id { get; set; }

        public string ref_group_id { get; set; }

        public DateTime created_at { get; set; }

        public string messagetype { get; set; }
    }

    [Table("adapter_smev3.send_receive")]
    public class send_receive
    {
        [Key]
        public int uid { get; set; }

        [Required]
        public string id { get; set; }

        [Required]
        public string status { get; set; }

        public DateTime? created_at { get; set; }

        public string receive_id { get; set; }
    }

    [Table("adapter_smev3.send_table")]
    public class send_table
    {
        [Key]
        public int uid { get; set; }

        public string id { get; set; }

        [Required]
        public string content { get; set; }

        [Required]
        public string status { get; set; }

        public DateTime created_at { get; set; }

    }
}
#endregion

#region Контекст данных
namespace SmevAdapterService.AdapterLayer.Integration
{
    public class AdapterDbModel : DbContext
    {
        private void FixEfProviderServicesProblem()
        {
            // The Entity Framework provider type 'System.Data.Entity.SqlServer.SqlProviderServices, EntityFramework.SqlServer'
            // for the 'System.Data.SqlClient' ADO.NET provider could not be loaded. 
            // Make sure the provider assembly is available to the running application. 
            // See http://go.microsoft.com/fwlink/?LinkId=260882 for more information.
            var instance = SqlProviderServices.Instance;
        }

        public static NpgsqlConnection GetADOConnectionString(string name)
        {
            var ctx = new AdapterDbModel(name);
            NpgsqlConnection ec = (NpgsqlConnection)(ctx.Database.Connection);
            return ec;
        }

        public AdapterDbModel() :base("name=PostgresPro")
        {

        }
        public AdapterDbModel(string connectionString) : base(connectionString)
        {
            
        }

        public AdapterDbModel(DbConnection connection,bool owner) : base(connection, owner)
        {

        }

        public virtual DbSet<adapter_log> adapter_log { get; set; }
        public virtual DbSet<errors_log> errors_log { get; set; }
        public virtual DbSet<receive_table> receive_table { get; set; }
        public virtual DbSet<send_receive> send_receive { get; set; }
        public virtual DbSet<send_table> send_table { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }


    [Table("adapter_smev3.adapter_log")]
    public class adapter_log
    {
        [Key]
        public int uid { get; set; }

        public string id { get; set; }

        public string request { get; set; }

        public string response { get; set; }

        [StringLength(15)]
        public string status { get; set; }

        public string fail_info { get; set; }
    }

    [Table("adapter_smev3.errors_log")]
    public class errors_log
    {
        [Key]
        public int uid { get; set; }

        public string ref_id { get; set; }

        [Required]
        [StringLength(20)]
        public string error_code { get; set; }

        [Required]
        [StringLength(200)]
        public string error_exception_type { get; set; }

        public string error_description { get; set; }

        [StringLength(30)]
        public string vs_code { get; set; }

        public string vs_name { get; set; }

        public string methodName { get; set; }
    }

    [Table("adapter_smev3.receive_table")]
    public class receive_table
    {
        [Key]
        public int uid { get; set; }

        public string id { get; set; }

        public string node_id { get; set; }

        [Required]
        public string content { get; set; }

        public string ref_id { get; set; }

        public string ref_group_id { get; set; }

        public DateTime created_at { get; set; }

        public string messagetype { get; set; }

        public string status { get; set; }
    }

    [Table("adapter_smev3.send_receive")]
    public class send_receive
    {
        [Key]
        public int uid { get; set; }

        [Required]
        public string id { get; set; }

        [Required]
        public string status { get; set; }

        public DateTime? created_at { get; set; }

        public string receive_id { get; set; }
    }

    [Table("adapter_smev3.send_table")]
    public class send_table
    {
        [Key]
        public int uid { get; set; }

        public string id { get; set; }

        [Required]
        public string content { get; set; }

        [Required]
        public string status { get; set; }

        public DateTime created_at { get; set; }

    }
}

#endregion


