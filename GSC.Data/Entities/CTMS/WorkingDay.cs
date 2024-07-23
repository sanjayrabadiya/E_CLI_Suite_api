using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
namespace GSC.Data.Entities.CTMS
{
    public class WorkingDay : BaseEntity, ICommonAduit
    {
        public int ParentProjectId { get; set; }
        public string Description { get; set; }
        public bool? IsSite { get; set; }
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
        public string  TotalHour { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
        public List<SiteTypes> siteTypes { get; set; } = null;
    }
}
