using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Audit
{
    public class TrackerResult
    {
        public string EntityName { get; set; }
        public string FieldName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}
