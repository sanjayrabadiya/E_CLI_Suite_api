using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GSC.Data.Dto.CTMS
{
    public class WeekEndMasterDto: BaseDto
    {
        [Required(ErrorMessage = "Study is required.")]
        public int ProjectId { get; set; }
        public int? SiteId { get; set; }
        [Required(ErrorMessage = "Weekoff is required.")]
        public DayType? AllWeekOff { get; set; }
        [Required(ErrorMessage = "Frequency is required.")]
        public FrequencyType Frequency { get; set; }
        public bool? IsSite { get; set; }

    }

    public class WeekEndGridDto : BaseAuditDto
    {
        public string ProjectCode { get; set; }
        public string SiteCode { get; set; }
        public string AllWeekOff { get; set; }
        public string Frequency { get; set; }
        public bool IsSite { get; set; }
    }
}
