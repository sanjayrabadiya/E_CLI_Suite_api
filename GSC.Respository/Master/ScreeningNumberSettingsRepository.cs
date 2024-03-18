using GSC.Common.GenericRespository;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;

namespace GSC.Respository.Master
{
    public class ScreeningNumberSettingsRepository : GenericRespository<ScreeningNumberSettings>, IScreeningNumberSettingsRepository
    {
        public ScreeningNumberSettingsRepository(IGSCContext context) : base(context)
        {
        }
    }
}
