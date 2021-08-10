using System.Collections.Generic;
using System.Threading.Tasks;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;

namespace GSC.Respository.Project.Design
{
    public interface IProjectDesignTemplateRepository : IGenericRepository<ProjectDesignTemplate>
    {
        ProjectDesignTemplate GetTemplateClone(int id);
        IList<DropDownDto> GetTemplateDropDown(int projectDesignVisitId);

        IList<DropDownDto> GetTemplateDropDownForProjectSchedule(int projectDesignVisitId, int? collectionSource, int? refVariable);
        IList<DropDownDto> GetClonnedTemplateDropDown(int id);

        IList<DropDownDto> GetTemplateDropDownByPeriodId(int projectDesignPeriodId,
            VariableCategoryType variableCategoryType);

        DesignScreeningTemplateDto GetTemplate(int id);


        IList<DropDownDto> GetTemplateDropDownForVisitStatus(int projectDesignVisitId);

        Task<bool> IsTemplateExits(int projectDesignId);

        CheckVersionDto CheckStudyVersion(int projectDesignVisitId);
    }
}