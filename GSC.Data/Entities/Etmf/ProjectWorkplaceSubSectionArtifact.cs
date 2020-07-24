using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Etmf
{
    public class ProjectWorkplaceSubSectionArtifact : BaseEntity
    {
        public int ProjectWorkplaceSubSectionId { get; set; }
        public string ArtifactName { get; set; }
        public int CompanyId { get; set; }
        public List<ProjectWorkplaceSubSecArtificatedocument> ProjectWorkplaceSubSecArtificatedocument { get; set; }
    }
}
