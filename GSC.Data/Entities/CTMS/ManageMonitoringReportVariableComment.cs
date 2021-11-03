using System.ComponentModel.DataAnnotations.Schema;
using GSC.Common.Base;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.UserMgt;

namespace GSC.Data.Entities.CTMS
{
    public class ManageMonitoringReportVariableComment : BaseEntity
    {
        public int ManageMonitoringReportVariableId { get; set; }
        public int? RoleId { get; set; }
        public string Comment { get; set; }
        public ManageMonitoringReportVariable ManageMonitoringReportVariable { get; set; }

        [ForeignKey("RoleId")] public SecurityRole Role { get; set; }
    }
}