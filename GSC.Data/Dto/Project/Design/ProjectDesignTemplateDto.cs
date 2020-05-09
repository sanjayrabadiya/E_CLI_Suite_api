using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Common;
using GSC.Helper;

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

        public int DomainId { get; set; }
        public bool IsRepeated { get; set; }

        public VariableTemplateDto VariableTemplate { get; set; }
        public DomainDto Domain { get; set; }
        public IList<ProjectDesignVariableDto> Variables { get; set; }
        public bool MyReview { get; set; }
        public string StatusName { get; set; }
        public bool IsSubmittedButton { get; set; }
        public int ScreeningTemplateId { get; set; }
        public int? ProductTypeId { get; set; }
        public ScreeningStatus Status { get; set; }
        public string EditCheckMessage { get; set; }

        public Operator RuleOperator { get; set; }
        public bool IsLocked { get; set; }
    }
}