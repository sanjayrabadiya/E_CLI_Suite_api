using GSC.Common.Base;
using System;
using System.Collections.Generic;
using GSC.Common.Common;

namespace GSC.Data.Entities.Etmf
{
    public class ProjectWorkplaceArtificate : BaseEntity, ICommonAduit
    {
        public int ProjectWorkplaceSectionId { get; set; }
        public int EtmfArtificateMasterLbraryId { get; set; }
        public int? CompanyId { get; set; }
        public int? ParentArtificateId { get; set; }
        public EtmfArtificateMasterLbrary EtmfArtificateMasterLbrary { get; set; }
        public ProjectWorkplaceSection ProjectWorkplaceSection { get; set; }
        public List<ProjectWorkplaceArtificatedocument> ProjectWorkplaceArtificatedocument { get; set; }
    }
}
