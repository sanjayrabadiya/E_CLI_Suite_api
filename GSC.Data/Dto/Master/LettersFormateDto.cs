using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Master
{
    public class LettersFormateDto : BaseDto
    {       
        public string LetterCode { get; set; }
        [Required(ErrorMessage = "Letter Name is required.")]
        public string LetterName { get; set; }
        public string Description { get; set; }
        [Required(ErrorMessage = "Letter Body is required.")]
        public string LetterBody { get; set; }
    }
    public class LettersFormateGridDto : BaseAuditDto
    {
        public string LetterCode { get; set; }
        public string LetterName { get; set; }
        public string Description { get; set; }
        public string LetterBody { get; set; }


    }
}
