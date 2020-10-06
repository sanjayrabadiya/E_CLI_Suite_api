using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Etmf
{
    public class ProjectWorkplaceArtificatedocument : BaseEntity
    {
        public int ProjectWorkplaceArtificateId { get; set; }
        public string DocumentName { get; set; }
        public ArtifactDocStatusType Status { get; set; }
        public string Version { get; set; }
        public string DocPath { get; set; }
        public int CompanyId { get; set; }
        public bool? IsAccepted { get; set; }
        public ProjectWorkplaceArtificate ProjectWorkplaceArtificate { get; set; }
    }
}
