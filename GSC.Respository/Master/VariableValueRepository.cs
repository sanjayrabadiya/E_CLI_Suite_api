using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Master
{
    public class VariableValueRepository : GenericRespository<VariableValue>, IVariableValueRepository
    {
        public VariableValueRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser) : base(context)
        {
        }
    }
}