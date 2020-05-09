using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Master
{
    public class VariableValueRepository : GenericRespository<VariableValue, GscContext>, IVariableValueRepository
    {
        public VariableValueRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser) : base(uow,
            jwtTokenAccesser)
        {
        }
    }
}