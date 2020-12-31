using GSC.Helper;
using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Entities.Screening
{
    public class ScreeningTemplateEditCheckValue
    {
        [Key]
        public int Id { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public int ScreeningTemplateId { get; set; }
        public int EditCheckDetailId { get; set; }
        public string EditCheckRefValue { get; set; }
        public EditCheckValidateType ValidateType { get; set; }
    }
}
