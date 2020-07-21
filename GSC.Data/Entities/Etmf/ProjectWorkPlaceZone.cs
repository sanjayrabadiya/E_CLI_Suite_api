using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Etmf
{
    public class ProjectWorkPlaceZone : BaseEntity
    {

        public int ProjectWorkplaceDetailId{ get; set; }
        public int EtmfZoneMasterLibraryId{ get; set; }

        public List<ProjectWorkplaceSection> ProjectWorkplaceSection { get; set; }

        public EtmfZoneMasterLibrary EtmfZoneMasterLibrary { get; set; }

        public int? CompanyId { get; set; }
    }
}
