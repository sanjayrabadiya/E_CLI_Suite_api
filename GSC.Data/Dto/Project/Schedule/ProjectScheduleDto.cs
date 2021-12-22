using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Project.Schedule
{
    public class ProjectScheduleDto : BaseAuditDto
    {
        public int? ProjectId { get; set; }

        [Required(ErrorMessage = "Project Name is required.")]
        public int ProjectDesignId { get; set; }

        [Required(ErrorMessage = "Project Period is required.")]
        public int ProjectDesignPeriodId { get; set; }

        [Required(ErrorMessage = "Project Visit is required.")]
        public int ProjectDesignVisitId { get; set; }

        [Required(ErrorMessage = "Reference Template is required.")]
        public int ProjectDesignTemplateId { get; set; }

        [Required(ErrorMessage = "Variable is required.")]
        public int ProjectDesignVariableId { get; set; }

        public IList<ProjectScheduleTemplateDto> Templates { get; set; }
        public string ProjectName { get; set; }
        public string PeriodName { get; set; }
        public string VisitName { get; set; }
        public string TemplateName { get; set; }
        public string VariableName { get; set; }
        public bool IsLock { get; set; }
        public string AutoNumber { get; set; }
        public int? CompanyId { get; set; }
    }

    public class ProjectScheduleReportDto : BaseAuditDto
    {
        public string ProjectCode { get; set; }
        public string AutoNumber { get; set; }
        public string ReferencePeriod { get; set; }
        public string ReferenceVisit { get; set; }
        public string ReferenceTemplate { get; set; }
        public string ReferenceVariable { get; set; }
        public string TargetPeriod { get; set; }
        public string TargetVisit { get; set; }
        public string TargetTemplate { get; set; }
        public string TargetVariable { get; set; }
        public string Operator { get; set; }
        public int? RefTimeInterValHH { get; set; }
        public int? RefTimeInterValMM { get; set; }
        public int? RefTimeInterNoOfDay { get; set; }
        public int PositiveDeviation { get; set; }
        public int NegativeDeviation { get; set; }
        public string Message { get; set; }
    }

    public class ProjectScheduleSetupSearchDto : BaseDto
    {
        public int ProjectId { get; set; }
        public int? SiteId { get; set; }
    }
}