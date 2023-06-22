using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;
using System.Collections.Generic;

namespace GSC.Respository.Project.Design
{
    public interface IProjectDesignVisitRestrictionRepository : IGenericRepository<ProjectDesignVisitRestriction>
    {
        List<ProjectDesignVisitRestrictionDto> GetProjectDesignVisitRestrictionDetails(int ProjectDesignVisitId);
        void Save(List<ProjectDesignVisitRestriction> projectDesignVisitRestriction);
        void updatePermission(List<ProjectDesignVisitRestriction> projectDesignVisitRestriction);
    }
}