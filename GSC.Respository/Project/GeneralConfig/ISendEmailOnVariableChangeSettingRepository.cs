using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Generalconfig;
using GSC.Data.Entities.Project.Generalconfig;
using System.Collections.Generic;

namespace GSC.Respository.Project.GeneralConfig
{
    public interface ISendEmailOnVariableChangeSettingRepository : IGenericRepository<SendEmailOnVariableChangeSetting>
    {
        List<SendEmailOnVariableChangeSettingGridDto> GetList(int ProjectDesignId);
        string Duplicate(SendEmailOnVariableChangeSetting objSave);
    }
}