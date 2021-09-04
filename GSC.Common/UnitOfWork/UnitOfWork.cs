using System.Threading.Tasks;

namespace GSC.Common.UnitOfWork
{
    public class UnitOfWork<TContext> : IUnitOfWork<TContext>
        where TContext : IContext
    {
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

       
    }
}