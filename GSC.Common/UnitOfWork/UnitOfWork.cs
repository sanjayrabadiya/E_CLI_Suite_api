using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using GSC.Data.Entities.Audit;
using GSC.Data.Entities.Common;
using GSC.Domain.Context;
using GSC.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace GSC.Common.UnitOfWork
{
    public class UnitOfWork<TContext> : IUnitOfWork<TContext>
        where TContext : GscContext
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IAuditTracker _auditTracker;
        public UnitOfWork(TContext context, IJwtTokenAccesser jwtTokenAccesser, IAuditTracker auditTracker)
        {
            Context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _auditTracker = auditTracker;
        }

        public int Save()
        {
            var addChangeTracker = Context.GetAuditTracker().ToList();

            var audits = _auditTracker.GetAuditTracker();
            var result = Context.SaveChanges(_jwtTokenAccesser);
           // AduitSave(audits, addChangeTracker);
            return result;
        }

        async void AduitSave(List<AuditTrailCommon> audits, List<EntityEntry> entities)
        {
            if (audits != null || audits.Count() > 0)
            {
                audits.ForEach(x =>
                {
                    if (x.RecordId == 0)
                    {
                        var entity = entities.FirstOrDefault(c => c.CurrentValues.EntityType.ClrType.Name == x.TableName);
                        x.RecordId = (entity.Entity as BaseEntity).Id;
                    }
                    Context.AuditTrailCommon.Add(x);

                });
                Context.SaveChanges(_jwtTokenAccesser);
            }
        }

        public async Task<int> SaveAsync()
        {
            return await Context.SaveChangesAsync(_jwtTokenAccesser);
        }

        public IQueryable<TEntity> FromSql<TEntity>(string sql, params object[] parameters) where TEntity : class
        {
            return Context.Set<TEntity>().FromSqlRaw(sql, parameters);
        }

        public TContext Context { get; }

        public void Dispose()
        {
            Context.Dispose();
        }

        public void Begin()
        {
            Context.Begin();
        }
        public void Commit()
        {
            Context.Commit();
        }

        public void Rollback()
        {
            Context.Rollback();
        }

        public IList<EntityEntry> GetAuditTracker()
        {
            return Context.GetAuditTracker();

        }
    }
}