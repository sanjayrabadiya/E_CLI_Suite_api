using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Project.Design
{
    public class ProjectDesignTemplateNote : BaseEntity, ICommonAduit
    {
        public int ProjectDesignTemplateId { get; set; }
        public string Note { get; set; }
        public bool IsPreview { get; set; }
        public ProjectDesignTemplate ProjectDesignTemplate { get; set; }
    }
}
