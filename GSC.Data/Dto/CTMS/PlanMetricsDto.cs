﻿using GSC.Data.Entities.Common;
using System;
using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.CTMS
{
    public class PlanMetricsDto : BaseDto
    {
        [Required(ErrorMessage = "Project is required.")]
        public int ProjectId { get; set; }
        [Required]
        public string MetricsType { get; set; }
        [Required]
        public int Forecast { get; set; }
        public int? Planned { get; set; }
        public int? Actual { get; set; }
        [Required(ErrorMessage = "Plan Start Date is required.")]
        public DateTime PlnStartDate { get; set; }
        [Required(ErrorMessage = "Plan End Date is required.")]
        public DateTime PlnEndDate { get; set; }
    }

    public class PlanMetricsGridDto : BaseAuditDto
    {
        public int ProjectId { get; set; }
        public string ProjectCode { get; set; }
        public string MetricsType { get; set; }
        public string SiteName { get; set; }
        public int Forecast { get; set; }
        public int Planned { get; set; }
        public int? Actual { get; set; }
        public DateTime PlnStartDate { get; set; }
        public DateTime PlnEndDate { get; set; }

    }
}
