using GSC.Data.Entities.Common;
using GSC.Helper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Centeral
{
    public static class ContextHelper
    {
        public static void ApplyStateChanges(this DbContext context, IJwtTokenAccesser jwtTokenAccesser)
        {
            foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
            {
                var stateInfo = entry.Entity;
                if (stateInfo.ObjectState == ObjectState.Modified)
                {
                    // entry.Entity.ModifiedBy = UserInfo.UserName;
                    entry.Entity.ModifiedDate = DateTime.Now.ToUniversalTime();
                }
                else if (stateInfo.ObjectState == ObjectState.Added)
                {
                    // entry.Entity.CreatedBy = UserInfo.UserName;
                }
                else if (stateInfo.ObjectState == ObjectState.Deleted)
                {
                    // entry.Entity.DeletedBy = UserInfo.UserName;
                    entry.Entity.DeletedDate = DateTime.Now.ToUniversalTime();
                }

                entry.State = StateHelpers.ConvertState(stateInfo.ObjectState);
            }
        }
    }
}
