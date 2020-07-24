using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Etmf
{
    public class ProjectWorkplaceArtificate : BaseEntity
    {
        public int ProjectWorkplaceSectionId { get; set; }
        public int EtmfArtificateMasterLbraryId { get; set; }
        public int? CompanyId { get; set; }
        public EtmfArtificateMasterLbrary EtmfArtificateMasterLbrary { get; set; }

        public List<ProjectWorkplaceArtificatedocument> ProjectWorkplaceArtificatedocument { get; set; }
    }
}
