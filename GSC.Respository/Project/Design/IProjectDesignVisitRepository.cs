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
        IList<DropDownDto> GetVisitsByProjectDesignId(int projectDesignId);
        string Duplicate(ProjectDesignVisit objSave);
        IList<ProjectDesignVisitBasicDto> GetVisitAndTemplateByPeriordId(int projectDesignPeriodId);
        CheckVersionDto CheckStudyVersion(int projectDesignPeriodId);
    }
}