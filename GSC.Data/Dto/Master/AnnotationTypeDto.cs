using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Master
{
    public class AnnotationTypeDto : BaseDto
    {
        [Required(ErrorMessage = "Annotatione Code is required.")]
        public string AnnotationeCode { get; set; }

        [Required(ErrorMessage = "Annotatione Name is required.")]
        public string AnnotationeName { get; set; }

        public int? CompanyId { get; set; }
    }
}