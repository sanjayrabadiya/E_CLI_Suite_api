using GSC.Data.Entities.Common;
using System;
namespace GSC.Data.Dto.CTMS
{
    public class HolidayMasterGridDto : BaseAuditDto
    {
        public string ProjectCode { get; set; }
        public string SiteCode { get; set; }
        public string HolidayName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool IsSite { get; set; }
    }
}
