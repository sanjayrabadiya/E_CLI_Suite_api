using GSC.Data.Entities.Audit;
using System;
using System.Collections.Generic;


namespace GSC.Data.Dto.Audit
{
    public class TrackerResult
    {
        public string EntityName { get; set; }
        public string FieldName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }

  


}
