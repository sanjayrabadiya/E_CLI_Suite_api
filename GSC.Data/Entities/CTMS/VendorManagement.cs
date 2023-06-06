using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Helper;

namespace GSC.Data.Entities.Master
{
    public class VendorManagement : BaseEntity, ICommonAduit
    {
        public string CompanyName { get; set; }
        public string ServiceType { get; set; }
        public string ContactNo { get; set; }
        public string RegOfficeAddress { get; set; }
        public string BranchOfficeDetails { get; set; }
        public VendorManagementAudit? VendorManagementAuditId { get; set; }
    }
}