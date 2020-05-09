using GSC.Data.Entities.Location;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.UserMgt;
using Microsoft.EntityFrameworkCore;

namespace GSC.Domain
{
    public static class DefaultEntityMappingExtension
    {
        public static void DefalutMappingValue(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ScopeName>().Property(x => x.Name).HasColumnName("ScopeName");
            // modelBuilder.Entity<AppUser>()
            //   .Property(b => b.CreatedDate)
            //   .HasDefaultValueSql("getdate()");

            modelBuilder.Entity<AppUserClaim>()
                .Property(b => b.CreatedDate)
                .HasDefaultValueSql("getdate()");

            modelBuilder.Entity<Country>()
                .Property(b => b.CreatedDate)
                .HasDefaultValueSql("getdate()");

            modelBuilder.Entity<State>()
                .Property(b => b.CreatedDate)
                .HasDefaultValueSql("getdate()");

            modelBuilder.Entity<City>()
                .Property(b => b.CreatedDate)
                .HasDefaultValueSql("getdate()");

            modelBuilder.Entity<Language>()
                .Property(b => b.CreatedDate)
                .HasDefaultValueSql("getdate()");

            modelBuilder.Entity<ScopeName>()
                .Property(b => b.CreatedDate)
                .HasDefaultValueSql("getdate()");
        }

        public static void DefalutDeleteValueFilter(this ModelBuilder modelBuilder)
        {
            ////modelBuilder.Entity<Country>()
            ////    .HasQueryFilter(p => !p.IsDeleted);

            ////modelBuilder.Entity<State>()
            ////    .HasQueryFilter(p => !p.IsDeleted);

            ////modelBuilder.Entity<City>()
            ////    .HasQueryFilter(p => !p.IsDeleted);

            ////modelBuilder.Entity<Language>()
            ////    .HasQueryFilter(p => !p.IsDeleted);

            ////modelBuilder.Entity<ScopeName>()
            ////    .HasQueryFilter(p => !p.IsDeleted);
        }
    }
}