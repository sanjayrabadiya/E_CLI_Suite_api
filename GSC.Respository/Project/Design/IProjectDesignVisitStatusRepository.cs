using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;
using System.Collections.Generic;

namespace GSC.Respository.Project.Design
{
    public interface IProjectDesignVisitStatusRepository : IGenericRepository<ProjectDesignVisitStatus>
    {
        ProjectDesignVisitStatusDto GetProjectDesignVariableDetail(int visitId, ScreeningVisitStatus screeningVisitStatus);
        ProjectDesignVisitStatusDto GetProjectDesignVisitStatusById(int Id);
        List<ProjectDesignVisitStatusGridDto> GetVisits(int VisitId);
        string Duplicate(ProjectDesignVisitStatusDto objSave);

    }
}