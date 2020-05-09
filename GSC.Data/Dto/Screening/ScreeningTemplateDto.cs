using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Screening;
using GSC.Helper;

namespace GSC.Data.Dto.Screening
{
    public class ScreeningTemplateDto : BaseDto
    {
        [Required(ErrorMessage = "Screening is required.")]
        public int ScreeningEntryId { get; set; }

        [Required(ErrorMessage = "Template is required.")]
        public int ProjectDesignTemplateId { get; set; }

        public ScreeningStatus Status { get; set; }
        public int? ParentId { get; set; }
        public int ProjectDesignVisitId { get; set; }
        public int? RepeatedVisit { get; set; }
        public string ProjectDesignTemplateName { get; set; }
        public string ProjectDesignTemplateNameWithIndex { get; set; }
        public int DesignOrder { get; set; }
        public string ProjectDesignVisitName { get; set; }
        public string StatusName { get; set; }
        public int Progress { get; set; }
        public short? ReviewLevel { get; set; }
        public bool IsParent { get; set; }
        public short? StartLevel { get; set; }
        public ProjectDesignTemplateDto ProjectDesignTemplate { get; set; }
        public ICollection<ScreeningTemplateValueDto> ScreeningTemplateValues { get; set; }
        public ICollection<ScreeningTemplateDto> Children { get; set; }
        public QueryStatusDto TemplateQueryStatus { get; set; }
        public bool IsStartReview { get; set; }
        public bool IsCompleteReview { get; set; }
        public bool MyReview { get; set; }
        public bool IsVisitRepeated { get; set; }
        public int? EditCheckDetailId { get; set; }
        public bool IsEditChecked { get; set; }
        public bool IsDisable { get; set; }
        public bool IsLocked { get; set; }
    }
}