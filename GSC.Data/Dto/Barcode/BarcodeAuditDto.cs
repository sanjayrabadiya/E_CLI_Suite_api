using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Barcode
{
    public class BarcodeAuditDto : BaseDto
    {
        public string VolunteerNo { get; set; }
        public string BarcodeString { get; set; }
        public string Note { get; set; }
        public string Action { get; set; }
        public int? UserRoleId { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreateUser { get; set; }
        public int? ReasonId { get; set; }
        public string ReasonOth { get; set; }
        public string ReasonName { get; set; }

    }
}
