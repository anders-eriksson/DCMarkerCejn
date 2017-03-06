using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using GlblRes = global::DCMarkerEF.Properties.Resources;

namespace DCMarkerEF
{
    public partial class DCLasermarkContext : DbContext
    {
        public DCLasermarkContext()
            : base("name=DCLasermarkContext")
        {
            // Without this line, the compiler will optimize away  System.Data.Entity.SqlServer
            var type = typeof(System.Data.Entity.SqlServer.SqlProviderServices);
        }

        public virtual DbSet<Fixture> Fixture { get; set; }
        public virtual DbSet<HistoryData> HistoryData { get; set; }
        public virtual DbSet<LaserData> LaserData { get; set; }
        public virtual DbSet<QuarterCode> QuarterCode { get; set; }
        public virtual DbSet<SerialNumber> SerialNumber { get; set; }
        public virtual DbSet<WeekCode> WeekCode { get; set; }

        protected override System.Data.Entity.Validation.DbEntityValidationResult ValidateEntity(DbEntityEntry entityEntry, System.Collections.Generic.IDictionary<object, object> items)
        {
            if (entityEntry.Entity is LaserData)
            {
                if (string.IsNullOrWhiteSpace(entityEntry.CurrentValues.GetValue<string>("F1")))
                {
                    var list = new List<System.Data.Entity.Validation.DbValidationError>();
                    list.Add(new System.Data.Entity.Validation.DbValidationError("F1", GlblRes.ArticleF1_is_required));

                    return new System.Data.Entity.Validation.DbEntityValidationResult(entityEntry, list);
                }
            }

            return base.ValidateEntity(entityEntry, items);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Types()
            //  .Configure(c => c.Ignore("IsDirty"));

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

        //public override int SaveChanges()
        //{
        //    foreach (var history in this.ChangeTracker.Entries()
        //      .Where(e => e.Entity is IModificationHistory && (e.State == EntityState.Added ||
        //              e.State == EntityState.Modified))
        //       .Select(e => e.Entity as IModificationHistory)
        //      )
        //    {
        //        history.DateModified = DateTime.Now;
        //        if (history.DateCreated == DateTime.MinValue)
        //        {
        //            history.DateCreated = DateTime.Now;
        //        }
        //    }
        //    int result = base.SaveChanges();
        //    foreach (var history in this.ChangeTracker.Entries()
        //                                  .Where(e => e.Entity is IModificationHistory)
        //                                  .Select(e => e.Entity as IModificationHistory)
        //      )
        //    {
        //        history.IsDirty = false;
        //    }
        //    return result;
        //}

        public override int SaveChanges()
        {
            var objContext = ((IObjectContextAdapter)this).ObjectContext;
            var entries = objContext.ObjectStateManager.GetObjectStateEntries(EntityState.Added | EntityState.Modified)
                .Select(entry => entry.Entity);
            var str = typeof(string).Name;

            foreach (var entity in entries)
            {
                var properties = from p in entity.GetType().GetProperties()
                                 where p.PropertyType.Name == str
                                 select p;

                var items = from item in properties
                            let value = (string)item.GetValue(entity, null)
                            where value != null && value.Trim().Length == 0
                            select item;

                foreach (var item in items)
                {
                    item.SetValue(entity, null, null);
                }
            }

            return base.SaveChanges();
        }
    }
}