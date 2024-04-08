using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Common;
using GSC.Helper;
using GSC.Shared.Extension;

namespace GSC.Data.Dto.Project.Design
{
    public class ProjectDesignTemplateDto : BaseDto
    {
        public int ProjectDesignVisitId { get; set; }

        [Required(ErrorMessage = "Template Code is required.")]
        public string TemplateCode { get; set; }

        [Required(ErrorMessage = "Template Name is required.")]
        public string TemplateName { get; set; }

        public string ActivityName { get; set; }
        public int VariableTemplateId { get; set; }
        public string DomainName { get; set; }
        public int? ParentId { get; set; }
        public List<int> ClonnedTemplateIds { get; set; }
        public int DesignOrder { get; set; }
        public string ProjectDesignTemplateName { get; set; }
        public string ProjectDesignVisitName { get; set; }
        private DateTime? _refTimeInterval { get; set; }

        public DateTime? RefTimeInterval
        {
            get => _refTimeInterval.UtcDate();
            set => _refTimeInterval = value.UtcDate();
        }
        public bool AllowActive { get; set; }
        public int DomainId { get; set; }
        public bool IsRepeated { get; set; }
        public bool IsParticipantView { get; set; }
        public VariableTemplateDto VariableTemplate { get; set; }
        public DomainDto Domain { get; set; }
        public IList<ProjectDesignVariableDto> Variables { get; set; }
        public int? ProductTypeId { get; set; }
        public double? InActiveVersion{ get; set; }
        public double? StudyVersion { get; set; }
        public string Label { get; set; }
        public string PreLabel { get; set; }
    }


    public class DesignScreeningTemplateDto
    {
        public int Id { get; set; }
        public int ProjectDesignVisitId { get; set; }
        public string ActivityName { get; set; }
        public string TemplateCode { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public string TemplateName { get; set; }
        public string ProjectDesignVisitName { get; set; }
        public string ScreeningVisitName { get; set; }
        public string DesignOrder { get; set; }
        public int? DomainId { get; set; }
        public string DomainName { get; set; }
        public bool IsRepeated { get; set; }
        public IList<DesignScreeningVariableDto> Variables { get; set; }
        public bool MyReview { get; set; }
        public string StatusName { get; set; }
        public bool IsSubmittedButton { get; set; }
        public bool IsUnSubmittedButton { get; set; }
        public bool IsUnReviewedButton { get; set; }
        public int ScreeningTemplateId { get; set; }
        public int? ParentId { get; set; }
        public ScreeningTemplateStatus Status { get; set; }
        public bool IsSchedule { get; set; }
        public string EditCheckMessage { get; set; }
        public bool IsWarning { get; set; }
        public bool IsLocked { get; set; }
        public List<string> Notes { get; set; }
        public List<string> NotesAlign { get; set; }
        public int VariableTemplateId { get; set; }
        public bool? IsTemplateSeqNo { get; set; }
        public bool? IsVariableSeqNo { get; set; }
        public bool IsNA { get; set; }
    }

    public class CloneTemplateDto
    {
        public int Id { get; set; }
        public List<int> ClonnedTemplateIds { get; set; }
    }
    public class ProjectDesignTemplateSiteAccessDto
    {
        public int TemplateId { get; set; }
        public List<int> SiteIds { get; set; }
    }
}