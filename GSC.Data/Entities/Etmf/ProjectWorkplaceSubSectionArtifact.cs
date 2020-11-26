using GSC.Common.Base;
using System;
using System.Collections.Generic;
using GSC.Common.Common;

namespace GSC.Data.Entities.Etmf
{
    public class ProjectWorkplaceSubSectionArtifact : BaseEntity, ICommonAduit
    {
        public int ProjectWorkplaceSubSectionId { get; set; }
        public string ArtifactName { get; set; }
        public int CompanyId { get; set; }
        public List<ProjectWorkplaceSubSecArtificatedocument> ProjectWorkplaceSubSecArtificatedocument { get; set; }
        public ProjectWorkplaceSubSection ProjectWorkplaceSubSection { get; set; }
    }
}
