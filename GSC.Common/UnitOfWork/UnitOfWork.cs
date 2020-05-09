using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using GSC.Domain.Context;
using GSC.Helper;
using Microsoft.EntityFrameworkCore;

namespace GSC.Common.UnitOfWork
{
    public class UnitOfWork<TContext> : IUnitOfWork<TContext>
        where TContext : GscContext
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public UnitOfWork(TContext context, IJwtTokenAccesser jwtTokenAccesser)
        {
            Context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public int Save()
        {
            using (var transaction = Context.Database.BeginTransaction())
            {
                try
                {
                    var retValu = Context.SaveChanges(_jwtTokenAccesser);
                    transaction.Commit();
                    return retValu;
                }
                catch (DbException)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task<int> SaveAsync()
        {
            using (var transaction = Context.Database.BeginTransaction())
            {
                try
                {
                    var val = await Context.SaveChangesAsync(_jwtTokenAccesser);
                    transaction.Commit();
                    return val;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public IQueryable<TEntity> FromSql<TEntity>(string sql, params object[] parameters) where TEntity : class
        {
            return Context.Set<TEntity>().FromSql(sql, parameters);
        }

        public TContext Context { get; }

        public void Dispose()
        {
            Context.Dispose();
        }
    }
}