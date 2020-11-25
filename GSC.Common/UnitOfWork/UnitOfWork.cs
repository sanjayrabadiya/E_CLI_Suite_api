using System.Collections.Generic;
using System.Threading.Tasks;
using GSC.Shared;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace GSC.Common.UnitOfWork
{
    public class UnitOfWork<TContext> : IUnitOfWork<TContext>
        where TContext : IContext
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly TContext _context;
       
        public UnitOfWork(TContext context)
        {
            _context = context;
        }

        public int Save()
        {
            return _context.Save();
        }


        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public TContext Context => _context;
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