using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Etmf
{
    public class ProjectWorkplaceSection : BaseEntity
    {
        public int ProjectWorkPlaceZoneId { get; set; }

        public int EtmfSectionMasterLibraryId { get; set; }
        public List<ProjectWorkplaceArtificate> ProjectWorkplaceArtificate { get; set; }
        public int? CompanyId { get; set; }
        public EtmfSectionMasterLibrary EtmfSectionMasterLibrary { get; set; }
    }
}
