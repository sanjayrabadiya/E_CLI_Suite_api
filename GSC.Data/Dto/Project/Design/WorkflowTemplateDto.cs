using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Project.Design
{
    public class WorkflowTemplateDto : BaseDto
    {
        public int ProjectDesignTemplateId { get; set; }
        public int[] LevelNos { get; set; }
    }
}
