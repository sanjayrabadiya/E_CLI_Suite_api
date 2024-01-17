using GSC.Data.Dto.Master;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Location;
using GSC.Data.Entities.SupplyManagement;
using GSC.Helper;
using GSC.Shared.DocumentService;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Dto.SupplyManagement
{
    public class SupplyManagementEmailConfigurationDto : BaseDto
    {
        public int ProjectId { get; set; }
        public int? SiteId { get; set; }
        public SupplyManagementEmailTriggers Triggers { get; set; }

        public SupplyManagementEmailRecurrenceType? RecurrenceType { get; set; }
        public int Days { get; set; }
        public bool IsActive { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
        public string EmailBody { get; set; }

        [NotMapped]
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

        public string TimeZone { get; set; }
        public string IpAddress { get; set; }

        public SupplyManagementEmailRecurrenceType? RecurrenceType { get; set; }

        public string RecurrenceTypeName { get; set; }
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

        public int ThresholdValue { get; set; }

        public int RemainingKit { get; set; }

        public string RequestType { get; set; }

        public string RequestToSiteCode { get; set; }
        public string RequestToSiteName { get; set; }

        public string Country { get; set; }

        public string TypeOfKitReturn { get; set; }

        public int NoOfKitReturn { get; set; }

        public string Treatment { get; set; }

        public string ReasonForUnblind { get; set; }
        public DateTime UnblindDatetime { get; set; }
        public string UnblindBy { get; set; }

        public string RequestFromSiteCode { get; set; }

        public string RequestFromSiteName { get; set; }

        public string ApprovedBy { get; set; }
        public string ApprovedOn { get; set; }




    }
    public class SupplyManagementEmailConfigurationDetailHistoryGridDto : BaseAuditDto
    {
        public int SupplyManagementEmailConfigurationDetailId { get; set; }
        public int RoleId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string RoleName { get; set; }

        public string TriggerName { get; set; }
    }
}
