using GSC.Common.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Project.Design
{
    public class ElectronicSignature : BaseEntity
    {
        public int ProjectDesignId { get; set; }
        public bool IsCompleteDesign { get; set; }
        public bool IsCompleteWorkflow { get; set; }
        public bool IsCompleteSchedule { get; set; }
        public bool IsCompleteEditCheck { get; set; }

    }
}
