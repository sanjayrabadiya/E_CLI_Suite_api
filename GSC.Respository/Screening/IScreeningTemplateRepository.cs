using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Dto.Report;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Helper;

namespace GSC.Respository.Screening
{
    public interface IScreeningTemplateRepository : IGenericRepository<ScreeningTemplate>
    {
        List<MyReviewDto> GetScreeningTemplateReview();
        ScreeningTemplate TemplateRepeat(int id);
        List<ScreeningTemplateDto> GetTemplateTree(int screeningEntryId, List<Data.Dto.Screening.ScreeningTemplateValueBasic> templateValues, WorkFlowLevelDto workFlowLevel);

        DesignScreeningTemplateDto GetScreeningTemplate(DesignScreeningTemplateDto designTemplateDto,
            int screeningTemplateId);
        IList<ReviewDto> GetReviewReportList(ReviewSearchDto filters);
        List<LockUnlockListDto> GetLockUnlockList(LockUnlockSearchDto lockUnlockParams);

        ScreeningTemplateValueSaveBasics ValidateVariableValue(ScreeningTemplateValue screeningTemplateValue, List<EditCheckIds> EditCheckIds, CollectionSources? collectionSource);

        void SubmitReviewTemplate(int screeningTemplateId,bool isLockUnLock);

        int GetProjectDesignId(int screeningTemplateId);
        int GeScreeningEntryId(int screeningTemplateId);
    }
}