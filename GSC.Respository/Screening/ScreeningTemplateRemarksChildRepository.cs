using GSC.Common.GenericRespository;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;

namespace GSC.Respository.Screening
{
    public class ScreeningTemplateRemarksChildRepository : GenericRespository<ScreeningTemplateRemarksChild>, IScreeningTemplateRemarksChildRepository
    {
        public ScreeningTemplateRemarksChildRepository(IGSCContext context)
            : base(context)
        {
        }
    }
}
