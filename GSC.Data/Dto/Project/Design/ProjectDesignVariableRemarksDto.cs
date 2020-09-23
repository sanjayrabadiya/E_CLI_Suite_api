using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Project.Design
{
    public class ProjectDesignVariableRemarksDto : BaseDto
    {
        public int ProjectDesignVariableId { get; set; }

        [Required(ErrorMessage = "Range is required.")]
        public int Range { get; set; }

        [Required(ErrorMessage = "Remarks is required.")]
        public string Remarks { get; set; }

        public string ScreeningValue { get; set; }
        public int ScreeningTemplateValueChildId { get; set; }
        public string ScreeningValueOld { get; set; }
    }

    public class ScreeningVariableRemarksDto
    {
        public int Id { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public int Range { get; set; }
        public string Remarks { get; set; }
        public string ScreeningValue { get; set; }
        public int ScreeningTemplateValueChildId { get; set; }
        public string ScreeningValueOld { get; set; }
    }

    public class ProjectDesignVariableRemarksDropDown
    {
        public int Id { get; set; }
        public string ValueCode { get; set; }
        public string ValueName { get; set; }
    }
}
