using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;

namespace GSC.Respository.Project.Design
{
    public interface IProjectDesignTemplateRepository : IGenericRepository<ProjectDesignTemplate>
    {
        ProjectDesignTemplate GetTemplate(int id);
        IList<DropDownDto> GetTemplateDropDown(int projectDesignVisitId);

        IList<DropDownDto> GetTemplateDropDownForProjectSchedule(int projectDesignVisitId);
        IList<DropDownDto> GetClonnedTemplates(int id);
        IList<ProjectDesignTemplate> GetTemplateIdsByPeriordId(int projectDesignPeriodId);

        IList<DropDownDto> GetTemplateDropDownByPeriodId(int projectDesignPeriodId,
            VariableCategoryType variableCategoryType);

        IList<DropDownDto> GetTemplateDropDownAnnotation(int projectDesignVisitId);

        // IList<DropDownDto> GetTemplateByLockedDropDown(int projectDesignVisitId, int projectId, bool isLock);
        IList<DropDownDto> GetTemplateByLockedDropDown(LockUnlockDDDto lockUnlockDDDto);

        IList<ProjectDesignTemplate> GetAllTemplate(int projectId, int? periodId);
    }
}