using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Project.Schedule
{
    public class ProjectScheduleDto : BaseDto
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
}