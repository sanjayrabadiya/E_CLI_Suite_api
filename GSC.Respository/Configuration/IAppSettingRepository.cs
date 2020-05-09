using GSC.Common.GenericRespository;
using GSC.Data.Entities.Configuration;

namespace GSC.Respository.Configuration
{
    public interface IAppSettingRepository : IGenericRepository<AppSetting>
    {
        void Save<T>(T settings, int? companyId = null);
        T Get<T>(int? companyId = null);
    }
}