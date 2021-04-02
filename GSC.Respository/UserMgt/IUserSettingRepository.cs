using GSC.Common.GenericRespository;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;

namespace GSC.Respository.UserMgt
{
    public interface IUserSettingRepository : IGenericRepository<UserSetting>
    {
        UserSettingDto GetProjectDefaultData();
    }
}