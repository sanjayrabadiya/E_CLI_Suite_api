using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Centeral.UnitOfWork
{
    public interface IUnitOfWorkCenteral
    {
        int Save();
        Task<int> SaveAsync();
        void Begin();
        void Commit();
        void Rollback();
        //IList<EntityEntry> GetAuditTracker();
        IQueryable<TEntity> FromSql<TEntity>(string sql, params object[] parameters) where TEntity : class;
    }
}
