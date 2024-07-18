using GSC.Data.Entities.Common;
using GSC.Shared.DocumentService;
using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.Master
{
    public class SiteContractDto : BaseDto
    {
        [Required(ErrorMessage = "Project is required.")]
        public int ProjectId { get; set; }
        [Required(ErrorMessage = "Site is required.")]
        public int SiteId { get; set; }
        public int? CountryId { get; set; }

        [StringLength(300, ErrorMessage = "Remark Maximum 300 characters exceeded")]
        public string Remark { get; set; }
        public string ContractCode { get; set; }
        public string ContractFileName { get; set; }
        public string ContractDocumentPath { get; set; }
        public FileModel ContractFileModel { get; set; }
        public bool IsDocument { get; set; }
        public int? ContractTemplateFormatId { get; set; }
        public string FormatBody { get; set; }
    }
    public class SiteContractGridDto : BaseAuditDto
    {
        public int  ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int? CountryId { get; set; }
        public string CountryName { get; set; }
        public string ContractCode { get; set; }
        public string Remark { get; set; }
        public string ContractFileName { get; set; }
        public string ContractDocumentPath { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
    }
}
