using GSC.Common.GenericRespository;
using GSC.Data.Entities.Configuration;

namespace GSC.Respository.Configuration
{
    public interface IUploadSettingRepository : IGenericRepository<UploadSetting>
    {
        string GetImagePath();
        string GetDocumentPath();
        string GetWebImageUrl();
        string GetWebDocumentUrl();
        object getWebImageUrl();
        bool IsUnlimitedUploadlimit();
        string ValidateUploadlimit(int ProjectId);
    }
}