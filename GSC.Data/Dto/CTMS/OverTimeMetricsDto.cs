using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GSC.Data.Dto.CTMS
{
    public class OverTimeMetricsDto: BaseDto
    {
        [Required]
        public int PlanMetricsId { get; set; }
        [Required(ErrorMessage = "Site is required.")]
        public int? ProjectId { get; set; }
        [Required(ErrorMessage = "PlanningType is required.")]
        public string PlanningType { get; set; }
        public bool? If_Active { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Planned { get; set; }
        public string TotalPlannig { get; set; }
        public int? ActualPlannedno { get; set; }
        public int? Actual { get; set; }
    }
    public class OverTimeMetricsGridDto : BaseAuditDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int PlanMetricsId { get; set; }   
        public bool? If_Active { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Planned { get; set; }
        public int? Actual { get; set; }
        public string SiteName { get; set; }
        public string PlanningType { get; set; }
        public string TotalPlannig { get; set; }
        public int? ActualPlannedno { get; set; }
    }
}
