using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GSC.Data.Entities.Custom
{
   public class AuditValue
    {
        [Key]
        public string Value { get; set; }
    }
}
