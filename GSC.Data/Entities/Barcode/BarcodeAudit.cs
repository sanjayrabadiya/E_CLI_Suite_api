using GSC.Common.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Barcode
{
    public class BarcodeAudit : BaseEntity
    {
        public int BarcodeId { get; set; }
        public string TableName { get; set; }
        public string Action { get; set; }
        public int? AuditReasonId { get; set; }
        public string Note { get; set; }
        public int? UserId { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
    }
}
