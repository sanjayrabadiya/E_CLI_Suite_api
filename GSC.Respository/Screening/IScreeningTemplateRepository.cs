using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Dto.Report;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Helper;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Respository.Screening
{
    public interface IScreeningTemplateRepository : IGenericRepository<ScreeningTemplate>
    {
        List<MyReviewDto> GetScreeningTemplateReview();
        ScreeningTemplate TemplateRepeat(int id);
        List<ScreeningTemplateTree> GetTemplateTree(int screeningEntryId, WorkFlowLevelDto workFlowLevel);
        List<TemplateText> GetTemplateData(int ProjectId, int VisitId);

        DesignScreeningTemplateDto GetScreeningTemplate(DesignScreeningTemplateDto designTemplateDto,
            int screeningTemplateId);
        IList<ReviewDto> GetReviewReportList(ReviewSearchDto filters);
        List<LockUnlockListDto> GetLockUnlockList(LockUnlockSearchDto lockUnlockParams);
        
        ScreeningTemplateValueSaveBasics ValidateVariableValue(ScreeningTemplateValue screeningTemplateValue, List<EditCheckIds> EditCheckIds, CollectionSources? collectionSource);

        void SubmitReviewTemplate(int screeningTemplateId, bool isFromLockUnLock);
        bool IsRepated(int screeningTemplateId);
        int GetProjectDesignId(int screeningTemplateId);
        int GeScreeningEntryId(int screeningTemplateId);
        string GetStatusName(ScreeningTemplateBasic basicDetail, bool myReview, WorkFlowLevelDto workFlowLevel);
        IList<DropDownDto> GetTemplateByLockedDropDown(LockUnlockDDDto lockUnlockDDDto);
        IList<VisitDeviationReport> GetVisitDeviationReport(VisitDeviationReportSearchDto filters);

        bool CheckLockedProject(int ProjectId);
        IList<ScheduleDueReport> GetScheduleDueReport(ScheduleDueReportSearchDto filters);

        void SendVariableEmail(ScreeningTemplateValueDto screeningTemplateValueDto, ScreeningTemplateValueQueryDto screeningTemplateValueQueryDto);
        //Screening Grid view
        IList<DesignScreeningTemplateDto> GetScreeningGridView(DesignScreeningTemplateDto designTemplateDto, int ScreeningTemplateId);

        List<TemplateStatusList> GetTemplateStatus(int ProjectId, int VisitId, int ScreeningEntryId);

        void DeleteRepeatVisitTemplate(int Id);
        IList<ReviewDto> GetScreeningReviewReportList(ScreeningQuerySearchDto filters);
    }
}