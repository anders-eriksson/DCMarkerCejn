namespace DCHistory.Model
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class DCHistoryContext : DbContext
    {
        public DCHistoryContext()
            : base("name=DCHistoryContext")
        {
            // Without this line, the compiler will optimize away System.Data.Entity.SqlServer
            var type = typeof(System.Data.Entity.SqlServer.SqlProviderServices);
        }

        public virtual DbSet<HistoryData> HistoryDatas { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HistoryData>()
                .Property(e => e.Kant)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<HistoryData>()
                .Property(e => e.DateMark)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<HistoryData>()
                .Property(e => e.DateMarkLong)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<HistoryData>()
                .Property(e => e.DateMarkShort)
                .IsFixedLength()
                .IsUnicode(false);
        }
    }
}