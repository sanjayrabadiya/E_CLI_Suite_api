using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Pharmacy
{
    public class PharmacyTemplateValueChildDto : BaseDto
    {
        public int PharmacyTemplateValueId { get; set; }
        public int VariableValueId { get; set; }
        public string Value { get; set; }
    }
}