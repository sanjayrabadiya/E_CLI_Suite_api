using GSC.Common.Base;
using GSC.Data.Entities.Project.Design;

namespace GSC.Data.Entities.Screening
{
    public class ScreeningTemplateRemarksChild : BaseEntity
    {
        public int ScreeningTemplateValueId { get; set; }
        public int ProjectDesignVariableRemarksId { get; set; }
        public string Remarks { get; set; }
        public ProjectDesignVariableRemarks ProjectDesignVariableRemarks { get; set; }
    }
}
