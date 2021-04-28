using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;
using System.Collections.Generic;

namespace GSC.Respository.Project.Design
{
    public interface IProjectDesingTemplateRestrictionRepository : IGenericRepository<Data.Entities.Project.Design.ProjectDesingTemplateRestriction>
    {
        List<Data.Dto.Project.Design.ProjectDesingTemplateRestrictionDto> GetProjectDesingTemplateRestrictionDetails(int ProjectDesignTemplateId);
        void Save(List<Data.Entities.Project.Design.ProjectDesingTemplateRestriction> projectDesingTemplateRestriction);
        void updatePermission(List<Data.Entities.Project.Design.ProjectDesingTemplateRestriction> projectDesingTemplateRestriction);
    }
}