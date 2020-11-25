using GSC.Common.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GSC.Common
{
    public interface IContext : IDisposable
    {
        int Save();
        Task<int> SaveWithOutAuditAsync();
        void Begin();
        void Commit();
        void Rollback();
        void ApplyStateChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken());
        void SetModified(object entity);
        void SetAdd(object entity);
        void SetRemove(object entity);
        EntityEntry Entry(object entity);
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
        IQueryable<TEntity> FromSql<TEntity>(string sql, params object[] parameters) where TEntity : class;
        void DetachAllEntities();
        DbSet<AuditTrailCommon> AuditTrailCommon { get; set; }
    }
}
