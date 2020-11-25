using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    }
}