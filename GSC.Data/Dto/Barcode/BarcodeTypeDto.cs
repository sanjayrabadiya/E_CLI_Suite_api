using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Barcode
{
    public class BarcodeTypeDto : BaseDto
    {
        [Required(ErrorMessage = "Barcode Type name is required.")]
        public string BarcodeTypeName { get; set; }

        public int? CompanyId { get; set; }
    }
}