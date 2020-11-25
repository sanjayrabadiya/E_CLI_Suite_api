using GSC.Common.Common;
using Microsoft.EntityFrameworkCore;

namespace GSC.Common
{
    public class StateHelpers
    {
        public static EntityState ConvertState(ViewModelState objstate)
        {
            switch (objstate)
            {
                case ViewModelState.Added:
                    return EntityState.Added;
                case ViewModelState.Modified:
                    return EntityState.Modified;
                case ViewModelState.Deleted:
                    return EntityState.Deleted;
                default:
                    return EntityState.Unchanged;
            }
        }
    }
}