using GSC.Common.Common;
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
    public class GSCBaseContext<TContext> : DbContext where TContext : DbContext
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IAuditTracker _auditTracker;
        protected GSCBaseContext(DbContextOptions<TContext> options, IJwtTokenAccesser jwtTokenAccesser,
            IAuditTracker auditTracker)
         : base(options)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _auditTracker = auditTracker;
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public DbSet<UserAduit> UserAduit { get; set; }
        public DbSet<AuditTrailCommon> AuditTrailCommon { get; set; }
        public DbSet<AuditValue> AuditValue { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {

            SetAuditInformation();
            var addChangeTracker = GetAuditTracker();
            var audits = _auditTracker.GetAuditTracker(addChangeTracker, this);
            var result = base.SaveChangesAsync(cancellationToken);
            AduitSave(audits, addChangeTracker.ToList());
            return result;
        }

        public async Task<int> SaveWithOutAuditAsync()
        {
            SetAuditInformation();
            return await base.SaveChangesAsync();
        }

        public int Save()
        {
            SetAuditInformation();
            var addChangeTracker = GetAuditTracker();
            var audits = _auditTracker.GetAuditTracker(addChangeTracker, this);
            var result = base.SaveChanges();
            AduitSave(audits, addChangeTracker.ToList());
            return result;
        }

        public void ApplyStateChanges()
        {
            SetAuditInformation();
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

                    if (entry.Entity.AuditAction == Helper.AuditAction.Deleted)
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

        async void AduitSave(List<AuditTrailCommon> audits, List<EntityEntry> entities)
        {
            DetachAllEntities();
            if (audits != null && audits.Count() > 0)
            {
                audits.ForEach(x =>
                {
                    if (x.RecordId == 0)
                    {
                        var entity = entities.FirstOrDefault(c => c.CurrentValues.EntityType.ClrType.Name == x.TableName);
                        x.RecordId = (entity.Entity as BaseEntity).Id;
                    }

                    this.Entry(x).State = EntityState.Added;

                });
                base.SaveChangesAsync().Wait();
            }
        }

    }
}
