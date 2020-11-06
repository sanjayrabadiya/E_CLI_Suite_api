using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;

namespace GSC.Respository.Project.Design
{
    public interface IProjectDesignVisitStatusRepository : IGenericRepository<ProjectDesignVisitStatus>
    {
        ProjectDesignVisitStatusDto GetProjectDesignVariableDetail(int visitId, ScreeningVisitStatus screeningVisitStatus);
        ProjectDesignVisitStatusDto GetProjectDesignVisitStatusByTemplate(int ProjectDesignTemplateId);
    }
}