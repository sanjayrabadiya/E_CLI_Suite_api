using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Master
{
    public class CurrencyDto : BaseDto
    {
        [Required(ErrorMessage = "CurrencyName is required.")]
        public string CurrencyName { get; set; }

        [Required]
        public int CountryId { get; set; }
        public string CurrencySymbol { get; set; }  
    }

    public class CurrencyGridDto : BaseAuditDto
    {
        public string CurrencyName { get; set; }
        public string Country { get; set; }
        public string CurrencySymbol { get; set; }
       
    }
}
