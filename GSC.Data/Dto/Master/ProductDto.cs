using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;

namespace GSC.Data.Dto.Master
{
    public class ProductDto : BaseDto
    {
        [Required(ErrorMessage = "Product Code is required.")]
        public string ProductCode { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Project Number is required.")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Product Type is required.")]
        public int ProductTypeId { get; set; }

        public int? CompanyId { get; set; }

        public ProductType ProductType { get; set; }
        public Entities.Master.Project Project { get; set; }
    }
}