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
}