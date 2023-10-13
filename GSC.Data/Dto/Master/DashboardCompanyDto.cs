using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Shared.DocumentService;

namespace GSC.Data.Dto.Configuration
{
    public class DashboardCompanyGridDto : BaseAuditDto
    {
        public string CompanyCode { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Location { get; set; }
        public string Logo { get; set; }
        public string LogoPath { get; set; }
        public string CountryName { get; set; }
        public string StateName { get; set; }
        public string CityName { get; set; }
        public string Address { get; set; }
    }
    public class DashboardStudyGridDto : BaseAuditDto
    {
        public string projectName { get; set; }
        public string Status { get; set; }
        public string Country { get; set; }
    }
}