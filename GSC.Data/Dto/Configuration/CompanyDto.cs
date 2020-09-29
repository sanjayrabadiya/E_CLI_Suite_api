using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Helper.DocumentService;

namespace GSC.Data.Dto.Configuration
{
    public class CompanyDto : BaseDto
    {
        [Required(ErrorMessage = "Company Code is required.")]
        public string CompanyCode { get; set; }

        [Required(ErrorMessage = "Company Name is required.")]
        public string CompanyName { get; set; }

        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public Entities.Location.Location Location { get; set; }
        public string Logo { get; set; }
        public string LogoPath { get; set; }
        public FileModel FileModel { get; set; }
    }

    public class CompanyGridDto : BaseAuditDto
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
}