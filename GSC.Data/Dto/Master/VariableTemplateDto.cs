using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using GSC.Helper;

namespace GSC.Data.Dto.Master
{
    public class VariableTemplateDto : BaseDto
    {
        [Required(ErrorMessage = "Template Code is required.")]
        public string TemplateCode { get; set; }

        public int ActivityId { get; set; }

        [Required(ErrorMessage = "Template Name is required.")]
        public string TemplateName { get; set; }

        [Required(ErrorMessage = "Domain Name is required.")]
        public int DomainId { get; set; }

        public bool IsRepeated { get; set; }
        public ActivityMode ActivityMode { get; set; }
        public AuditModule? ModuleId { get; set; }
        public List<VariableTemplateDetailDto> VariableTemplateDetails { get; set; }
        public IList<VariableTemplateNoteDto> Notes { get; set; }
        public int? CompanyId { get; set; }
        public Activity Activity { get; set; }
        // public bool SystemType { get; set; }
    }

    public class VariableTemplateGridDto : BaseAuditDto
    {
        public string TemplateCode { get; set; }
        public string ActivityName { get; set; }
        public string ModuleName { get; set; }
        public string DomainName { get; set; }
        public string ActivityMode { get; set; }
        public string TemplateName { get; set; }
        public bool IsRepeated { get; set; }
        // public bool SystemType { get; set; }

    }

    public class DesignVerificationApprovalTemplateDto
    {
        public int Id { get; set; }
        public string ActivityName { get; set; }
        public string TemplateCode { get; set; }
        public int VariableTemplateId { get; set; }
        public string VariableTemplateName { get; set; }
        public int DesignOrder { get; set; }
        public IList<VerificationApprovalVariableDto> Variables { get; set; }
        public int VerificationApprovalTemplateId { get; set; }
        public List<string> Notes { get; set; }
        public List<VariableTemplateDetail> VariableTemplateDetails { get; set; }
    }

    public class MonitoringReportTemplateDto : BaseDto
    {
        public string ActivityName { get; set; }
        public string TemplateCode { get; set; }
        public int VariableTemplateId { get; set; }
        public string VariableTemplateName { get; set; }
        public int DesignOrder { get; set; }
        public int ManageMonitoringReportId { get; set; }
        public int ProjectId { get; set; }
        public bool IsSender { get; set; }
        public MonitoringReportStatus StatusId { get; set; }
        public List<string> Notes { get; set; }
        public List<VariableTemplateDetail> VariableTemplateDetails { get; set; }
        public IList<ManageMonitoringVariableDto> Variables { get; set; }

    }
}