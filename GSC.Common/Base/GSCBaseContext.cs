using GSC.Common.Common;
using GSC.Shared.Generic;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GSC.Common.Base
{
    public class GSCBaseContext<TContext> : DbContext where TContext : DbContext
    {

        public ICommonSharedService _commonSharedService { get; set; }
        protected GSCBaseContext(DbContextOptions<TContext> options, ICommonSharedService commonSharedService)
         : base(options)
        {
            _commonSharedService = commonSharedService;
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }




        public DbSet<UserAduit> UserAduit { get; set; }
        public DbSet<AuditTrail> AuditTrail { get; set; }
        public DbSet<AuditValue> AuditValue { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {

            SetAuditInformation();
            var audits = _commonSharedService.AuditTracker.GetAuditTracker(GetAuditTracker(), this);
            var result =  base.SaveChangesAsync(cancellationToken);
            AduitSave(audits);
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
            var audits = _commonSharedService.AuditTracker.GetAuditTracker(GetAuditTracker(), this);
            var result = base.SaveChanges();
            AduitSave(audits);
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

        public void SetDBConnection(string connectionString)
        {
            var getConnection = this.Database.GetDbConnection();
            getConnection.ConnectionString = connectionString;
            this.Database.SetDbConnection(getConnection);
        }
        public string GetConnectionString()
        {
            return this.Database.GetConnectionString();
        }

        public IQueryable<TEntity> FromSql<TEntity>(string sql, params object[] parameters) where TEntity : class => Set<TEntity>().FromSqlRaw(sql, parameters);
        #endregion
        void SetAuditInformation()
        {

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedBy = _commonSharedService.JwtTokenAccesser.UserId;
                    entry.Entity.CreatedDate = _commonSharedService.JwtTokenAccesser.GetClientDate();

                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Property(x => x.CreatedBy).IsModified = false;
                    entry.Property(x => x.CreatedDate).IsModified = false;

                    if (entry.Entity.AuditAction == AuditAction.Deleted)
                    {
                        entry.Entity.DeletedBy = _commonSharedService.JwtTokenAccesser.UserId;
                        entry.Entity.DeletedDate = _commonSharedService.JwtTokenAccesser.GetClientDate();
                    }
                    else
                    {
                        entry.Entity.ModifiedBy = _commonSharedService.JwtTokenAccesser.UserId;
                        entry.Entity.ModifiedDate = _commonSharedService.JwtTokenAccesser.GetClientDate();
                    }
                }
        }




        private IList<EntityEntry> GetAuditTracker()
        {
            ChangeTracker.DetectChanges();
            var result = new List<EntityEntry>();
            foreach (var item in ChangeTracker.Entries().ToList())
            {
                if (item.Entity is ICommonAduit)
                {
                    result.Add(item);
                }
            }
            return result;
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



        async void AduitSave(List<AuditTrail> audits)
        {
            DetachAllEntities();
            if (audits != null && audits.Any())
            {
                audits.ForEach(x =>
                {
                    if (x.RecordId == 0)
                    {
                        x.RecordId = x.Entity.Id;
                    }

                    this.Entry(x).State = EntityState.Added;

                });
                base.SaveChangesAsync().Wait();
            }
        }

    }
}
