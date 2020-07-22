using System.Linq;
using System.Threading.Tasks;
using GSC.Domain.Context;

namespace GSC.Common.UnitOfWork
{
    public interface IUnitOfWork
    {
        int Save();
        Task<int> SaveAsync();
        void Begin();
        void Commit();
        void Rollback();
        IQueryable<TEntity> FromSql<TEntity>(string sql, params object[] parameters) where TEntity : class;
    }
}