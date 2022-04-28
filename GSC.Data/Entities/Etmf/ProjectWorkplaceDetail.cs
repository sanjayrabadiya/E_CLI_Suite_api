using GSC.Common.Base;
using System;
using System.Collections.Generic;
using GSC.Common.Common;
using GSC.Helper;

namespace GSC.Data.Entities.Etmf
{
    public class ProjectWorkplaceDetail : BaseEntity, ICommonAduit
    {
        public int ProjectWorkplaceId { get; set; }

        public int WorkPlaceFolderId { get; set; }
        public int ItemId{ get; set; }
        public string ItemName { get; set; }
        public ProjectWorkplace ProjectWorkplace { get; set; }
        public List<ProjectWorkPlaceZone> ProjectWorkPlaceZone { get; set; }
        public List<EtmfUserPermission> EtmfUserPermission { get; set; }

    }
}
