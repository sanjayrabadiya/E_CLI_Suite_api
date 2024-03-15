using GSC.Common.GenericRespository;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;

namespace GSC.Respository.Screening
{
    public interface IScreeningSettingRepository : IGenericRepository<ScreeningSetting>
    {
        ScreeningSettingDto GetProjectDefaultData();
    }
}