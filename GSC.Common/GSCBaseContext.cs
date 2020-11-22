using GSC.Shared;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GSC.Common
{
    public class GSCBaseContext<TContext> : DbContext where TContext : DbContext
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        protected GSCBaseContext(DbContextOptions<TContext> options, IJwtTokenAccesser jwtTokenAccesser)
         : base(options)
        {
            _jwtTokenAccesser = jwtTokenAccesser;

            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public override int SaveChanges()
        {
            throw new Exception("Please provide IJwtTokenAccesser in SaveChanges() method.");
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new Exception("Please provide IJwtTokenAccesser in SaveChangesAsync() method.");
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new Exception("Please provide IJwtTokenAccesser in SaveChangesAsync() method.");
        }

        public int SaveChanges(IJwtTokenAccesser jwtTokenAccesser)
        {
            SetModifiedInformation(jwtTokenAccesser);


            var result = base.SaveChanges();


            return result;
        }

        public int SaveChanges(int fake)
        {
            return base.SaveChanges();
        }

        public async Task<int> SaveChangesAsync(IJwtTokenAccesser jwtTokenAccesser)
        {
            SetModifiedInformation(jwtTokenAccesser);


            var result = await base.SaveChangesAsync();


            return result;
        }

        public void DetectionAll()
        {
            var entries = ChangeTracker.Entries().Where(e =>
                    e.State == EntityState.Added ||
                    e.State == EntityState.Unchanged ||
                    e.State == EntityState.Modified ||
                    e.State == EntityState.Deleted)
                .ToList();
            entries.ForEach(r => r.State = EntityState.Detached);
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

        private void SetModifiedInformation(IJwtTokenAccesser jwtTokenAccesser)
        {
            if (jwtTokenAccesser == null || jwtTokenAccesser.UserId <= 0) return;

            //foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            //    if (entry.State == EntityState.Added)
            //    {
            //        entry.Entity.CreatedBy = jwtTokenAccesser.UserId;
            //        entry.Entity.CreatedDate = DateTime.Now.ToUniversalTime();
            //    }
            //    else if (entry.State == EntityState.Modified)
            //    {
            //        entry.Property(x => x.CreatedBy).IsModified = false;
            //        entry.Property(x => x.CreatedDate).IsModified = false;

            //        if (entry.Entity.InActiveRecord)
            //        {
            //            entry.Entity.DeletedBy = jwtTokenAccesser.UserId;
            //            entry.Entity.DeletedDate = DateTime.Now.ToUniversalTime();
            //        }
            //        else
            //        {
            //            entry.Entity.ModifiedBy = jwtTokenAccesser.UserId;
            //            entry.Entity.ModifiedDate = DateTime.Now.ToUniversalTime();
            //        }
            //    }

            //foreach (var entry in ChangeTracker.Entries<ScreeningTemplateValueAudit>())
            //{
            //    entry.Entity.TimeZone = jwtTokenAccesser.GetHeader("clientTimeZone");
            //    entry.Entity.IpAddress = jwtTokenAccesser.IpAddress;
            //}
        }
    }
}
