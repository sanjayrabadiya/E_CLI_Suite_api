using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Project.Workflow
{
    public class WorkflowVisitDto : BaseDto
    {
        public bool IsIndependent { get; set; }
        public int? ProjectWorkflowIndependentId { get; set; }
        public int? ProjectWorkflowLevelId { get; set; }
        public int[] ProjectDesignVisitIds { get; set; }
    }
}
