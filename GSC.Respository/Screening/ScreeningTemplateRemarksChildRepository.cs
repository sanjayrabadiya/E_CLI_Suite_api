using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Shared;

namespace GSC.Respository.Screening
{
    public class ScreeningTemplateRemarksChildRepository : GenericRespository<ScreeningTemplateRemarksChild, GscContext>, IScreeningTemplateRemarksChildRepository
    {
        public ScreeningTemplateRemarksChildRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
        }
    }
}
