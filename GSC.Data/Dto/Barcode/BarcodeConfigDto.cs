using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Barcode
{
    public class BarcodeConfigDto : BaseDto
    {
        [Required(ErrorMessage = "Barcode Type is required.")]
        public int BarcodeTypeId { get; set; }

        public string BarcodeTypeName { get; set; }
        //[Required(ErrorMessage = "Barcode Size is required.")]
        //public int BarcodeSizeId { get; set; }

        public bool SubjectNo { get; set; }
        public bool ProjectNo { get; set; }
        public bool Period { get; set; }
        public bool VolunteerId { get; set; }
        public bool RandomizationNo { get; set; }
        public int BarcodeFor { get; set; }
        public string BarcodeForName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool DisplayValue { get; set; }
        public int FontSize { get; set; }
        public int TextMargin { get; set; }
        public int MarginTop { get; set; }
        public int MarginBottom { get; set; }
        public int MarginLeft { get; set; }
        public int MarginRight { get; set; }
        public int? CompanyId { get; set; }
    }
}