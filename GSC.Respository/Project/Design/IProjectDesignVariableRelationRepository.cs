using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;

namespace GSC.Respository.Project.Design
{
    public interface IProjectDesignVariableRelationRepository : IGenericRepository<ProjectDesignVariableRelation>
    {
        ProjectDesignVariableDisplayRelationDto GetProjectDesignVariableRelationById(int Id);
    }
}