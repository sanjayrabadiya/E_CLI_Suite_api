using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Dto.Report;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;

namespace GSC.Respository.Screening
{
    public interface IScreeningTemplateValueRepository : IGenericRepository<ScreeningTemplateValue>
    {
        void UpdateVariableOnSubmit(int projectDesignTemplateId, int screeningTemplateId,
            List<int> projectDesignVariableId);

        QueryStatusDto GetQueryStatusCount(int screeningTemplateId);

        QueryStatusDto GetQueryStatusByModel(List<Data.Dto.Screening.ScreeningTemplateValueBasic> screeningTemplateValue,
            int screeningTemplateId);

        void DeleteChild(int screeningTemplateValueId);
        void UpdateChild(List<ScreeningTemplateValueChild> children);
        string CheckCloseQueries(int screeningTemplateId);
        string GetValueForAudit(ScreeningTemplateValueDto screeningTemplateValueDto);
        bool IsFitness(int screeningTemplateId);
        bool IsDiscontinued(int screeningTemplateId);
        List<PeriodQueryStatusDto> GetQueryStatusByPeridId(int projectDesignPeriodId);

        int GetQueryCountByVisitId(int projectDesignVisitId, int screeningEntryid);

        List<DashboardQueryStatusDto> GetQueryByProjectDesignId(int projectDesignId);
        CommonDto GetProjectDatabaseEntries(ProjectDatabaseSearchDto filters);
        List<QueryStatusDto> GetQueryStatusBySubject(int screeningEntryId);
    }
}