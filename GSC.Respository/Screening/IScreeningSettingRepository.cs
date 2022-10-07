using GSC.Common.GenericRespository;
using GSC.Data.Dto.Screening;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.Screening;
using GSC.Data.Entities.UserMgt;

namespace GSC.Respository.Screening
{
    public interface IScreeningSettingRepository : IGenericRepository<ScreeningSetting>
    {
        ScreeningSettingDto GetProjectDefaultData();
    }
}