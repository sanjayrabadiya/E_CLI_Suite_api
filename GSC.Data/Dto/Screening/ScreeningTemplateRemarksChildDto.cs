using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Screening
{
    public class ScreeningTemplateRemarksChildDto : BaseDto
    {
        public int ScreeningTemplateValueId { get; set; }
        public int ProjectDesignVariableRemarksId { get; set; }
        public int Range { get; set; }
        public string Remarks { get; set; }
    }

    public class ScreeningTemplateRemarksChildBasic
    {
        public int Id { get; set; }
    }
}
