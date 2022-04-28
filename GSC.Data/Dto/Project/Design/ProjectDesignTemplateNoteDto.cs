using GSC.Data.Entities.Common;
using GSC.Data.Entities.Project.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Project.Design
{
    public class ProjectDesignTemplateNoteDto : BaseDto
    {
        public int ProjectDesignTemplateId { get; set; }
        public string Note { get; set; }
        public bool IsPreview { get; set; }
        public bool? IsBottom { get; set; }
        public string ProjectDesignTemplateName { get; set; }

        public ProjectDesignTemplate ProjectDesignTemplate { get; set; }
    }

    public class ProjectDesignTemplateNoteGridDto : BaseAuditDto
    {
        public int ProjectDesignTemplateId { get; set; }
        public string ProjectDesignTemplateName { get; set; }
        public string Note { get; set; }
        public bool? IsBottom { get; set; }
        public bool IsPreview { get; set; }

    }
}
