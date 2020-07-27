using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Etmf
{
   public class ProjectWorkplaceSubSection : BaseEntity
    {
        public int ProjectWorkplaceSectionId { get; set; }
        public string SubSectionName { get; set; }
        public int CompanyId { get; set; }

        public ProjectWorkplaceSection ProjectWorkplaceSection { get; set; }

        public List<ProjectWorkplaceSubSectionArtifact> ProjectWorkplaceSubSectionArtifact { get; set; }
    }
}
