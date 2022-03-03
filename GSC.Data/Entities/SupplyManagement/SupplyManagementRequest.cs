﻿using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Location;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementRequest : BaseEntity, ICommonAduit
    {

        public bool IsSiteRequest { get; set; }
        public int? FromProjectId { get; set; }
        public int? ToProjectId { get; set; }
        public int StudyProductTypeId { get; set; }
        public int RequestQty { get; set; }

        [ForeignKey("FromProjectId")]
        public GSC.Data.Entities.Master.Project FromProject { get; set; }

        [ForeignKey("ToProjectId")]
        public GSC.Data.Entities.Master.Project ToProject { get; set; }

        [ForeignKey("StudyProductTypeId")]
        public PharmacyStudyProductType PharmacyStudyProductType { get; set; }

    }   
}