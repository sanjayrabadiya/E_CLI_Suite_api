using GSC.Common.GenericRespository;
using GSC.Data.Dto.Configuration;
using GSC.Data.Entities.Configuration;

namespace GSC.Respository.UserMgt
{
    public interface IReleaseSettingRepository : IGenericRepository<ReleaseSetting>
    {
        ReleaseSettingDto GetVersionNum();
    }
}