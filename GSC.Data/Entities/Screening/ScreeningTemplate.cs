using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;

namespace GSC.Data.Entities.Screening
{
    public class ScreeningTemplate : BaseEntity
    {
        public int ProjectDesignTemplateId { get; set; }
        public ScreeningTemplateStatus Status { get; set; }
        public int? ParentId { get; set; }
        public int? Progress { get; set; }
        public short? ReviewLevel { get; set; }
        public short? StartLevel { get; set; }
        public int ScreeningVisitId { get; set; }
        public ScreeningVisit ScreeningVisit { get; set; }
        public ICollection<ScreeningTemplateValue> ScreeningTemplateValues { get; set; }

        [ForeignKey("ParentId")]
        public ICollection<ScreeningTemplate> Children { get; set; }

        public List<ScreeningTemplateReview> ScreeningTemplateReview { get; set; }
        public bool IsLocked { get; set; }
        public bool IsCompleteReview { get; set; }
        public bool IsDisable { get; set; }
        public int? RepeatSeqNo { get; set; }
        public ProjectDesignTemplate ProjectDesignTemplate { get; set; }
    }
}