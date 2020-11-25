using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Shared.Audit
{
    public class AuditTrailViewModel
    {
        public string TableName { get; set; }
        public int RecordId { get; set; }
        public string Action { get; set; }
        public string ColumnName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string Reason { get; set; }
        public string UserRole { get; set; }
        public string ReasonOth { get; set; }
    }
}
