using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GSC.Common.Common
{
    public class AuditValue
    {
        [Key]
        public string Value { get; set; }
    }
}
