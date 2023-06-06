using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Helper;

namespace GSC.Data.Dto.Master
{
    public class VendorManagementDto : BaseDto
    {
        [Required(ErrorMessage = "Company Name is required.")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Service Type is required.")]
        public string ServiceType { get; set; }

        [Required(ErrorMessage = "Contact No is required.")]
        public string ContactNo { get; set; }

        [Required(ErrorMessage = "Registered Office Address is required.")]
        public string RegOfficeAddress { get; set; }
        public string BranchOfficeDetails { get; set; }

        [Required(ErrorMessage = "Selecting Audit is required.")]
        public VendorManagementAudit? VendorManagementAuditId { get; set; }
    }

    public class VendorManagementGridDto : BaseAuditDto
    {
        public string ServiceType { get; set; }
        public string ContactNo { get; set; }
        public string RegOfficeAddress { get; set; }
        public string BranchOfficeDetails { get; set; }
        public VendorManagementAudit? VendorManagementAuditId { get; set; }
    }
}