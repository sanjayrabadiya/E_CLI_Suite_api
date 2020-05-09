using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Client
{
    public class ClientContactDto : BaseDto
    {
        public int ClientId { get; set; }

        [Required(ErrorMessage = "Contact Type is required.")]
        public int ContactTypeId { get; set; }

        [Required(ErrorMessage = "Contact Number is required.")]
        public string ContactNo { get; set; }

        public string ContactName { get; set; }

        public bool IsDefault { get; set; }

        public string ContactTypeName { get; set; }
    }
}