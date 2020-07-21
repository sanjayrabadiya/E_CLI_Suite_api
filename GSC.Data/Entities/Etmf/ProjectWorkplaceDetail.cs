﻿using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Etmf
{
    public class ProjectWorkplaceDetail : BaseEntity
    {
        public int ProjectWorkplaceId { get; set; }

        public int WorkPlaceFolderId { get; set; }
        public int ItemId{ get; set; }
        public string ItemName { get; set; }

        public List<ProjectWorkPlaceZone> ProjectWorkPlaceZone { get; set; }

    }
}
