using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Master
{
    public class VariableTemplateNoteDto : BaseDto
    {
        [Required(ErrorMessage = "Template is required.")]
        public int VariableTemplateId { get; set; }

        [Required(ErrorMessage = "Note is required.")]
        public string Note { get; set; }

        public bool IsPreview { get; set; }
    }
}