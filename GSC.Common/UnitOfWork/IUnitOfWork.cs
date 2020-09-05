using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GSC.Domain.Context;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace GSC.Common.UnitOfWork
{
    public interface IUnitOfWork
    {
        int Save();
        Task<int> SaveAsync();
        void Begin();
        void Commit();
        void Rollback();
        IList<EntityEntry> GetAuditTracker();
        IQueryable<TEntity> FromSql<TEntity>(string sql, params object[] parameters) where TEntity : class;
    }
}