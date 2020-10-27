using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.InformConcent;
using GSC.Domain.Context;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Respository.InformConcent
{
    public class EconsentSetupRolesRepository : GenericRespository<EconsentSetupRoles, GscContext>, IEconsentSetupRolesRepository
    {
        public EconsentSetupRolesRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser) : base(uow, jwtTokenAccesser)
        {

        }
    }
}
