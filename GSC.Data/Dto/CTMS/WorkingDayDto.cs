using GSC.Data.Entities.Common;
using GSC.Data.Entities.CTMS;
using System;
using System.Collections.Generic;
namespace GSC.Data.Dto.CTMS
{
    public class WorkingDayDto : BaseDto
    {
        public int? ParentProjectId { get; set; }
        public bool? IsSite { get; set; }
        public string Description { get; set; }
        public bool? Sunday { get; set; }
        public DateTime? SunStartTime { get; set; }
        public DateTime? SunEndTime { get; set; }
        public string SunTotalHour { get; set; }
        public bool? Monday { get; set; }
        public DateTime? MonStartTime { get; set; }
        public DateTime? MonEndTime { get; set; }
        public string MonTotalHour { get; set; }
        public bool? Tuesday { get; set; }
        public DateTime? TueStartTime { get; set; }
        public DateTime? TueEndTime { get; set; }
        public string TueTotalHour { get; set; }
        public bool? Wednesday { get; set; }
        public DateTime? WedStartTime { get; set; }
        public DateTime? WedEndTime { get; set; }
        public string WedTotalHour { get; set; }
        public bool? Thursday { get; set; }
        public DateTime? ThuStartTime { get; set; }
        public DateTime? ThuEndTime { get; set; }
        public string ThuTotalHour { get; set; }
        public bool? Friday { get; set; }
        public DateTime? FriStartTime { get; set; }
        public DateTime? FriEndTime { get; set; }
        public string FriTotalHour { get; set; }
        public bool? Saturday { get; set; }
        public DateTime? SatStartTime { get; set; }
        public DateTime? SatEndTime { get; set; }
        public string SatTotalHour { get; set; }
        public string TotalHour { get; set; }
        public List<SiteTypes>? siteTypes { get; set; }
        
    }
    public class WorkingDayListDto : BaseAuditDto
    {
        public int? ParentProjectId { get; set; }
        public string ProjectCode { get; set; }
        public string SiteCode { get; set; }
        public bool? IsSite { get; set; }
        public string Description { get; set; }
        public bool? Sunday { get; set; }
        public bool? Monday { get; set; }
        public bool? Tuesday { get; set; }
        public bool? Wednesday { get; set; }
        public bool? Thursday { get; set; }
        public bool? Friday { get; set; }
        public bool? Saturday { get; set; }
    }
}
