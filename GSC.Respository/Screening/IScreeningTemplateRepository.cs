using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Dto.Report;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;

namespace GSC.Respository.Screening
{
    public interface IScreeningTemplateRepository : IGenericRepository<ScreeningTemplate>
    {
        List<MyReviewDto> GetScreeningTemplateReview();
        ScreeningTemplate TemplateRepeat(int id);
        void VisitRepeat(int projectDesignVisitId, int screeningEntryId);

        List<ScreeningTemplateDto> GetTemplateTree(int screeningEntryId, int? parentId,
            List<ScreeningTemplateValue> templateValues, WorkFlowLevelDto workFlowLevel);

        ProjectDesignTemplateDto GetScreeningTemplate(ProjectDesignTemplateDto designTemplateDto,
            ScreeningTemplateDto screeningTemplate);

        List<ScreeningTemplateLockUnlockDto> GetTemplatesLockUnlock(ScreeningTemplateLockUnlockParams lockUnlockParams);
        List<DashboardStudyStatusDto> GetDashboardStudyStatusByVisit(int projectId);

        List<DashboardStudyStatusDto> GetDashboardStudyStatusBySite(int projectId);

        IList<ReviewDto> GetReviewReportList(ReviewSearchDto filters);
        List<LockUnlockListDto> GetLockUnlockList(LockUnlockSearchDto lockUnlockParams);        
    }
}