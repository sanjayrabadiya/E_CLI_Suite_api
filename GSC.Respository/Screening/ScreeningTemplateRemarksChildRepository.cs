using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Shared;

namespace GSC.Respository.Screening
{
    public class ScreeningTemplateRemarksChildRepository : GenericRespository<ScreeningTemplateRemarksChild>, IScreeningTemplateRemarksChildRepository
    {
        public ScreeningTemplateRemarksChildRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
        }
    }
}
