using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Pharmacy
{
    public class PharmacyVerificationTemplateValueChildDto : BaseDto
    {
        public int PharmacyVerificationTemplateValueId { get; set; }
        public int VariableValueId { get; set; }
        public string Value { get; set; }
    }
}