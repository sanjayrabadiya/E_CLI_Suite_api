using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Screening
{
    public class ScreeningTemplateValueChildDto : BaseDto
    {
        public int ScreeningTemplateValueId { get; set; }
        public int ProjectDesignVariableValueId { get; set; }
        public string Value { get; set; }
    }

    public class ScreeningTemplateValueChildBasic
    {
        public int Id { get; set; }
    }

}