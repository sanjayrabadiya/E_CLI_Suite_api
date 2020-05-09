using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.Master
{
    public class VariableTemplateRightDto
    {
        [Required(ErrorMessage = "Security Role is required.")]
        public int SecurityRoleId { get; set; }

        public List<int> VariableTemplateIds { get; set; }
    }
}