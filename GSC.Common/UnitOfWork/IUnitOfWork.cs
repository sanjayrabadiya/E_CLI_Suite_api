using System.Linq;
using System.Threading.Tasks;
using GSC.Domain.Context;

namespace GSC.Common.UnitOfWork
{
    public interface IUnitOfWork<TContext>
        where TContext : GscContext
    {
        TContext Context { get; }
        int Save();
        Task<int> SaveAsync();
        IQueryable<TEntity> FromSql<TEntity>(string sql, params object[] parameters) where TEntity : class;
    }
}