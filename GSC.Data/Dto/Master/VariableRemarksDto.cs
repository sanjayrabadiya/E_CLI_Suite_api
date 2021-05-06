using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Master
{
    public class VariableRemarksDto:BaseDto
    {
        [Required(ErrorMessage = "Range is required.")]
        public string Range { get; set; }

        public string Remark{ get; set; }

    }
}
