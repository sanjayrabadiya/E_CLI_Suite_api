using GSC.Centeral.Models;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Screening;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Centeral.Context
{
    public class CenteralContext : DbContext
    {
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer("data source=GSCL1;Initial Catalog=CenteralDB;user id=sa;password=gsc2019");
        //}

        public CenteralContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Users> Users { get; set; }
        public DbSet<UserPassword> UserPassword { get; set; }

        public DbSet<RefreshToken> RefreshToken { get; set; }

        public int SaveChanges(IJwtTokenAccesser jwtTokenAccesser)
        {
            SetModifiedInformation(jwtTokenAccesser);

            //var auditTrails = GetAuditTrailCommons(jwtTokenAccesser);

            var result = base.SaveChanges();

            //SaveAuditTrailCommons(auditTrails);

            return result;
        }

        public async Task<int> SaveChangesAsync(IJwtTokenAccesser jwtTokenAccesser)
        {
            SetModifiedInformation(jwtTokenAccesser);

            //var auditTrails = GetAuditTrailCommons(jwtTokenAccesser);

            var result = await base.SaveChangesAsync();

            //SaveAuditTrailCommons(auditTrails);

            return result;
        }

        private void SetModifiedInformation(IJwtTokenAccesser jwtTokenAccesser)
        {
            if (jwtTokenAccesser == null || jwtTokenAccesser.UserId <= 0) return;

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedBy = jwtTokenAccesser.UserId;
                    entry.Entity.CreatedDate = DateTime.Now.ToUniversalTime();
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Property(x => x.CreatedBy).IsModified = false;
                    entry.Property(x => x.CreatedDate).IsModified = false;

                    if (entry.Entity.InActiveRecord)
                    {
                        entry.Entity.DeletedBy = jwtTokenAccesser.UserId;
                        entry.Entity.DeletedDate = DateTime.Now.ToUniversalTime();
                    }
                    else
                    {
                        entry.Entity.ModifiedBy = jwtTokenAccesser.UserId;
                        entry.Entity.ModifiedDate = DateTime.Now.ToUniversalTime();
                    }
                }

            foreach (var entry in ChangeTracker.Entries<ScreeningTemplateValueAudit>())
            {
                entry.Entity.TimeZone = jwtTokenAccesser.GetHeader("clientTimeZone");
                entry.Entity.IpAddress = jwtTokenAccesser.IpAddress;
            }
        }

        public int SaveChanges(int fake)
        {
            return base.SaveChanges();
        }

        public void Begin()
        {
            base.Database.BeginTransaction();
        }

        public void Commit()
        {
            base.Database.CommitTransaction();
        }

        public void Rollback()
        {
            base.Database.RollbackTransaction();
        }
    }
}
