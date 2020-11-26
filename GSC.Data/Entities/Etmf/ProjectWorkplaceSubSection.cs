using GSC.Common.Base;
using System;
using System.Collections.Generic;
using GSC.Common.Common;

namespace GSC.Data.Entities.Etmf
{
   public class ProjectWorkplaceSubSection : BaseEntity, ICommonAduit
    {
        public int ProjectWorkplaceSectionId { get; set; }
        public string SubSectionName { get; set; }
        public int CompanyId { get; set; }

        public ProjectWorkplaceSection ProjectWorkplaceSection { get; set; }

        public List<ProjectWorkplaceSubSectionArtifact> ProjectWorkplaceSubSectionArtifact { get; set; }
    }
}
