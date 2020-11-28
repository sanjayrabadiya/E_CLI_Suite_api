using System;
using GSC.Common.Base;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Common
{
    public static class ContextHelper
    {
        public static void ApplyStateChanges(this DbContext context, IJwtTokenAccesser jwtTokenAccesser)
        {
            foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
            {
                var stateInfo = entry.Entity;

                //if (stateInfo.State == ViewModelState.Modified)
                //{
                //    // entry.Entity.ModifiedBy = UserInfo.UserName;
                //    entry.Entity.ModifiedDate = DateTime.Now.ToUniversalTime();
                //}
                //else if (stateInfo.State == ViewModelState.Added)
                //{
                //    // entry.Entity.CreatedBy = UserInfo.UserName;
                //}
                //else if (stateInfo.State == ViewModelState.Deleted)
                //{
                //    // entry.Entity.DeletedBy = UserInfo.UserName;
                //    entry.Entity.DeletedDate = DateTime.Now.ToUniversalTime();
                //}

                //entry.State = StateHelpers.ConvertState(stateInfo.State);
            }
        }
    }
}