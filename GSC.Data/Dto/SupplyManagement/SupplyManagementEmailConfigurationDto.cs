using GSC.Data.Dto.Master;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Location;
using GSC.Data.Entities.SupplyManagement;
using GSC.Helper;
using GSC.Shared.DocumentService;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Dto.SupplyManagement
{
    public class SupplyManagementEmailConfigurationDto : BaseDto
    {
        public int ProjectId { get; set; }
        public int? SiteId { get; set; }
        public SupplyManagementEmailTriggers Triggers { get; set; }
        public int Days { get; set; }
        public bool IsActive { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
        public string EmailBody { get; set; }
        public IList<SupplyManagementEmailConfigurationDetail> SupplyManagementEmailConfigurationDetail { get; set; } = null;



    }

    public class SupplyManagementEmailConfigurationGridDto : BaseAuditDto
    {
        public int ProjectId { get; set; }
        public int? SiteId { get; set; }
        public string TriggersName { get; set; }
        public string ProjectCode { get; set; }
        public string SiteCode { get; set; }
        public int Days { get; set; }
        public bool IsActive { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
        public string Roles { get; set; }
        public string Users { get; set; }
        public string Reason { get; set; }
        public string EmailBody { get; set; }

        public IList<SupplyManagementEmailConfigurationDetail> SupplyManagementEmailConfigurationDetail { get; set; }


    }
    public class SupplyManagementEmailConfigurationDetailGridDto : BaseAuditDto
    {
        public int SupplyManagementEmailConfigurationId { get; set; }
        public int RoleId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string RoleName { get; set; }
    }

    public class IWRSEmailModel
    {
        public string StudyCode { get; set; }
        public string SiteCode { get; set; }
        public string SiteName { get; set; }
        public string ProductType { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public string ActionBy { get; set; }

        public string RequestedBy
        {
            get; set;
        }

        public string Visit { get; set; }

        public string ScreeningNo { get; set; }

        public string RandomizationNo { get; set; }

        public string KitNo { get; set; }

        public int RequestedQty { get; set; }

        public int ApprovedQty { get; set; }

        public string RequestType { get; set; }


    }
}
