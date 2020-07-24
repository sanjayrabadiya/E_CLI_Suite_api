using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Etmf
{
    public class ProjectWorkplaceSubSecArtificatedocument : BaseEntity
    {
        public int ProjectWorkplaceSubSectionArtifactId { get; set; }
        public string DocumentName { get; set; }
        public string DocPath { get; set; }
        public int CompanyId { get; set; }

        public ProjectWorkplaceSubSectionArtifact ProjectWorkplaceSubSectionArtifact { get; set; }
    }
}
