using System.Collections.Generic;
using GSC.Helper;

namespace GSC.Data.Dto.Screening
{

    public class ScreeningVisitTree
    {
        public int ScreeningVisitId { get; set; }
        public string ProjectDesignVisitName { get; set; }
        public int? VisitSeqNo { get; set; }
        public int ProjectDesignVisitId { get; set; }
        public string VisitStatusName { get; set; }
        public ScreeningVisitStatus VisitStatus { get; set; }
        public int? ParentScreeningVisitId { get; set; }
        public bool IsVisitRepeated { get; set; }
        public List<ScreeningTemplateTree> ScreeningTemplates { get; set; }
        public bool IsLocked { get; set; }
        public int? DesignOrder { get; set; }
    }

    public class ScreeningTemplateTree
    {
        public int ScreeningVisitId { get; set; }
        public int Id { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public int? ParentId { get; set; }
        public ScreeningTemplateStatus Status { get; set; }
        public string ProjectDesignTemplateName { get; set; }
        public decimal DesignOrder { get; set; }
        public int Progress { get; set; }
        public short? ReviewLevel { get; set; }
        public string StatusName { get; set; }
        public ICollection<ScreeningTemplateTree> Children { get; set; }
        public bool MyReview { get; set; }
        public bool IsLocked { get; set; }
        public int TotalQueries { get; set; }

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
        public bool IsLocked { get; set; }
        public bool IsNoCRF { get; set; }
        public bool IsDisable { get; set; }
        public int ProjectDesignVisitId { get; set; }
        public double? StudyVersion { get; set; }
        public short? LastReviewLevel { get; set; }

    }


    public class ScreeningTemplateRequest
    {
        public int Id { get; set; }
        public int ProjectDesignTemplateId { get; set; }

    }
}