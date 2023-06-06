using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Generalconfig;
using GSC.Data.Entities.Project.Generalconfig;
using System.Collections.Generic;

namespace GSC.Respository.Project.GeneralConfig
{
    public interface IEmailConfigurationEditCheckRoleRepository : IGenericRepository<EmailConfigurationEditCheckRole>
    {
        void AddChileRecord(EmailConfigurationEditCheckRoleDto emailConfigurationEditCheckRoleDto);

        List<DropDownDto> GetProjectRightsRoleEmailTemplate(int projectId);
    }
}