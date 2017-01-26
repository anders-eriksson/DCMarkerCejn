namespace DCMarkerEF
{
    using System.Data.Entity;

    public partial class DCLasermarkContext : DbContext
    {
        public DCLasermarkContext()
            : base("name=DCLasermarkContext")
        {
            var type = typeof(System.Data.Entity.SqlServer.SqlProviderServices);
        }

        public virtual DbSet<AssemblyCode> AssemblyCode { get; set; }
        public virtual DbSet<Fixture> Fixture { get; set; }
        public virtual DbSet<HistoryData> HistoryData { get; set; }
        public virtual DbSet<LaserData> LaserData { get; set; }
        public virtual DbSet<QuarterCode> QuarterCode { get; set; }
        public virtual DbSet<SerialNumber> SerialNumber { get; set; }
        public virtual DbSet<WeekCode> WeekCode { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AssemblyCode>()
                .Property(e => e.Unit)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<AssemblyCode>()
                .Property(e => e.Code)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Fixture>()
                .Property(e => e.FixturId)
                .IsUnicode(false);

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

            modelBuilder.Entity<LaserData>()
                .Property(e => e.Kant)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<QuarterCode>()
                .Property(e => e.QYear)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<QuarterCode>()
                .Property(e => e.Q1)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<QuarterCode>()
                .Property(e => e.Q2)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<QuarterCode>()
                .Property(e => e.Q3)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<QuarterCode>()
                .Property(e => e.Q4)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<WeekCode>()
                .Property(e => e.Code)
                .IsFixedLength()
                .IsUnicode(false);
        }
    }
}