using GSC.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GSC.Common.Base
{
    public class GscContext<TContext> : DbContext where TContext : DbContext
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        protected GscContext(DbContextOptions<TContext> options, IJwtTokenAccesser jwtTokenAccesser)
         : base(options)
        {
            _jwtTokenAccesser = jwtTokenAccesser;

            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public DbSet<UserAduit> UserAduit { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            SetAuditInformation();
            return base.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> SaveAsync()
        {
            SetAuditInformation();
            return await base.SaveChangesAsync();
        }

        public int Save()
        {
            SetAuditInformation();
            return base.SaveChanges();
        }

        public void ApplyStateChanges()
        {
            foreach (var entry in base.ChangeTracker.Entries<BaseEntity>())
            {
                var stateInfo = entry.Entity;
                //entry.State = StateHelpers.ConvertState(stateInfo.State);
            }
        }

        #region Transactions
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
        #endregion

        #region Entity Operations
        public void SetRemove(object entity)
        {
            Entry(entity).State = EntityState.Deleted;
        }
        public void SetModified(object entity)
        {
            Entry(entity).State = EntityState.Modified;
        }
        public void SetAdd(object entity)
        {
            Entry(entity).State = EntityState.Added;
        }

        public IQueryable<TEntity> FromSql<TEntity>(string sql, params object[] parameters) where TEntity : class => Set<TEntity>().FromSqlRaw(sql, parameters);
        #endregion
        void SetAuditInformation()
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedBy = _jwtTokenAccesser.UserId;
                    entry.Entity.CreatedDate = DateTime.Now.ToUniversalTime();
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Property(x => x.CreatedBy).IsModified = false;
                    entry.Property(x => x.CreatedDate).IsModified = false;

                    if (entry.Entity.AuditAction== Helper.AuditAction.Deleted)
                    {
                        entry.Entity.DeletedBy = _jwtTokenAccesser.UserId;
                        entry.Entity.DeletedDate = DateTime.Now.ToUniversalTime();
                    }
                    else
                    {
                        entry.Entity.ModifiedBy = _jwtTokenAccesser.UserId;
                        entry.Entity.ModifiedDate = DateTime.Now.ToUniversalTime();
                    }
                }
        }

        


        public IList<EntityEntry> GetAuditTracker()
        {
            ChangeTracker.DetectChanges();

            return ChangeTracker.Entries().ToList();
        }

        public void DetachAllEntities()
        {
            var changedEntriesCopy = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added ||
                            e.State == EntityState.Modified ||
                            e.State == EntityState.Deleted ||
                            e.State == EntityState.Unchanged)
                .ToList();

            foreach (var entry in changedEntriesCopy)
                entry.State = EntityState.Detached;
        }

        //public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    throw new Exception("Please provide IJwtTokenAccesser in SaveChangesAsync() method.");
        //}

        //public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    throw new Exception("Please provide IJwtTokenAccesser in SaveChangesAsync() method.");
        //}

        //public IQueryable<TEntity> FromSql<TEntity>(string sql, params object[] parameters)
        //    where TEntity : class => Set<TEntity>().FromSqlRaw(sql, parameters);


        //public int Save()
        //{
        //    SetModifiedInformation();
        //    var result = base.SaveChanges();
        //    return result;
        //}

        //public int SaveChanges(int fake)
        //{
        //    return base.SaveChanges();
        //}

        //public async Task<int> SaveChangesAsync()
        //{
        //    SetModifiedInformation();

        //    var result = await base.SaveChangesAsync();


        //    return result;
        //}

        //public void DetectionAll()
        //{
        //    var entries = ChangeTracker.Entries().Where(e =>
        //            e.State == EntityState.Added ||
        //            e.State == EntityState.Unchanged ||
        //            e.State == EntityState.Modified ||
        //            e.State == EntityState.Deleted)
        //        .ToList();
        //    entries.ForEach(r => r.State = EntityState.Detached);
        //}



        //public void Begin()
        //{
        //    base.Database.BeginTransaction();
        //}

        //public void Commit()
        //{
        //    base.Database.CommitTransaction();
        //}

        //public void Rollback()
        //{
        //    base.Database.RollbackTransaction();
        //}

        //private void SetModifiedInformation()
        //{
        //    if (_jwtTokenAccesser == null || _jwtTokenAccesser.UserId <= 0) return;

        //    foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        //        if (entry.State == EntityState.Added)
        //        {
        //            entry.Entity.CreatedBy = _jwtTokenAccesser.UserId;
        //            entry.Entity.CreatedDate = DateTime.Now.ToUniversalTime();
        //        }
        //        else if (entry.State == EntityState.Modified)
        //        {
        //            entry.Property(x => x.CreatedBy).IsModified = false;
        //            entry.Property(x => x.CreatedDate).IsModified = false;

        //            if (entry.Entity.InActiveRecord)
        //            {
        //                entry.Entity.DeletedBy = _jwtTokenAccesser.UserId;
        //                entry.Entity.DeletedDate = DateTime.Now.ToUniversalTime();
        //            }
        //            else
        //            {
        //                entry.Entity.ModifiedBy = _jwtTokenAccesser.UserId;
        //                entry.Entity.ModifiedDate = DateTime.Now.ToUniversalTime();
        //            }
        //        }

        //    //foreach (var entry in ChangeTracker.Entries<ScreeningTemplateValueAudit>())
        //    //{
        //    //    entry.Entity.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
        //    //    entry.Entity.IpAddress = _jwtTokenAccesser.IpAddress;
        //    //}
        //}
    }
}
