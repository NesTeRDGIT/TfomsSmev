using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmevAdapterService.CabinetService
{
    class InformingModelDataContext: DbContext
	{
		public InformingModelDataContext(string ConnectionString) : base(ConnectionString)
		{

		}

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
		
			modelBuilder.Entity<PR88_INF_ZGLV>().ToTable("dbo.PR88_INF_ZGLV").HasKey(e => e.Id).Property(x=>x.Id).HasColumnName("Id").HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			modelBuilder.Entity<PR88_INF>().ToTable("dbo.PR88_INF").HasKey(e => e.Id).Property(x => x.Id).HasColumnName("Id").HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

			modelBuilder.Entity<PR88_INF>().HasRequired(x => x.PR88_INF_ZGLV).WithMany(x => x.PR88_INF).HasForeignKey(x=>x.Zglv_Id);
			base.OnModelCreating(modelBuilder);
		}
		public virtual DbSet<PR88_INF> PR88_INF { get; set; }
		public virtual DbSet<PR88_INF_ZGLV> PR88_INF_ZGLV { get; set; }
	}

	
	[Table("dbo.PR88_INF_ZGLV")]
	public  class PR88_INF_ZGLV
	{
		public int Id { get; set; }
		public string FILENAME { get; set; }
		public string CMOCOD { get; set; }
		public int NRECORDS { get; set; }
		public DateTime DATE_LOAD { get; set; }
		public string VERSION { get; set; }
		public string PACKET_TYPE { get; set; }

		public virtual ICollection<PR88_INF> PR88_INF { get; set; } = new List<PR88_INF>();
	}

	[Table("dbo.PR88_INF")]
	public class PR88_INF
	{
		public int Id { get; set; }
		public int N_ZAP { get; set; }
		public string ID_PAC { get; set; }
		public string ENP { get; set; }
		public int VID_INF { get; set; }
		public int FORM_INF { get; set; }
		public string SP { get; set; }
		public DateTime DATE_INF { get; set; }
		public int Zglv_Id { get; set; }
		
		public virtual PR88_INF_ZGLV PR88_INF_ZGLV { get; set; }
	}
}
