using System;
using System.Collections.Generic;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Common;
using GSC.Helper;

namespace GSC.Data.Dto.Project.Schedule
{
    public class ProjectScheduleTemplateDto : BaseDto
    {
        public int ProjectScheduleId { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public int ProjectDesignVariableId { get; set; }

        public int? PositiveDeviation { get; set; }
        public int? NegativeDeviation { get; set; }
        public string TemplateName { get; set; }
        public int TemplateDesignOrder { get; set; }
        public IList<DropDownDto> Variables { get; set; }
        public bool IsVariablLoaded { get; set; }
        public int ProjectDesignPeriodId { get; set; }
        public int ProjectDesignVisitId { get; set; }
        public int? NoOfDay { get; set; }

        public int? HH { get; set; }
        public int? MM { get; set; }

        public ProjectScheduleOperator? Operator { get; set; }

        public string PeriodName { get; set; }
        public string VisitName { get; set; }

        public string VariableName { get; set; }

        public string Message { get; set; }
        public string OperatorName { get; set; }

        public DateTime? DeletedDate { get; set; }
    }
}