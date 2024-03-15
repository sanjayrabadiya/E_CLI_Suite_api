using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Project.Design;

namespace GSC.Respository.Project.Design
{
    public interface IProjectDesignVisitRepository : IGenericRepository<ProjectDesignVisit>
    {
        ProjectDesignVisit GetVisit(int id);
        IList<DropDownDto> GetVisitDropDown(int projectDesignPeriodId);
        IList<DropDownDto> GetVisitDropDownByProjectId(int ProjectId);
        IList<DropDownDto> GetVisitsByProjectDesignId(int projectDesignId);
        string Duplicate(ProjectDesignVisit objSave);
        List<ProjectDesignVisitBasicDto> GetVisitAndTemplateByPeriordId(int projectDesignPeriodId);
        CheckVersionDto CheckStudyVersion(int projectDesignPeriodId);
        IList<ProjectDesignVisitDto> GetVisitList(int projectDesignPeriodId);
        IList<DropDownDto> GetVisitsforWorkflowVisit(int projectDesignId);

        string ValidationVisitIWRS(ProjectDesignVisit visit);
    }
}