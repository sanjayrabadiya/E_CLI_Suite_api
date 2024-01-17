
using DocumentFormat.OpenXml.Office2010.Excel;
using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Location;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementThresholdHistory : BaseEntity
    {
        public int ProjectId { get; set; }
        public int SiteId { get; set; }
    }
}
