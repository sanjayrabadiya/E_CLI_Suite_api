using GSC.Data.Entities.Common;
using GSC.Helper;
using GSC.Shared.DocumentService;
using System.Collections.Generic;

namespace GSC.Data.Dto.SupplyManagement
{
    public class SupplyManagementUploadFileDto : BaseDto
    {
        public int ProjectId { get; set; }
        public int? CountryId { get; set; }
        public int? SiteId { get; set; }
        public SupplyManagementUploadFileLevel SupplyManagementUploadFileLevel { get; set; }
        public FileModel FileModel { get; set; }
        public LabManagementUploadStatus Status { get; set; }
        public string FileName { get; set; }
        public string PathName { get; set; }
        public string MimeType { get; set; }
        public bool IsApprove { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
        public IList<SupplyManagementUploadFileDetailDto> Details { get; set; } = null;
    }

    public class SupplyManagementUploadFileGridDto : BaseAuditDto
    {
        public string StudyCode { get; set; }
        public string Country { get; set; }
        public string SiteCode { get; set; }
        public string Level { get; set; }
        public bool IsApprove { get; set; }
        public string Reason { get; set; }
        public string ReasonOth { get; set; }
        public string StatusName { get; set; }
        public LabManagementUploadStatus Status { get; set; }

        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
    }
}
