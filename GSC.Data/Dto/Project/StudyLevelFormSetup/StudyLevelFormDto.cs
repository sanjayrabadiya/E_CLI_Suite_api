using GSC.Data.Dto.Master;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GSC.Data.Dto.Project.StudyLevelFormSetup
{
    public class StudyLevelFormDto : BaseDto
    {
        [Required(ErrorMessage = "Project Id is required.")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Module is required.")]
        public int AppScreenId { get; set; }

        [Required(ErrorMessage = "Activity is required.")]
        public int ActivityId { get; set; }

        [Required(ErrorMessage = "Form is required.")]
        public int VariableTemplateId { get; set; }
    }

    public class StudyLevelFormGridDto : BaseAuditDto
    {
        public string ProjectName { get; set; }
        public string AppScreenName { get; set; }
        public string Activity { get; set; }
        public string VariableTemplateName { get; set; }

    }

    public class CtmsMonitoringReportFormDto : BaseDto
    {
        public string ActivityName { get; set; }
        public string TemplateCode { get; set; }
        public int VariableTemplateId { get; set; }
        public string TemplateName { get; set; }
        public int DesignOrder { get; set; }
        public int CtmsMonitoringReportId { get; set; }
        public int ProjectId { get; set; }
        public bool IsSender { get; set; }
        public MonitoringReportStatus? ReportStatus { get; set; }
        public bool VariableDisable { get; set; }
        public List<string> Notes { get; set; }
        public List<VariableTemplateDetail> VariableTemplateDetails { get; set; }
        public IList<StudyLevelFormVariableDto> Variables { get; set; }

    }

    public class DesignVerificationApprovalTemplateDto : BaseDto
    {
        public string ActivityName { get; set; }
        public string TemplateCode { get; set; }
        public int VariableTemplateId { get; set; }
        public string TemplateName { get; set; }
        public int DesignOrder { get; set; }
        public int VerificationApprovalTemplateId { get; set; }
        public List<string> Notes { get; set; }
        public List<VariableTemplateDetail> VariableTemplateDetails { get; set; }
        public IList<VerificationApprovalVariableDto> Variables { get; set; }
    }

    public class CTMSVariableValueDto
    {
        public int Id { get; set; }
        public int StudyLevelFormVariableId { get; set; }
        public string ValueName { get; set; }
        public string ScreeningValue { get; set; }
        public int CtmsMonitoringReportVariableValueChildId { get; set; }
        public string ScreeningValueOld { get; set; }
        public string Label { get; set; }
        public int SeqNo { get; set; }
        public double? StudyVersion { get; set; }
        public double? InActiveVersion { get; set; }
        public short? LevelNo { get; set; }
        public bool IsDeleted { get; set; }
        public TableCollectionSource? TableCollectionSource { get; set; }
        public string Style { get; set; }
        public string OriginalValue { get; set; }
    }

}
