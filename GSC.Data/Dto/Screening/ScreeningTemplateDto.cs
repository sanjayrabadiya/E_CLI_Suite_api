using System.Collections.Generic;
using GSC.Data.Entities.Common;
using GSC.Helper;

namespace GSC.Data.Dto.Screening
{

    public class ScreeningVisitTree
    {
        public int ScreeningVisitId { get; set; }
        public string ScreeningVisitName { get; set; }
        public int? VisitSeqNo { get; set; }
        public int ProjectDesignVisitId { get; set; }
        public string VisitStatusName { get; set; }
        public ScreeningVisitStatus VisitStatus { get; set; }
        public int? ParentScreeningVisitId { get; set; }
        public bool IsVisitRepeated { get; set; }
        public List<ScreeningTemplateTree> ScreeningTemplates { get; set; }
        public bool IsLocked { get; set; }
        public int? DesignOrder { get; set; }
        //NA report
        public bool IsNA { get; set; }

        public bool IsPatientLevel { get; set; }
    }

    public class ScreeningTemplateTree
    {
        public int ScreeningVisitId { get; set; }
        public int Id { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public int? ParentId { get; set; }
        public ScreeningTemplateStatus Status { get; set; }
        public string ProjectDesignTemplateName { get; set; }
        public string ScreeningTemplateName { get; set; }
        public string DesignOrder { get; set; }
        public string DesignOrderForOrderBy { get; set; }
        public int Progress { get; set; }
        public short? ReviewLevel { get; set; }
        public string StatusName { get; set; }
        public ICollection<ScreeningTemplateTree> Children { get; set; }
        public bool MyReview { get; set; }
        public bool IsLocked { get; set; }
        public int TotalQueries { get; set; }
        public bool? IsTemplateSeqNo { get; set; }
        public bool? IsVariableSeqNo { get; set; }
        public string Label { get; set; }
        public string PreLabel { get; set; }
        public bool? IsHide { get; set; }
        //NA report
        public bool? IsNA { get; set; }

        
    }


    public class ScreeningTemplateBasic
    {
        public int Id { get; set; }
        public int ScreeningEntryId { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public ScreeningTemplateStatus Status { get; set; }
        public ScreeningPatientStatus? PatientStatus { get; set; }
        public ScreeningVisitStatus? VisitStatus { get; set; }
        public int? ParentId { get; set; }
        public int? DomainId { get; set; }
        public int ScreeningVisitId { get; set; }
        public int ProjectDesignId { get; set; }
        public short? ReviewLevel { get; set; }
        public int ProjectId { get; set; }
        public bool IsLocked { get; set; }
        public bool IsNoCRF { get; set; }
        public bool IsDisable { get; set; }
        public int ProjectDesignVisitId { get; set; }
        public double? StudyVersion { get; set; }
        public short? LastReviewLevel { get; set; }
        public Gender Gender { get; set; }
        public bool IsCompleteReview { get; set; }
        public bool? IsHide { get; set; }

    }

    public class ScreeningTemplateRepeat
    {
        public int ScreeningTemplateId { get; set; }
        public string ScreeningTemplateName { get; set; }
    }

    //NA report
    public class NAReportSearchDto : BaseDto
    {
        public int? SiteId { get; set; }
        public int?[] SubjectIds { get; set; }
        public int?[] VisitIds { get; set; }
        public int?[] TemplateIds { get; set; }
    }

    public class NAReportDto
    {
        public int ScreeningTemplateId { get; set; }
        public string Visit { get; set; }
        public string VisitStatus { get; set; }
        public string FormName { get; set; }
        public string FormStatus { get; set; }
        public string Initial { get; set; }
        public string ScreeningNo { get; set; }
        public string RandomizationNumber { get; set; }

    }
}