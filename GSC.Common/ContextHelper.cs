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
                //Empty code Block
            }
        }
    }
}