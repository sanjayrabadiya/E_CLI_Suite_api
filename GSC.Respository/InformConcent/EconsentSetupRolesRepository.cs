using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.InformConcent;
using GSC.Domain.Context;
using GSC.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Respository.InformConcent
{
    public class EconsentSetupRolesRepository : GenericRespository<EconsentSetupRoles>, IEconsentSetupRolesRepository
    {
        public EconsentSetupRolesRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser) : base(context)
        {

        }
    }
}
