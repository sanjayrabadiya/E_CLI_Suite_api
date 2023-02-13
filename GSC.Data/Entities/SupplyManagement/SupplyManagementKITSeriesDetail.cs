
using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.Location;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementKITSeriesDetail : BaseEntity
    {
        public int SupplyManagementKITSeriesId { get; set; }
        public int ProjectDesignVisitId { get; set; }
        public int PharmacyStudyProductTypeId { get; set; }
        public int? RandomizationId { get; set; }
        public int NoOfImp { get; set; }
        public int NoofPatient { get; set; }
        public int TotalUnits { get; set; }
        public bool? IsUnUsed { get; set; }
        public SupplyManagementKITSeries SupplyManagementKITSeries { get; set; }
        public ProjectDesignVisit ProjectDesignVisit { get; set; }
        public Randomization Randomization { get; set; }
        public PharmacyStudyProductType PharmacyStudyProductType { get; set; }

    }
}
