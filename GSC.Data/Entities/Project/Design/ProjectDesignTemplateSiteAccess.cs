using System.Collections.Generic;
using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.LanguageSetup;
using GSC.Data.Entities.Master;

namespace GSC.Data.Entities.Project.Design
{
    public class ProjectDesignTemplateSiteAccess : BaseEntity
    {
        public int ProjectId { get; set; }

        public int ProjectDesignTemplateId { get; set; }

        public GSC.Data.Entities.Master.Project Project { get; set; }
    }
}