using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Configuration
{
    public class UploadSettingDto : BaseDto
    {
        [Required(ErrorMessage = "Image Path is required.")]
        public string ImagePath { get; set; }

        [Required(ErrorMessage = "Document Path is required.")]
        public string DocumentPath { get; set; }

        [Required(ErrorMessage = "Company is required.")]
        public int CompanyId { get; set; }

        [Required(ErrorMessage = "Image url is required.")]
        public string ImageUrl { get; set; }

        [Required(ErrorMessage = "Document Url is required.")]
        public string DocumentUrl { get; set; }
    }
}