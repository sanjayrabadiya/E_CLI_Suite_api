using GSC.Common.Base;
using System;
using System.Collections.Generic;
using GSC.Common.Common;

namespace GSC.Data.Entities.Etmf
{
    public class ProjectWorkplaceSection : BaseEntity, ICommonAduit
    {
        public int ProjectWorkPlaceZoneId { get; set; }

        public int EtmfSectionMasterLibraryId { get; set; }
        public List<ProjectWorkplaceArtificate> ProjectWorkplaceArtificate { get; set; }
        public List<ProjectWorkplaceSubSection> ProjectWorkplaceSubSection { get; set; }

        public int? CompanyId { get; set; }
        public EtmfSectionMasterLibrary EtmfSectionMasterLibrary { get; set; }
        public ProjectWorkPlaceZone ProjectWorkPlaceZone { get; set; }
    }
}
