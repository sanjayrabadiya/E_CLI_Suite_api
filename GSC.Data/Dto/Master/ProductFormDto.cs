using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Master
{
    public class ProductFormDto : BaseDto
    {
        [Required(ErrorMessage = "Form Name is required.")]
        public string FormName { get; set; }
    }
}