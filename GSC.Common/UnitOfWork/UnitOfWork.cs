using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
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
            _auditTracker.GetAuditTracker();
            return Context.SaveChanges(_jwtTokenAccesser);
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