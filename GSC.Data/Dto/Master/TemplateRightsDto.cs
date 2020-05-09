using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;

namespace GSC.Data.Dto.Master
{
    public class TemplateRightsDto : BaseDto
    {
        [Required(ErrorMessage = "Variable Code is required.")]
        public string TemplateCode { get; set; }

        public int VariableTemplateId { get; set; }
        public string RoleId { get; set; }

        public VariableTemplate VariableTemplate { get; set; }
        //public SecurityRole SecurityRole { get; set; }
    }
}