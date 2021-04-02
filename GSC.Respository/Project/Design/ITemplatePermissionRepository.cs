using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;
using System.Collections.Generic;

namespace GSC.Respository.Project.Design
{
    public interface ITemplatePermissionRepository : IGenericRepository<TemplatePermission>
    {
        List<TemplatePermissionDto> GetTemplatePermissionDetails(int ProjectDesignTemplateId);
        void Save(List<TemplatePermission> templatePermission);
        void updatePermission(List<TemplatePermission> TemplatePermission);
    }
}