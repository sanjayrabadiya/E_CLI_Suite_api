using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Project.Design;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace GSC.Data.Entities.AdverseEvent
{
    public class AEReportingValue : BaseEntity, ICommonAduit
    {
        public int AEReportingId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public string Value { get; set; }
       
        public ProjectDesignVariable ProjectDesignVariable { get; set; }
        
    }
}
