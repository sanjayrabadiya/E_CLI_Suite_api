using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Shared;

namespace GSC.Respository.Master
{
    public class VariableTemplateDetailRepository : GenericRespository<VariableTemplateDetail, GscContext>,
        IVariableTemplateDetailRepository
    {
        public VariableTemplateDetailRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
        }
    }
}