
using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Location;
using GSC.Data.Entities.Master;
using GSC.Helper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementUploadFile : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public int? CountryId { get; set; }
        public int? SiteId { get; set; }
        public SupplyManagementUploadFileLevel SupplyManagementUploadFileLevel { get; set; }
        public string FileName { get; set; }
        public string PathName { get; set; }
        public string MimeType { get; set; }
        public LabManagementUploadStatus Status { get; set; }
        public bool IsApprove { get; set; }
        public Entities.Master.Project Project { get; set; }
        public Country Country { get; set; }
        [ForeignKey("SiteId")]
        public Entities.Master.Project Site { get; set; }
        public int? AuditReasonId { get; set; }
        public AuditReason AuditReason { get; set; }
        public string ReasonOth { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
        public IList<SupplyManagementUploadFileDetail> Details { get; set; } = null;
    }
}
